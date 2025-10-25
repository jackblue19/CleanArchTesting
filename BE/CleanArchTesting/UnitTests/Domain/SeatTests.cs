using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace UnitTests.Domain;

public class SeatTests
{
    [Fact]
    public void SeatLabel_CombinesRowAndNumber()
    {
        var seat = new Seat
        {
            SeatId = 1,
            ScreenId = 1,
            RowLabel = "C",
            SeatNumber = 9,
            SeatType = "STANDARD"
        };

        seat.SeatLabel.Should().Be("C9");
    }
}
