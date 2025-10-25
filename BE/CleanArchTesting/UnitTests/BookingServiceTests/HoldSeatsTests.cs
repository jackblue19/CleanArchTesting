// UnitTests/BookingServiceTests/HoldSeatsTests.cs
using Application.DTOs;
using Application.Interfaces;
using Application.UseCases;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace UnitTests.BookingServiceTests;

public class HoldSeatsTests
{
    private static Mock<IClock> ClockAt(DateTime when)
    {
        var m = new Mock<IClock>();
        m.Setup(x => x.UtcNow).Returns(when);
        return m;
    }

    [Fact]
    public async Task HoldSeats_Fails_When_Seats_Not_Same_Screen()
    {
        var db = new Mock<ICinemaDbContext>();
        var show = new Show { ShowId = 1, ScreenId = 10, BasePrice = 100 };
        db.Setup(x => x.Shows.FindAsync(It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(show);

        var seats = new List<Seat>
        {
            new() { SeatId=1, ScreenId=10, RowLabel="A", SeatNumber=1, SeatType="STANDARD" },
            new() { SeatId=2, ScreenId=11, RowLabel="A", SeatNumber=2, SeatType="STANDARD" },
        };
        db.Setup(x => x.Seats).ReturnsDbSet(seats);

        var repo = new Mock<IReservationRepository>();
        repo.Setup(r => r.GetActiveByShowAndSeatAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation?)null);

        db.Setup(x => x.Reservations).ReturnsDbSet(new List<Reservation>());
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var svc = new BookingService(db.Object, repo.Object, Mock.Of<IVoucherService>(), ClockAt(DateTime.UtcNow).Object);

        var result = await svc.HoldSeatsAsync(new HoldSeatsRequest(1, 1, new long[]{1,2}, null), default);
        result.Success.Should().BeFalse();
        result.Status.Should().Be("INVALID");
    }

    [Fact]
    public async Task HoldSeats_Succeeds_When_All_Valid()
    {
        var now = new DateTime(2024,1,1,0,0,0, DateTimeKind.Utc);
        var db = new Mock<ICinemaDbContext>();
        var show = new Show { ShowId = 1, ScreenId = 10, BasePrice = 100 };
        db.Setup(x => x.Shows.FindAsync(It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(show);

        var seats = new List<Seat>
        {
            new() { SeatId=1, ScreenId=10, RowLabel="A", SeatNumber=1, SeatType="STANDARD" },
            new() { SeatId=2, ScreenId=10, RowLabel="A", SeatNumber=2, SeatType="STANDARD" },
        };
        db.Setup(x => x.Seats).ReturnsDbSet(seats);

        var repo = new Mock<IReservationRepository>();
        repo.Setup(r => r.GetActiveByShowAndSeatAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation?)null);

        var reservations = new List<Reservation>();
        db.Setup(x => x.Reservations).ReturnsDbSet(reservations);
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var svc = new BookingService(db.Object, repo.Object, Mock.Of<IVoucherService>(), ClockAt(now).Object);

        var result = await svc.HoldSeatsAsync(new HoldSeatsRequest(1, 1, new long[]{1,2}, "idem-1"), default);
        result.Success.Should().BeTrue();
        reservations.Should().HaveCount(2);
        reservations.All(r => r.Status=="HELD").Should().BeTrue();
        reservations.All(r => r.HoldExpiresAtUtc==now.AddMinutes(5)).Should().BeTrue();
    }
}
