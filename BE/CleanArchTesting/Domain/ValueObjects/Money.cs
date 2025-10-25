// File: src/Domain/ValueObjects/Money.cs
namespace Domain.ValueObjects;
public readonly record struct Money(decimal Amount)
{
    public static Money Zero => new(0m);
    public Money Add(Money other) => new(Amount + other.Amount);
    public Money Subtract(Money other) => new(Math.Max(0, Amount - other.Amount));
    public static implicit operator decimal(Money m) => m.Amount;
    public override string ToString() => Amount.ToString("0.##");
}
