using System;

namespace Domain.Entities;

public partial class Voucher
{
    private const string PercentType = "PERCENT";

    public bool IsActiveOn(DateTime utcNow) => Active && utcNow >= ValidFromUtc && utcNow <= ValidToUtc;

    public bool MeetsSpend(decimal subtotal) => !MinSpend.HasValue || subtotal >= MinSpend.Value;

    public decimal CalculateDiscount(decimal subtotal, DateTime utcNow)
    {
        if (subtotal <= 0 || !IsActiveOn(utcNow) || !MeetsSpend(subtotal))
        {
            return 0m;
        }

        var discount = Type.Equals(PercentType, StringComparison.OrdinalIgnoreCase)
            ? Math.Round(subtotal * Value / 100m, 2, MidpointRounding.AwayFromZero)
            : Value;

        if (MaxDiscount.HasValue)
        {
            discount = Math.Min(discount, MaxDiscount.Value);
        }

        return Math.Min(discount, subtotal);
    }
}
