// Application/Validators/ConfirmBookingRequestValidator.cs
using Application.DTOs;

namespace Application.Validators;

public static class ConfirmBookingRequestValidator
{
    public static (bool ok, string? error) Validate(ConfirmBookingRequest req)
    {
        if (req.ReservationId <= 0) return (false, "ReservationId required");
        if (req.PaymentIntentId is { Length: > 64 }) return (false, "PaymentIntentId too long");
        if (req.VoucherCode is { Length: > 32 }) return (false, "Voucher code too long");
        return (true, null);
    }
}
