using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace UnitTests.Domain;

public class VShowSeatAvailabilityTests
{
    private static VShowSeatAvailability Create(string state = "FREE")
    {
        return new VShowSeatAvailability
        {
            ScreenId = 1,
            ShowId = 1,
            SeatId = 1,
            RowLabel = "B",
            SeatNumber = 7,
            SeatType = "STANDARD",
            SeatState = state
        };
    }

    [Fact]
    public void SeatLabel_ConcatenatesRowAndNumber()
    {
        var availability = Create();
        availability.SeatLabel.Should().Be("B7");
    }

    [Fact]
    public void IsInState_IsCaseInsensitive()
    {
        var availability = Create(state: "held");
        availability.IsInState("HELD").Should().BeTrue();
    }

    [Fact]
    public void IsFree_TrueOnlyForFreeState()
    {
        Create(state: "FREE").IsFree.Should().BeTrue();
        Create(state: "HELD").IsFree.Should().BeFalse();
    }
}
