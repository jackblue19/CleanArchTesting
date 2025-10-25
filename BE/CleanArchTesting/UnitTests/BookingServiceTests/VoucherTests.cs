// UnitTests/BookingServiceTests/VoucherTests.cs
using Application.Interfaces;
using Infrastructure.Services;
using Domain.Entities;
using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;

namespace UnitTests.BookingServiceTests;

public class VoucherTests
{
    [Fact]
    public async Task Voucher_Invalid_When_Inactive_Or_DateRange()
    {
        var now = new DateTime(2024,1,1,0,0,0, DateTimeKind.Utc);
        var db = new Mock<ICinemaDbContext>();
        db.Setup(x => x.Vouchers).ReturnsDbSet(new List<Voucher>{
            new(){ VoucherId=1, Code="X", Active=false, ValidFromUtc=now.AddDays(-1), ValidToUtc=now.AddDays(1), Type="PERCENT", Value=10 },
        });
        var svc = new VoucherService(db.Object);
        var r = await svc.ValidateAndCalculateAsync("X", 100, now, default);
        r.valid.Should().BeFalse();
        r.reason.Should().Contain("inactive");
    }

    [Fact]
    public async Task Voucher_Valid_With_Cap_And_MinSpend()
    {
        var now = new DateTime(2024,1,1,0,0,0, DateTimeKind.Utc);
        var db = new Mock<ICinemaDbContext>();
        db.Setup(x => x.Vouchers).ReturnsDbSet(new List<Voucher>{
            new(){ VoucherId=1, Code="P50", Active=true, ValidFromUtc=now.AddDays(-1), ValidToUtc=now.AddDays(1), Type="PERCENT", Value=50, MaxDiscount=80, MinSpend=100 },
        });
        var svc = new VoucherService(db.Object);
        var r = await svc.ValidateAndCalculateAsync("P50", 200, now, default);
        r.valid.Should().BeTrue();
        r.discount.Should().Be(80);
    }
}
