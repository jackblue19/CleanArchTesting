// UnitTests/Validators/ValidatorTests.cs
using System;
using Application.DTOs;
using Application.Validators;
using FluentAssertions;

namespace UnitTests.Validators;

public class ValidatorTests
{
    [Fact]
    public void HoldSeats_Validator_Detects_Duplicates()
    {
        var (ok, err) = HoldSeatsRequestValidator.Validate(new HoldSeatsRequest(1, 1, new long[] { 1, 1 }, null));
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

    [Fact]
    public void HoldSeats_Validator_RequiresSeatIds()
    {
        var (ok, err) = HoldSeatsRequestValidator.Validate(new HoldSeatsRequest(0, 1, Array.Empty<long>(), null));
        ok.Should().BeFalse();
        err.Should().Be("SeatIds required");
    }

    [Fact]
    public void HoldSeats_Validator_RejectsInvalidIds()
    {
        var (ok, err) = HoldSeatsRequestValidator.Validate(new HoldSeatsRequest(0, 0, new long[] { 1 }, null));
        ok.Should().BeFalse();
        err.Should().Be("Invalid showId/userId");
    }

    [Fact]
    public void HoldSeats_Validator_SucceedsForValidRequest()
    {
        var (ok, err) = HoldSeatsRequestValidator.Validate(new HoldSeatsRequest(1, 2, new long[] { 1, 2 }, null));
        ok.Should().BeTrue();
        err.Should().BeNull();
    }

    [Fact]
    public void ConfirmBooking_Validator_SucceedsForValidRequest()
    {
        var (ok, err) = ConfirmBookingRequestValidator.Validate(new ConfirmBookingRequest(10, "CODE", "pi_123"));
        ok.Should().BeTrue();
        err.Should().BeNull();
    }

    [Fact]
    public void ReleaseHold_Validator_RequiresReason()
    {
        var (ok, err) = ReleaseHoldRequestValidator.Validate(new ReleaseHoldRequest(10, " "));
        ok.Should().BeFalse();
        err.Should().Be("Reason required");
    }

    [Fact]
    public void ReleaseHold_Validator_RequiresReservationId()
    {
        var (ok, err) = ReleaseHoldRequestValidator.Validate(new ReleaseHoldRequest(0, "cancelled"));
        ok.Should().BeFalse();
        err.Should().Be("ReservationId required");
    }

    [Fact]
    public void ReleaseHold_Validator_SucceedsForValidRequest()
    {
        var (ok, err) = ReleaseHoldRequestValidator.Validate(new ReleaseHoldRequest(5, "user requested"));
        ok.Should().BeTrue();
        err.Should().BeNull();
    }
}
