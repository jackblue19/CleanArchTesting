// Cinema.API/Controllers/AuthController.cs
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ICinemaDbContext _db;
    public AuthController(ICinemaDbContext db) => _db = db;

    // Basic login: only email required, create if not exists
    [HttpPost("login")] 
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Email)) return BadRequest("Email required");
        var user = _db.Users.FirstOrDefault(u => u.Email == req.Email);
        if (user == null)
        {
            user = new Domain.Entities.User { Email = req.Email };
            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);
        }
        return Ok(new { user.UserId, user.Email });
    }

    public record LoginRequest(string Email);
}
