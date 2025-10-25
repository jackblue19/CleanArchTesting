// UnitTests/BookingServiceTests/Skeletons.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Application.UseCases;
using Domain.Entities;
using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;
using UnitTests.TestDoubles;
using Xunit;

namespace UnitTests.BookingServiceTests;

public class HoldSeats_ServiceSkeleton
{
    [Fact]
    public async Task HoldSeats_Valid_SingleSeat_Held()
    {
        var now = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc);
        var clock = new FakeClock(now);
        var db = new Mock<ICinemaDbContext>(MockBehavior.Strict);
        db.Setup(d => d.Shows).ReturnsDbSet(new List<Show>{ new(){ ShowId=1, ScreenId=10, BasePrice=100 } });
        db.Setup(d => d.Seats).ReturnsDbSet(new List<Seat>{ new(){ SeatId=5, ScreenId=10, SeatType="STANDARD" } });
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var repo = new Mock<IReservationRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetActiveByShowAndSeatAsync(1, 5, It.IsAny<CancellationToken>())).ReturnsAsync((Reservation?)null);
        repo.Setup(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var svc = new BookingService(db.Object, repo.Object, Mock.Of<IVoucherService>(), clock);
        var res = await svc.HoldSeatsAsync(new HoldSeatsRequest(1, 2, new long[]{5}, "k1"), default);

        res.Success.Should().BeTrue();
        res.Status.Should().Be("HELD");
        res.HoldExpiresAtUtc.Should().Be(now.AddMinutes(5));
        db.VerifyAll();
        repo.VerifyAll();
    }
}

public class Confirm_ServiceSkeleton
{
    [Fact]
    public async Task Confirm_ExpiredHold_Expired()
    {
        var now = new DateTime(2024,1,1,0,10,0,DateTimeKind.Utc);
        var repo = new Mock<IReservationRepository>(MockBehavior.Strict);
        repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Reservation{ ReservationId=1, Status="HELD", HoldExpiresAtUtc=new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) });
        var svc = new BookingService(Mock.Of<ICinemaDbContext>(MockBehavior.Strict), repo.Object, Mock.Of<IVoucherService>(), new FakeClock(now));
        var res = await svc.ConfirmBookingAsync(new ConfirmBookingRequest(1, null, null), default);
        res.Success.Should().BeFalse();
        res.Status.Should().Be("EXPIRED");
        repo.VerifyAll();
    }
}

public class ReleaseHold_ServiceSkeleton
{
    [Fact]
    public async Task ReleaseHold_Valid_Released()
    {
        var db = new Mock<ICinemaDbContext>(MockBehavior.Strict);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var repo = new Mock<IReservationRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Reservation{ ReservationId=1, Status="HELD", HoldExpiresAtUtc=DateTime.UtcNow.AddMinutes(2)});
        var svc = new BookingService(db.Object, repo.Object, Mock.Of<IVoucherService>(), Mock.Of<IClock>());
        var res = await svc.ReleaseHoldAsync(new ReleaseHoldRequest(1, "user-cancel"), default);
        res.Success.Should().BeTrue();
        res.Status.Should().Be("RELEASED");
        db.VerifyAll();
        repo.VerifyAll();
    }
}
