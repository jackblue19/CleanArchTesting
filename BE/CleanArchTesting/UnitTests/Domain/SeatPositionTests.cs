// UnitTests/Domain/SeatPositionTests.cs
using System;
using Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace UnitTests.Domain;

public class SeatPositionTests
{
    [Fact]
    public void SeatPosition_Valid_ToString()
    {
        var s = new SeatPosition("B", 7);
        s.ToString().Should().Be("B7");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void SeatPosition_Invalid_SeatNumber_Throws(int n)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SeatPosition("A", n));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void SeatPosition_Invalid_RowLabel_Throws(string row)
    {
        Assert.Throws<ArgumentException>(() => new SeatPosition(row, 1));
    }
}
