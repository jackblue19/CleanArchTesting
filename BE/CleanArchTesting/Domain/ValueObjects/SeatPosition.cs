// File: src/Domain/ValueObjects/SeatPosition.cs
namespace Domain.ValueObjects;
public readonly record struct SeatPosition(long SeatId, string Row, int Number);
