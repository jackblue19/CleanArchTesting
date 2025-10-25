// Domain/ValueObjects/Money.cs
using System;

namespace Domain.ValueObjects;

/// <summary>
/// Simple value object to represent non-negative money amounts with two decimals.
/// </summary>
public readonly struct Money : IEquatable<Money>, IComparable<Money>
{
    public decimal Value { get; }

    public static Money Zero => new(0m);

    public Money(decimal value)
    {
        if (decimal.Round(value, 2) < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Money cannot be negative");
        Value = decimal.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    public static Money From(decimal value) => new(value);

    public static Money operator +(Money a, Money b) => new(a.Value + b.Value);
    public static Money operator -(Money a, Money b)
    {
        var v = a.Value - b.Value;
        if (v < 0) v = 0;
        return new Money(v);
    }

    public static implicit operator decimal(Money m) => m.Value;

    public int CompareTo(Money other) => Value.CompareTo(other.Value);
    public bool Equals(Money other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is Money m && Equals(m);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString("0.00");
}
