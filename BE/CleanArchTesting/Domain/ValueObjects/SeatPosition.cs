// Domain/ValueObjects/SeatPosition.cs
namespace Domain.ValueObjects;

public readonly struct SeatPosition
{
    public string RowLabel { get; }
    public int SeatNumber { get; }

    public SeatPosition(string rowLabel, int seatNumber)
    {
        RowLabel = string.IsNullOrWhiteSpace(rowLabel) ? throw new ArgumentException("RowLabel required", nameof(rowLabel)) : rowLabel;
        if (seatNumber <= 0) throw new ArgumentOutOfRangeException(nameof(seatNumber));
        SeatNumber = seatNumber;
    }

    public override string ToString() => $"{RowLabel}{SeatNumber}";
}
