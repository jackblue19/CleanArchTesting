// Infrastructure/Repositories/ReservationRepository.cs
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly ICinemaDbContext _db;
    public ReservationRepository(ICinemaDbContext db) => _db = db;

    public async Task<Reservation?> GetActiveByShowAndSeatAsync(long showId, long seatId, CancellationToken ct)
        => await _db.Reservations.FirstOrDefaultAsync(r => r.ShowId == showId && r.SeatId == seatId && (r.Status == "HELD" || r.Status == "BOOKED"), ct);

    public async Task AddAsync(Reservation reservation, CancellationToken ct)
        => await _db.Reservations.AddAsync(reservation, ct);

    public async Task<Reservation?> GetByIdAsync(long reservationId, CancellationToken ct)
        => await _db.Reservations.FirstOrDefaultAsync(r => r.ReservationId == reservationId, ct);
}
