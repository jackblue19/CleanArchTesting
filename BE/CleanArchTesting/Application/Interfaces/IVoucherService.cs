// Application/Interfaces/IVoucherService.cs
namespace Application.Interfaces;

public interface IVoucherService
{
    Task<(bool valid, decimal discount, string? reason)> ValidateAndCalculateAsync(string code, decimal subtotal, DateTime now, CancellationToken ct);
}
