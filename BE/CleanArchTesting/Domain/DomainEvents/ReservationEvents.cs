// Domain/DomainEvents/ReservationEvents.cs
namespace Domain.DomainEvents;

public record SeatsHeldEvent(long ShowId, long[] SeatIds, long UserId, DateTime HoldExpiresAtUtc);
public record SeatsBookedEvent(long ShowId, long[] SeatIds, long UserId, long ReservationId);
public record SeatsReleasedEvent(long ShowId, long[] SeatIds, long UserId, string Reason);
