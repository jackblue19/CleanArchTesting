using System;
using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace UnitTests.Domain;

public class VoucherTests
{
    private static Voucher CreateVoucher(string type = "AMOUNT", decimal value = 10m)
    {
        return new Voucher
        {
            VoucherId = 1,
            Code = "CODE10",
            Type = type,
            Value = value,
            MaxDiscount = null,
            MinSpend = null,
            ValidFromUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ValidToUtc = new DateTime(2025, 12, 31, 23, 59, 59, DateTimeKind.Utc),
            Active = true
        };
    }

    [Fact]
    public void Voucher_IsActiveWithinWindow()
    {
        var voucher = CreateVoucher();
        voucher.IsActiveOn(new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc)).Should().BeTrue();
    }

    [Fact]
    public void Voucher_CalculateDiscount_AmountType()
    {
        var voucher = CreateVoucher(value: 15m);
        voucher.CalculateDiscount(40m, new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc)).Should().Be(15m);
    }

    [Fact]
    public void Voucher_CalculateDiscount_PercentType_WithCap()
    {
        var voucher = CreateVoucher(type: "PERCENT", value: 25m);
        voucher.MaxDiscount = 12m;
        var result = voucher.CalculateDiscount(100m, new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc));
        result.Should().Be(12m);
    }

    [Fact]
    public void Voucher_CalculateDiscount_RespectsMinSpend()
    {
        var voucher = CreateVoucher(type: "PERCENT", value: 50m);
        voucher.MinSpend = 50m;
        var result = voucher.CalculateDiscount(25m, new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc));
        result.Should().Be(0m);
    }

    [Fact]
    public void Voucher_CalculateDiscount_WhenInactive_ReturnsZero()
    {
        var voucher = CreateVoucher();
        voucher.Active = false;
        var result = voucher.CalculateDiscount(50m, new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc));
        result.Should().Be(0m);
    }
}
