// UnitTests/BookingServiceTests/HoldSeats_MoreTests.cs
using Application.DTOs;
using Application.Interfaces;
using Application.UseCases;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace UnitTests.BookingServiceTests;

public class HoldSeats_MoreTests
{
    [Fact]
    public async Task HoldSeats_Fails_When_DuplicateSeatIds()
    {
        var db = new Mock<ICinemaDbContext>();
        var svc = new BookingService(db.Object, Mock.Of<IReservationRepository>(), Mock.Of<IVoucherService>(), Mock.Of<IClock>());
        var result = await svc.HoldSeatsAsync(new HoldSeatsRequest(1,1,new long[]{1,1},null), default);
        result.Success.Should().BeFalse();
        result.Status.Should().Be("INVALID");
    }

    [Fact]
    public async Task HoldSeats_Fails_When_Seat_NotFound()
    {
        var db = new Mock<ICinemaDbContext>();
        db.Setup(x => x.Shows.FindAsync(It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(new Show{ ShowId=1, ScreenId=10, BasePrice=100 });
        db.Setup(x => x.Seats).ReturnsDbSet(new List<Seat>{ new(){ SeatId=1, ScreenId=10, RowLabel="A", SeatNumber=1, SeatType="STANDARD" } });
        var svc = new BookingService(db.Object, Mock.Of<IReservationRepository>(), Mock.Of<IVoucherService>(), Mock.Of<IClock>());
        var result = await svc.HoldSeatsAsync(new HoldSeatsRequest(1,1,new long[]{1,2},null), default);
        result.Success.Should().BeFalse();
        result.Status.Should().Be("NOT_FOUND");
    }
}
