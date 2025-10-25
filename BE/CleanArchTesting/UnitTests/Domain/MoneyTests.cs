using Domain.ValueObjects;
using FluentAssertions;

namespace UnitTests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Money_Basic_Arithmetic()
    {
        var a = Money.From(100);
        var b = Money.From(30.123m);
        (a + b).Value.Should().Be(130.12m);
        (a - b).Value.Should().Be(69.88m);
        (Money.From(10) - Money.From(20)).Value.Should().Be(0);
    }
}
