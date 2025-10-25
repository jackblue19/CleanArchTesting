namespace Domain.ValueObjects;
public readonly record struct IdempotencyKey(string Value)
{
    public static IdempotencyKey New() => new(Guid.NewGuid().ToString("N"));
}