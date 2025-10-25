// UnitTests/BookingServiceTests/HoldSeats_ConflictAndCoupleTests.cs
using Application.DTOs;
using Application.Interfaces;
using Application.UseCases;
using Domain.Entities;
using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;

namespace UnitTests.BookingServiceTests;

public class HoldSeats_ConflictAndCoupleTests
{
    [Fact]
    public async Task HoldSeats_Fails_When_Seat_Already_Active()
    {
        var db = new Mock<ICinemaDbContext>();
        db.Setup(x => x.Shows.FindAsync(It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(new Show{ ShowId=1, ScreenId=10, BasePrice=100 });
        db.Setup(x => x.Seats).ReturnsDbSet(new List<Seat>{ new(){ SeatId=1, ScreenId=10, RowLabel="A", SeatNumber=1, SeatType="STANDARD" } });

        var repo = new Mock<IReservationRepository>();
        repo.Setup(r => r.GetActiveByShowAndSeatAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Reservation{ ReservationId=123, ShowId=1, SeatId=1, Status="HELD", HoldExpiresAtUtc=DateTime.UtcNow.AddMinutes(3)});

        var svc = new BookingService(db.Object, repo.Object, Mock.Of<IVoucherService>(), Mock.Of<IClock>());
        var res = await svc.HoldSeatsAsync(new HoldSeatsRequest(1, 1, new long[]{1}, null), default);
        res.Success.Should().BeFalse();
        res.Status.Should().Be("CONFLICT");
        res.ReservationId.Should().Be(123);
    }

    [Fact]
    public async Task HoldSeats_Fails_When_CoupleSeats_NotPaired()
    {
        var db = new Mock<ICinemaDbContext>();
        db.Setup(x => x.Shows.FindAsync(It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(new Show{ ShowId=1, ScreenId=10, BasePrice=100 });
        var seats = new List<Seat>{
            new(){ SeatId=1, ScreenId=10, RowLabel="C", SeatNumber=5, SeatType="COUPLE_LEFT", SeatPairGroupId=100 },
            new(){ SeatId=2, ScreenId=10, RowLabel="C", SeatNumber=6, SeatType="COUPLE_RIGHT", SeatPairGroupId=100 },
        };
        db.Setup(x => x.Seats).ReturnsDbSet(seats);
        var repo = new Mock<IReservationRepository>();
        repo.Setup(r => r.GetActiveByShowAndSeatAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation?)null);

        var svc = new BookingService(db.Object, repo.Object, Mock.Of<IVoucherService>(), Mock.Of<IClock>());
        var res = await svc.HoldSeatsAsync(new HoldSeatsRequest(1, 1, new long[]{1}, null), default);
        res.Success.Should().BeFalse();
        res.Status.Should().Be("INVALID");
    }
}
