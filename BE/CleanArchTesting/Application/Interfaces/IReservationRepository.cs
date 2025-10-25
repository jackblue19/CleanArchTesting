// Application/Interfaces/IReservationRepository.cs
using Domain.Entities;

namespace Application.Interfaces;

public interface IReservationRepository
{
    Task<Reservation?> GetActiveByShowAndSeatAsync(long showId, long seatId, CancellationToken ct);
    Task AddAsync(Reservation reservation, CancellationToken ct);
    Task<Reservation?> GetByIdAsync(long reservationId, CancellationToken ct);
}
