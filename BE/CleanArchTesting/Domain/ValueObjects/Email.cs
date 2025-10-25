// File: src/Domain/ValueObjects/Email.cs
namespace Domain.ValueObjects;
public readonly record struct Email(string Value)
{
    public override string ToString() => Value;
    public static Email From(string input)
    {
        if ( string.IsNullOrWhiteSpace(input) )
            throw new ArgumentException("Email is required");
        if ( !input.Contains('@') )
            throw new ArgumentException("Email must contain '@'");
        return new Email(input.Trim());
    }
}
