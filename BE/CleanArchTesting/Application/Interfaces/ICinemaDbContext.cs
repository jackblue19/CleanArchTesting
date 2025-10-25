// Application/Interfaces/ICinemaDbContext.cs
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces;

public interface ICinemaDbContext
{
    DbSet<User> Users { get; }
    DbSet<Movie> Movies { get; }
    DbSet<Screen> Screens { get; }
    DbSet<Seat> Seats { get; }
    DbSet<Show> Shows { get; }
    DbSet<Reservation> Reservations { get; }
    DbSet<Voucher> Vouchers { get; }
    DbSet<VoucherRedemption> VoucherRedemptions { get; }
    DbSet<Idempotency> Idempotencies { get; }
    DbSet<PriceAdjustment> PriceAdjustments { get; }
    DbSet<VShowSeatAvailability> VShowSeatAvailabilities { get; }
    DbSet<VShowOccupancy> VShowOccupancies { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
