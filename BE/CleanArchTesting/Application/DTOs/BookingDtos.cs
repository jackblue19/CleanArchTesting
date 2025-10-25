// Application/DTOs/BookingDtos.cs
namespace Application.DTOs;

public record HoldSeatsRequest(long ShowId, long UserId, IReadOnlyCollection<long> SeatIds, string? IdempotencyKey);
public record HoldSeatsResult(bool Success, string Status, string Message, long? ReservationId, DateTime? HoldExpiresAtUtc);

public record ConfirmBookingRequest(long ReservationId, string? VoucherCode, string? PaymentIntentId);
public record ConfirmBookingResult(bool Success, string Status, string Message, decimal Subtotal, decimal Discount, decimal Total);

public record ReleaseHoldRequest(long ReservationId, string Reason);
public record ReleaseHoldResult(bool Success, string Status, string Message);
