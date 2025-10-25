using System;

namespace Domain.Entities;

public partial class VShowSeatAvailability
{
    public string SeatLabel => string.Concat(RowLabel, SeatNumber);

    public bool IsInState(string state) => string.Equals(SeatState, state, StringComparison.OrdinalIgnoreCase);

    public bool IsFree => IsInState("FREE");
}
