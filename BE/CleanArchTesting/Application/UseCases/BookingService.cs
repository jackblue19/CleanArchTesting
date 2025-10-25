// Application/UseCases/BookingService.cs
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases;

public interface IBookingService
{
    Task<HoldSeatsResult> HoldSeatsAsync(HoldSeatsRequest req, CancellationToken ct);
    Task<ConfirmBookingResult> ConfirmBookingAsync(ConfirmBookingRequest req, CancellationToken ct);
    Task<ReleaseHoldResult> ReleaseHoldAsync(ReleaseHoldRequest req, CancellationToken ct);
}

public class BookingService : IBookingService
{
    private readonly ICinemaDbContext _db;
    private readonly IReservationRepository _reservations;
    private readonly IVoucherService _voucherService;
    private readonly IClock _clock;

    public BookingService(ICinemaDbContext db, IReservationRepository reservations, IVoucherService voucherService, IClock clock)
    {
        _db = db; _reservations = reservations; _voucherService = voucherService; _clock = clock;
    }

    // Hold seats for a short period to avoid overbooking
    public async Task<HoldSeatsResult> HoldSeatsAsync(HoldSeatsRequest req, CancellationToken ct)
    {
        var v = Validators.HoldSeatsRequestValidator.Validate(req);
        if (!v.ok) return new(false, "INVALID", v.error!, null, null);

        // Verify show exists
        var show = await _db.Shows.FindAsync(new object?[] { req.ShowId }, ct);
        if (show == null) return new(false, "NOT_FOUND", "Show not found", null, null);

        // Check seats belong to the same screen as the show
        var seatInfos = await _db.Seats.Where(s => req.SeatIds.Contains(s.SeatId)).ToListAsync(ct);
        if (seatInfos.Count != req.SeatIds.Count) return new(false, "NOT_FOUND", "Some seats not found", null, null);
        if (seatInfos.Any(s => s.ScreenId != show.ScreenId)) return new(false, "INVALID", "Seats do not belong to the show's screen", null, null);

        // Enforce couple seats rule: both sides must be selected together
        var coupleGroups = seatInfos.Where(s => s.SeatType.StartsWith("COUPLE_", StringComparison.OrdinalIgnoreCase))
                                    .GroupBy(s => s.SeatPairGroupId)
                                    .Where(g => g.Key.HasValue);
        foreach (var g in coupleGroups)
        {
            var seatsInGroup = seatInfos.Where(s => s.SeatPairGroupId == g.Key).Select(s => s.SeatId).ToHashSet();
            var requestedInGroup = req.SeatIds.Where(id => seatsInGroup.Contains(id)).ToList();
            if (requestedInGroup.Count > 0 && requestedInGroup.Count != seatsInGroup.Count)
                return new(false, "INVALID", "Couple seats must be booked as a pair", null, null);
        }

        // Check availability using unique filtered index via probing existing active reservations
        foreach (var seatId in req.SeatIds)
        {
            var active = await _reservations.GetActiveByShowAndSeatAsync(req.ShowId, seatId, ct);
            if (active != null)
                return new(false, "CONFLICT", $"Seat {seatId} is already HELD/BOOKED", active.ReservationId, active.HoldExpiresAtUtc);
        }

        // Create one reservation per seat to match DB shape
        var now = _clock.UtcNow;
        var holdExpiry = now.AddMinutes(5); // basic hold window
        long? firstReservationId = null;
        foreach (var seatId in req.SeatIds)
        {
            var r = new Reservation
            {
                ShowId = req.ShowId,
                SeatId = seatId,
                UserId = req.UserId,
                Status = "HELD",
                CreatedAtUtc = now,
                HoldExpiresAtUtc = holdExpiry,
                Subtotal = 0,
                Discount = 0,
                PaymentIntentId = null,
                IdempotencyKey = req.IdempotencyKey
            };
            await _reservations.AddAsync(r, ct);
            if (firstReservationId == null) firstReservationId = r.ReservationId;
        }
        await _db.SaveChangesAsync(ct);

        return new(true, "HELD", "Seats held", firstReservationId, holdExpiry);
    }

    public async Task<ConfirmBookingResult> ConfirmBookingAsync(ConfirmBookingRequest req, CancellationToken ct)
    {
        var v = Validators.ConfirmBookingRequestValidator.Validate(req);
        if (!v.ok) return new(false, "INVALID", v.error!, 0, 0, 0);

        var r = await _reservations.GetByIdAsync(req.ReservationId, ct);
        if (r == null) return new(false, "NOT_FOUND", "Reservation not found", 0, 0, 0);

        if (r.Status == "BOOKED")
            return new(true, "BOOKED", "Already booked", r.Subtotal, r.Discount, r.Total ?? r.Subtotal - r.Discount);

        if (r.Status != "HELD") return new(false, "INVALID_STATE", "Reservation not HELD", 0, 0, 0);

        var now = _clock.UtcNow;
        if (r.HoldExpiresAtUtc is not null && r.HoldExpiresAtUtc <= now)
            return new(false, "EXPIRED", "Hold expired", 0, 0, 0);

        // Load data required for pricing without Include to remain test-friendly
        var show = await _db.Shows.FirstAsync(s => s.ShowId == r.ShowId, ct);
        var seat = await _db.Seats.FirstAsync(s => s.SeatId == r.SeatId, ct);
        var screen = await _db.Screens.FirstAsync(sc => sc.ScreenId == show.ScreenId, ct);
        var adjustments = await _db.PriceAdjustments.Where(a => a.ShowId == show.ShowId).ToListAsync(ct);

        decimal subtotal = show.BasePrice;
        foreach (var adj in adjustments)
        {
            var applies = adj.Target switch
            {
                "GLOBAL" => true,
                "SCREEN_TYPE" => string.Equals(adj.Key, screen.ScreenType, StringComparison.OrdinalIgnoreCase),
                "SEAT_TYPE" => string.Equals(adj.Key, seat.SeatType, StringComparison.OrdinalIgnoreCase),
                _ => false
            };
            if (!applies) continue;
            subtotal = adj.Mode.Equals("PERCENT", StringComparison.OrdinalIgnoreCase)
                ? subtotal + (subtotal * (adj.Amount / 100m))
                : subtotal + adj.Amount;
        }

        decimal discount = 0;
        if (!string.IsNullOrWhiteSpace(req.VoucherCode))
        {
            var (valid, d, reason) = await _voucherService.ValidateAndCalculateAsync(req.VoucherCode, subtotal, now, ct);
            if (!valid) return new(false, "VOUCHER_INVALID", reason ?? "Invalid voucher", 0, 0, 0);
            discount = d;
        }

        r.Subtotal = Math.Round(subtotal, 2);
        r.Discount = Math.Round(discount, 2);
        r.PaymentIntentId = req.PaymentIntentId;
        r.Status = "BOOKED";

        await _db.SaveChangesAsync(ct);
        var total = r.Total ?? r.Subtotal - r.Discount;
        return new(true, "BOOKED", "Reservation confirmed", r.Subtotal, r.Discount, total);
    }

    public async Task<ReleaseHoldResult> ReleaseHoldAsync(ReleaseHoldRequest req, CancellationToken ct)
    {
        var r = await _reservations.GetByIdAsync(req.ReservationId, ct);
        if (r == null) return new(false, "NOT_FOUND", "Reservation not found");
        if (r.Status != "HELD") return new(false, "INVALID_STATE", "Not a HELD reservation");
        r.Status = "RELEASED";
        r.HoldExpiresAtUtc = null;
        await _db.SaveChangesAsync(ct);
        return new(true, "RELEASED", req.Reason);
    }
}
