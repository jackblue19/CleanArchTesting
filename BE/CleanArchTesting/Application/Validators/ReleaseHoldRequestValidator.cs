// Application/Validators/ReleaseHoldRequestValidator.cs
using Application.DTOs;

namespace Application.Validators;

public static class ReleaseHoldRequestValidator
{
    public static (bool ok, string? error) Validate(ReleaseHoldRequest req)
    {
        if (req.ReservationId <= 0) return (false, "ReservationId required");
        if (string.IsNullOrWhiteSpace(req.Reason)) return (false, "Reason required");
        return (true, null);
    }
}
