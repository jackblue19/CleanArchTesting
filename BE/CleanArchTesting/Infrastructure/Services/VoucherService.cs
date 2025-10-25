// Infrastructure/Services/VoucherService.cs
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class VoucherService : IVoucherService
{
    private readonly ICinemaDbContext _db;
    public VoucherService(ICinemaDbContext db) => _db = db;

    public async Task<(bool valid, decimal discount, string? reason)> ValidateAndCalculateAsync(string code, decimal subtotal, DateTime now, CancellationToken ct)
    {
        var v = await _db.Vouchers.FirstOrDefaultAsync(x => x.Code == code, ct);
        if (v == null) return (false, 0, "Voucher not found");
        if (!v.Active) return (false, 0, "Voucher inactive");
        if (v.ValidFromUtc > now || v.ValidToUtc < now) return (false, 0, "Voucher expired or not yet valid");
        if (v.MinSpend.HasValue && subtotal < v.MinSpend.Value) return (false, 0, "Minimum spend not met");

        decimal discount = v.Type switch
        {
            "PERCENT" => subtotal * (v.Value / 100m),
            _ => v.Value
        };
        if (v.MaxDiscount.HasValue) discount = Math.Min(discount, v.MaxDiscount.Value);
        discount = Math.Round(discount, 2, MidpointRounding.AwayFromZero);
        return (true, discount, null);
    }
}
