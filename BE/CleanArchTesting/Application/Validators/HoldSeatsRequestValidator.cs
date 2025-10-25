// Application/Validators/HoldSeatsRequestValidator.cs
using Application.DTOs;

namespace Application.Validators;

public static class HoldSeatsRequestValidator
{
    public static (bool ok, string? error) Validate(HoldSeatsRequest req)
    {
        if (req.SeatIds == null || req.SeatIds.Count == 0) return (false, "SeatIds required");
        if (req.SeatIds.Count != req.SeatIds.Distinct().Count()) return (false, "Duplicate seatIds");
        if (req.ShowId <= 0 || req.UserId <= 0) return (false, "Invalid showId/userId");
        return (true, null);
    }
}
