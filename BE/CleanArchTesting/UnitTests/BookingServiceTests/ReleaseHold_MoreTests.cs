// UnitTests/BookingServiceTests/ReleaseHold_MoreTests.cs
using Application.DTOs;
using Application.Interfaces;
using Application.UseCases;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace UnitTests.BookingServiceTests;

public class ReleaseHold_MoreTests
{
    [Fact]
    public async Task Release_Fails_When_NotFound()
    {
        var repo = new Mock<IReservationRepository>();
        repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Reservation?)null);
        var res = await new BookingService(Mock.Of<ICinemaDbContext>(), repo.Object, Mock.Of<IVoucherService>(), Mock.Of<IClock>())
            .ReleaseHoldAsync(new ReleaseHoldRequest(1, "reason"), default);
        res.Success.Should().BeFalse();
        res.Status.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Release_Fails_When_Already_Released()
    {
        var repo = new Mock<IReservationRepository>();
        repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Reservation{ ReservationId=1, Status="RELEASED"});
        var res = await new BookingService(Mock.Of<ICinemaDbContext>(), repo.Object, Mock.Of<IVoucherService>(), Mock.Of<IClock>())
            .ReleaseHoldAsync(new ReleaseHoldRequest(1, "reason"), default);
        res.Success.Should().BeFalse();
        res.Status.Should().Be("INVALID_STATE");
    }
}
