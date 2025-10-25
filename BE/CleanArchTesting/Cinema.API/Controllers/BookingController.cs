// Cinema.API/Controllers/BookingController.cs
using Application.DTOs;
using Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.API.Controllers;

[ApiController]
[Route("api/booking")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _booking;
    public BookingController(IBookingService booking) => _booking = booking;

    [HttpPost("hold")]
    public async Task<IActionResult> Hold([FromBody] HoldSeatsRequest req, CancellationToken ct)
        => Ok(await _booking.HoldSeatsAsync(req, ct));

    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] ConfirmBookingRequest req, CancellationToken ct)
        => Ok(await _booking.ConfirmBookingAsync(req, ct));

    [HttpPost("release")]
    public async Task<IActionResult> Release([FromBody] ReleaseHoldRequest req, CancellationToken ct)
        => Ok(await _booking.ReleaseHoldAsync(req, ct));
}
