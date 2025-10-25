namespace Domain.Entities;

public partial class Seat
{
    public string SeatLabel => string.Concat(RowLabel, SeatNumber);
}
