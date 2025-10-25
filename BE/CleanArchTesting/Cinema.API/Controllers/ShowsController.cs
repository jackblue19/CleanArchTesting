// Cinema.API/Controllers/ShowsController.cs
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.API.Controllers;

[ApiController]
[Route("api/shows")] 
public class ShowsController : ControllerBase
{
    private readonly ICinemaDbContext _db;
    public ShowsController(ICinemaDbContext db) => _db = db;

    [HttpGet("{showId:long}/availability")]
    public async Task<IActionResult> GetAvailability(long showId, CancellationToken ct)
    {
        var data = await _db.VShowSeatAvailabilities
            .Where(v => v.ShowId == showId)
            .Select(v => new { v.ScreenId, v.ShowId, v.SeatId, v.RowLabel, v.SeatNumber, v.SeatType, v.SeatState })
            .ToListAsync(ct);
        return Ok(data);
    }

    [HttpGet("{showId:long}/occupancy")]
    public async Task<IActionResult> GetOccupancy(long showId, CancellationToken ct)
    {
        var row = await _db.VShowOccupancies.FirstOrDefaultAsync(v => v.ShowId == showId, ct);
        if (row == null) return NotFound();
        return Ok(new { row.ShowId, row.FreeSeats, row.HeldSeats, row.BookedSeats, row.TotalSeats, row.OccupancyPercent });
    }
}
