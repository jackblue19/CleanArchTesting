// UnitTests/Validators/ValidatorTests.cs
using Application.DTOs;
using Application.Validators;
using FluentAssertions;

namespace UnitTests.Validators;

public class ValidatorTests
{
    [Fact]
    public void HoldSeats_Validator_Detects_Duplicates()
    {
        var (ok, err) = HoldSeatsRequestValidator.Validate(new HoldSeatsRequest(1,1,new long[]{1,1},null));
        ok.Should().BeFalse();
        err.Should().NotBeNull();
    }

    [Fact]
    public void ConfirmBooking_Validator_Enforces_Lengths()
    {
        var longVoucher = new string('A', 33);
        var (ok, err) = ConfirmBookingRequestValidator.Validate(new ConfirmBookingRequest(1, longVoucher, new string('b', 65)));
        ok.Should().BeFalse();
        err.Should().NotBeNull();
    }
}
