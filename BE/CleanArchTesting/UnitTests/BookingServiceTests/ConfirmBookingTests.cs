// UnitTests/BookingServiceTests/ConfirmBookingTests.cs
using Application.DTOs;
using Application.Interfaces;
using Application.UseCases;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace UnitTests.BookingServiceTests;

public class ConfirmBookingTests
{
    [Fact]
    public async Task Confirm_Fails_When_Not_HELD()
    {
        var db = new Mock<ICinemaDbContext>();
        var repo = new Mock<IReservationRepository>();
        repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Reservation{ ReservationId=1, Status="RELEASED"});

        var svc = new BookingService(db.Object, repo.Object, Mock.Of<IVoucherService>(), Mock.Of<IClock>());
        var res = await svc.ConfirmBookingAsync(new ConfirmBookingRequest(1, null, null), default);
        res.Success.Should().BeFalse();
        res.Status.Should().Be("INVALID_STATE");
    }

    [Fact]
    public async Task Confirm_Succeeds_With_Percent_Voucher()
    {
        var now = new DateTime(2024,1,1,0,0,0, DateTimeKind.Utc);
        var db = new Mock<ICinemaDbContext>();
        var repo = new Mock<IReservationRepository>();
        repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Reservation{ ReservationId=1, Status="HELD", ShowId=1, SeatId=1, HoldExpiresAtUtc=now.AddMinutes(1)});

        db.Setup(x => x.Shows).ReturnsDbSet(new List<Show>{ new Show{ ShowId=1, ScreenId=10, BasePrice=100 } });
        db.Setup(x => x.Seats).ReturnsDbSet(new List<Seat>{ new Seat{ SeatId=1, ScreenId=10, SeatType="STANDARD" } });
        db.Setup(x => x.Screens).ReturnsDbSet(new List<Screen>{ new Screen{ ScreenId=10, ScreenType="2D" } });
        db.Setup(x => x.PriceAdjustments).ReturnsDbSet(new List<PriceAdjustment>());
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var voucherSvc = new Mock<IVoucherService>();
        voucherSvc.Setup(x => x.ValidateAndCalculateAsync("V10", 100, now, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, 10m, (string?)null));

        var svc = new BookingService(db.Object, repo.Object, voucherSvc.Object, Mock.Of<IClock>(x=>x.UtcNow==now));
        var res = await svc.ConfirmBookingAsync(new ConfirmBookingRequest(1, "V10", null), default);
        res.Success.Should().BeTrue();
        res.Subtotal.Should().Be(100);
        res.Discount.Should().Be(10);
        res.Total.Should().Be(90);
    }
}
