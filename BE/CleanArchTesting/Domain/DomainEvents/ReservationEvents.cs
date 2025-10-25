// Domain/DomainEvents/ReservationEvents.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.DomainEvents;

public record SeatsHeldEvent(long ShowId, long[] SeatIds, long UserId, DateTime HoldExpiresAtUtc)
{
    public int SeatCount => SeatIds?.Length ?? 0;

    public bool IncludesSeat(long seatId) => SeatIds?.Contains(seatId) ?? false;

    public bool BelongsToUser(long userId) => UserId == userId;

    public bool HasExpired(DateTime utcNow) => utcNow >= HoldExpiresAtUtc;

    public IReadOnlyCollection<long> AsReadOnlySeatIds() => Array.AsReadOnly(SeatIds ?? Array.Empty<long>());

    public SeatsBookedEvent ToBookingEvent(long reservationId)
    {
        if (reservationId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(reservationId), "Reservation id must be positive.");
        }

        return new SeatsBookedEvent(ShowId, SeatIds ?? Array.Empty<long>(), UserId, reservationId);
    }

    public SeatsReleasedEvent ToReleaseEvent(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Release reason must be provided.", nameof(reason));
        }

        return new SeatsReleasedEvent(ShowId, SeatIds ?? Array.Empty<long>(), UserId, reason);
    }
}

public record SeatsBookedEvent(long ShowId, long[] SeatIds, long UserId, long ReservationId)
{
    public int SeatCount => SeatIds?.Length ?? 0;

    public bool IncludesSeat(long seatId) => SeatIds?.Contains(seatId) ?? false;

    public bool BelongsToUser(long userId) => UserId == userId;

    public IReadOnlyCollection<long> AsReadOnlySeatIds() => Array.AsReadOnly(SeatIds ?? Array.Empty<long>());

    public SeatsReleasedEvent ToReleaseEvent(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Release reason must be provided.", nameof(reason));
        }

        return new SeatsReleasedEvent(ShowId, SeatIds ?? Array.Empty<long>(), UserId, reason);
    }
}

public record SeatsReleasedEvent(long ShowId, long[] SeatIds, long UserId, string Reason)
{
    public int SeatCount => SeatIds?.Length ?? 0;

    public bool IncludesSeat(long seatId) => SeatIds?.Contains(seatId) ?? false;

    public bool BelongsToUser(long userId) => UserId == userId;

    public bool WasReleasedFor(string reason) => string.Equals(Reason, reason, StringComparison.OrdinalIgnoreCase);

    public IReadOnlyCollection<long> AsReadOnlySeatIds() => Array.AsReadOnly(SeatIds ?? Array.Empty<long>());
}
