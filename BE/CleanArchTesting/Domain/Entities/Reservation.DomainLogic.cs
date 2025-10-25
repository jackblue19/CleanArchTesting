using System;

namespace Domain.Entities;

public partial class Reservation
{
    private const string HeldStatus = "HELD";
    private const string BookedStatus = "BOOKED";

    public bool HoldHasExpired(DateTime utcNow) => HoldExpiresAtUtc.HasValue && utcNow >= HoldExpiresAtUtc.Value;

    public bool IsHoldActive(DateTime utcNow) => string.Equals(Status, HeldStatus, StringComparison.OrdinalIgnoreCase) && !HoldHasExpired(utcNow);

    public bool IsBooked() => string.Equals(Status, BookedStatus, StringComparison.OrdinalIgnoreCase);

    public decimal AmountDue => Math.Max(0m, Subtotal - Discount);

    public void ApplyCalculatedTotal() => Total = AmountDue;
}
