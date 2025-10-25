using System;
using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace UnitTests.Domain;

public class ReservationTests
{
    private static Reservation CreateReservation(string status = "HELD", decimal subtotal = 50m, decimal discount = 5m, DateTime? holdExpiry = null)
    {
        return new Reservation
        {
            ReservationId = 1,
            ShowId = 1,
            SeatId = 1,
            UserId = 1,
            Status = status,
            CreatedAtUtc = DateTime.UtcNow,
            HoldExpiresAtUtc = holdExpiry,
            Subtotal = subtotal,
            Discount = discount,
            Total = null,
            PaymentIntentId = null,
            IdempotencyKey = null,
            RowVersion = Array.Empty<byte>()
        };
    }

    [Fact]
    public void HoldHasExpired_ReturnsTrue_WhenPastExpiry()
    {
        var reservation = CreateReservation(holdExpiry: DateTime.UtcNow.AddMinutes(-1));
        reservation.HoldHasExpired(DateTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void HoldIsActive_WhenNotExpired()
    {
        var reservation = CreateReservation(holdExpiry: DateTime.UtcNow.AddMinutes(10));
        reservation.IsHoldActive(DateTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void HoldIsNotActive_WhenExpired()
    {
        var reservation = CreateReservation(holdExpiry: DateTime.UtcNow.AddMinutes(-10));
        reservation.IsHoldActive(DateTime.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void IsBooked_WhenStatusMatches()
    {
        var reservation = CreateReservation(status: "BOOKED", holdExpiry: null);
        reservation.IsBooked().Should().BeTrue();
    }

    [Fact]
    public void ApplyCalculatedTotal_SetsTotalToAmountDue()
    {
        var reservation = CreateReservation(subtotal: 40m, discount: 10m);
        reservation.ApplyCalculatedTotal();
        reservation.Total.Should().Be(30m);
    }
}
