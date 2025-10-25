using System;
using Domain.DomainEvents;
using FluentAssertions;
using Xunit;

namespace UnitTests.Domain;

public class ReservationEventsTests
{
    private static readonly long[] SeatIds = { 1, 2, 3 };

    [Fact]
    public void SeatsHeldEvent_ComputesSeatCountAndInclusion()
    {
        var evt = new SeatsHeldEvent(1, SeatIds, 42, DateTime.UtcNow.AddMinutes(5));
        evt.SeatCount.Should().Be(3);
        evt.IncludesSeat(2).Should().BeTrue();
        evt.BelongsToUser(42).Should().BeTrue();
    }

    [Fact]
    public void SeatsHeldEvent_HasExpired_WhenNowPastExpiry()
    {
        var evt = new SeatsHeldEvent(1, SeatIds, 42, DateTime.UtcNow.AddMinutes(-1));
        evt.HasExpired(DateTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void SeatsHeldEvent_ToBookingEvent_ReturnsNewEvent()
    {
        var evt = new SeatsHeldEvent(1, SeatIds, 42, DateTime.UtcNow.AddMinutes(5));
        var booking = evt.ToBookingEvent(99);
        booking.ReservationId.Should().Be(99);
        booking.SeatCount.Should().Be(3);
    }

    [Fact]
    public void SeatsHeldEvent_ToBookingEvent_ThrowsWhenReservationInvalid()
    {
        var evt = new SeatsHeldEvent(1, SeatIds, 42, DateTime.UtcNow.AddMinutes(5));
        var action = () => evt.ToBookingEvent(0);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SeatsHeldEvent_ToReleaseEvent_ValidatesReason()
    {
        var evt = new SeatsHeldEvent(1, SeatIds, 42, DateTime.UtcNow.AddMinutes(5));
        var release = evt.ToReleaseEvent("timed out");
        release.Reason.Should().Be("timed out");
    }

    [Fact]
    public void SeatsBookedEvent_ToReleaseEvent_ValidatesReason()
    {
        var evt = new SeatsBookedEvent(1, SeatIds, 42, 55);
        evt.ToReleaseEvent("cancelled").Reason.Should().Be("cancelled");
    }

    [Fact]
    public void SeatsReleasedEvent_WasReleasedFor_IsCaseInsensitive()
    {
        var evt = new SeatsReleasedEvent(1, SeatIds, 42, "Timed Out");
        evt.WasReleasedFor("timed out").Should().BeTrue();
    }
}
