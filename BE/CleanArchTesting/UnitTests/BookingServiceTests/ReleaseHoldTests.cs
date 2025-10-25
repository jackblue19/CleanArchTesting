// UnitTests/BookingServiceTests/ReleaseHoldTests.cs
using Application.DTOs;
using Application.Interfaces;
using Application.UseCases;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace UnitTests.BookingServiceTests;

public class ReleaseHoldTests
{
    [Fact]
    public async Task Release_Fails_When_Not_HELD()
    {
        var repo = new Mock<IReservationRepository>();
        repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Reservation{ ReservationId=1, Status="BOOKED"});
        var svc = new BookingService(Mock.Of<ICinemaDbContext>(), repo.Object, Mock.Of<IVoucherService>(), Mock.Of<IClock>());
        var res = await svc.ReleaseHoldAsync(new ReleaseHoldRequest(1, "reason"), default);
        res.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Release_Succeeds_When_HELD()
    {
        var db = new Mock<ICinemaDbContext>();
        var repo = new Mock<IReservationRepository>();
        var r = new Reservation{ ReservationId=1, Status="HELD" };
        repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(r);
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var svc = new BookingService(db.Object, repo.Object, Mock.Of<IVoucherService>(), Mock.Of<IClock>());
        var res = await svc.ReleaseHoldAsync(new ReleaseHoldRequest(1, "user_cancel"), default);
        res.Success.Should().BeTrue();
        r.Status.Should().Be("RELEASED");
    }
}
