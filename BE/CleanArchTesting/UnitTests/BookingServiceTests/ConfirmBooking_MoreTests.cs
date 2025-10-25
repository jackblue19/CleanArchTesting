// UnitTests/BookingServiceTests/ConfirmBooking_MoreTests.cs
using Application.DTOs;
using Application.Interfaces;
using Application.UseCases;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace UnitTests.BookingServiceTests;

public class ConfirmBooking_MoreTests
{
    [Fact]
    public async Task Confirm_Fails_When_Expired()
    {
        var now = new DateTime(2024, 1, 1, 0, 10, 0, DateTimeKind.Utc);
        var repo = new Mock<IReservationRepository>();
        repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Reservation{ ReservationId=1, Status="HELD", HoldExpiresAtUtc=new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) });
        var svc = new BookingService(Mock.Of<ICinemaDbContext>(), repo.Object, Mock.Of<IVoucherService>(), Mock.Of<IClock>(x=>x.UtcNow==now));
        var res = await svc.ConfirmBookingAsync(new ConfirmBookingRequest(1, null, null), default);
        res.Success.Should().BeFalse();
        res.Status.Should().Be("EXPIRED");
    }

    [Fact]
    public async Task Confirm_Applies_Adjustments_Percent_And_Fixed()
    {
        var now = DateTime.UtcNow;
        var db = new Mock<ICinemaDbContext>();
        var repo = new Mock<IReservationRepository>();
        repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Reservation{ ReservationId=1, Status="HELD", ShowId=1, SeatId=1, HoldExpiresAtUtc=now.AddMinutes(1)});

        db.Setup(x => x.Shows).ReturnsDbSet(new List<Show>{ new(){ ShowId=1, ScreenId=10, BasePrice=100 } });
        db.Setup(x => x.Screens).ReturnsDbSet(new List<Screen>{ new(){ ScreenId=10, ScreenType="IMAX" } });
        db.Setup(x => x.Seats).ReturnsDbSet(new List<Seat>{ new(){ SeatId=1, ScreenId=10, SeatType="VIP" } });
        db.Setup(x => x.PriceAdjustments).ReturnsDbSet(new List<PriceAdjustment>{
            new(){ ShowId=1, Target="GLOBAL", Mode="PERCENT", Amount=10 }, // 100 -> 110
            new(){ ShowId=1, Target="SEAT_TYPE", Mode="FIXED", Key="VIP", Amount=20 }, // 110 -> 130
            new(){ ShowId=1, Target="SCREEN_TYPE", Mode="PERCENT", Key="IMAX", Amount=10 } // 130 -> 143
        });
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var svc = new BookingService(db.Object, repo.Object, Mock.Of<IVoucherService>(), Mock.Of<IClock>(x=>x.UtcNow==now));
        var res = await svc.ConfirmBookingAsync(new ConfirmBookingRequest(1, null, null), default);
        res.Success.Should().BeTrue();
        res.Subtotal.Should().Be(143);
    }

    [Fact]
    public async Task Confirm_Voucher_MinSpend_And_MaxDiscount()
    {
        var now = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc);
        var db = new Mock<ICinemaDbContext>();
        var repo = new Mock<IReservationRepository>();
        repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Reservation{ ReservationId=1, Status="HELD", ShowId=1, SeatId=1, HoldExpiresAtUtc=now.AddMinutes(1)});

        db.Setup(x => x.Shows).ReturnsDbSet(new List<Show>{ new(){ ShowId=1, ScreenId=10, BasePrice=200 } });
        db.Setup(x => x.Screens).ReturnsDbSet(new List<Screen>{ new(){ ScreenId=10, ScreenType="2D" } });
        db.Setup(x => x.Seats).ReturnsDbSet(new List<Seat>{ new(){ SeatId=1, ScreenId=10, SeatType="STANDARD" } });
        db.Setup(x => x.PriceAdjustments).ReturnsDbSet(new List<PriceAdjustment>());
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var voucher = new Mock<IVoucherService>();
        voucher.Setup(x => x.ValidateAndCalculateAsync("P50", 200, now, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, 80m, (string?)null)); // pretend min spend ok and capped at 80

        var svc = new BookingService(db.Object, repo.Object, voucher.Object, Mock.Of<IClock>(x=>x.UtcNow==now));
        var res = await svc.ConfirmBookingAsync(new ConfirmBookingRequest(1, "P50", null), default);
        res.Success.Should().BeTrue();
        res.Total.Should().Be(120);
    }
}
