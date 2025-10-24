using Cinema.API.Models.Requests;
using Cinema.API.Models.Responses;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private const string StatusBooked = "BOOKED";
    private const string StatusHeld = "HELD";
    private readonly CinemaDbContext _dbContext;

    public BookingsController(CinemaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<ActionResult<BookingResponse>> CreateBooking([FromBody] BookingRequest request)
    {
        var show = await _dbContext.Shows
            .Include(s => s.Movie)
            .FirstOrDefaultAsync(s => s.ShowId == request.ShowId);
        if (show == null)
        {
            return NotFound($"Show {request.ShowId} was not found.");
        }

        var seat = await _dbContext.Seats.FirstOrDefaultAsync(s => s.SeatId == request.SeatId);
        if (seat == null)
        {
            return NotFound($"Seat {request.SeatId} was not found.");
        }

        if (seat.ScreenId != show.ScreenId)
        {
            return BadRequest("Seat does not belong to the screen for the requested show.");
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == request.UserId);
        if (user == null)
        {
            return NotFound($"User {request.UserId} was not found.");
        }

        var seatIsTaken = await _dbContext.Reservations.AnyAsync(r =>
            r.ShowId == show.ShowId &&
            r.SeatId == seat.SeatId &&
            (r.Status == StatusBooked || r.Status == StatusHeld));
        if (seatIsTaken)
        {
            return Conflict("Seat is already booked for this show.");
        }

        var reservation = new Reservation
        {
            ShowId = show.ShowId,
            SeatId = seat.SeatId,
            UserId = user.UserId,
            Status = StatusBooked,
            CreatedAtUtc = DateTime.UtcNow,
            HoldExpiresAtUtc = null,
            Subtotal = show.BasePrice,
            Discount = request.Discount ?? 0m,
            PaymentIntentId = request.PaymentIntentId,
            IdempotencyKey = request.IdempotencyKey
        };

        _dbContext.Reservations.Add(reservation);
        await _dbContext.SaveChangesAsync();
        await _dbContext.Entry(reservation).ReloadAsync();
        await _dbContext.Entry(reservation).Reference(r => r.Show).LoadAsync();
        await _dbContext.Entry(reservation).Reference(r => r.Seat).LoadAsync();
        await _dbContext.Entry(reservation).Reference(r => r.User).LoadAsync();
        if (reservation.Show != null)
        {
            await _dbContext.Entry(reservation.Show).Reference(s => s.Movie).LoadAsync();
        }

        var response = MapToResponse(reservation);
        return CreatedAtAction(nameof(GetBooking), new { reservationId = reservation.ReservationId }, response);
    }

    [HttpGet("{reservationId:long}")]
    public async Task<ActionResult<BookingResponse>> GetBooking(long reservationId)
    {
        var reservation = await _dbContext.Reservations
            .Include(r => r.Show)
                .ThenInclude(s => s.Movie)
            .Include(r => r.Seat)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.ReservationId == reservationId);
        if (reservation == null)
        {
            return NotFound();
        }

        return Ok(MapToResponse(reservation));
    }

    [HttpGet("shows/{showId:long}/available-seats")]
    public async Task<ActionResult<IEnumerable<SeatAvailabilityResponse>>> GetAvailableSeats(long showId)
    {
        var show = await _dbContext.Shows.FirstOrDefaultAsync(s => s.ShowId == showId);
        if (show == null)
        {
            return NotFound($"Show {showId} was not found.");
        }

        var unavailableSeatIds = await _dbContext.Reservations
            .Where(r => r.ShowId == showId && (r.Status == StatusBooked || r.Status == StatusHeld))
            .Select(r => r.SeatId)
            .ToListAsync();

        var availableSeats = await _dbContext.Seats
            .Where(s => s.ScreenId == show.ScreenId && !unavailableSeatIds.Contains(s.SeatId))
            .OrderBy(s => s.RowLabel)
            .ThenBy(s => s.SeatNumber)
            .ToListAsync();

        var response = availableSeats
            .Select(s => new SeatAvailabilityResponse(s.SeatId, s.RowLabel, s.SeatNumber, s.SeatType))
            .ToList();

        return Ok(response);
    }

    [HttpGet("users/{userId:long}")]
    public async Task<ActionResult<IEnumerable<BookingResponse>>> GetBookingsForUser(long userId)
    {
        var reservations = await _dbContext.Reservations
            .Where(r => r.UserId == userId)
            .Include(r => r.Show)
                .ThenInclude(s => s.Movie)
            .Include(r => r.Seat)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync();

        if (reservations.Count == 0)
        {
            var userExists = await _dbContext.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists)
            {
                return NotFound($"User {userId} was not found.");
            }
        }

        return Ok(reservations.Select(MapToResponse));
    }

    private static BookingResponse MapToResponse(Reservation reservation)
    {
        var total = reservation.Total ?? Math.Max(0m, reservation.Subtotal - reservation.Discount);
        var seatLabel = reservation.Seat != null
            ? $"{reservation.Seat.RowLabel}{reservation.Seat.SeatNumber}"
            : string.Empty;

        return new BookingResponse(
            reservation.ReservationId,
            reservation.Status,
            reservation.CreatedAtUtc,
            reservation.HoldExpiresAtUtc,
            reservation.Subtotal,
            reservation.Discount,
            total,
            reservation.ShowId,
            reservation.Show?.Movie?.Title ?? string.Empty,
            reservation.Show?.StartAtUtc ?? DateTime.MinValue,
            reservation.Show?.EndAtUtc ?? DateTime.MinValue,
            reservation.SeatId,
            seatLabel,
            reservation.Seat?.SeatType ?? string.Empty,
            reservation.UserId,
            reservation.User?.Email ?? string.Empty
        );
    }
}
