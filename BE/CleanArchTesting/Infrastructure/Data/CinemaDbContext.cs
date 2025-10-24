using System;
using System.Collections.Generic;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public partial class CinemaDbContext : DbContext
{
    public CinemaDbContext()
    {
    }

    public CinemaDbContext(DbContextOptions<CinemaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Idempotency> Idempotencies { get; set; }

    public virtual DbSet<Movie> Movies { get; set; }

    public virtual DbSet<PriceAdjustment> PriceAdjustments { get; set; }

    public virtual DbSet<Reservation> Reservations { get; set; }

    public virtual DbSet<Screen> Screens { get; set; }

    public virtual DbSet<Seat> Seats { get; set; }

    public virtual DbSet<Show> Shows { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VShowOccupancy> VShowOccupancies { get; set; }

    public virtual DbSet<VShowSeatAvailability> VShowSeatAvailabilities { get; set; }

    public virtual DbSet<Voucher> Vouchers { get; set; }

    public virtual DbSet<VoucherRedemption> VoucherRedemptions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost,1433;Database=Cinema;User Id=sa;Password=StrongP@ssword1;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Idempotency>(entity =>
        {
            entity.HasKey(e => e.Key).HasName("PK__Idempote__C41E0288AEEA3DB1");

            entity.Property(e => e.CreatedAtUtc).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.MovieId).HasName("PK__Movies__4BD2941ADA6D9A45");
        });

        modelBuilder.Entity<PriceAdjustment>(entity =>
        {
            entity.HasKey(e => e.AdjustmentId).HasName("PK__PriceAdj__E60DB8939E4B1F3D");

            entity.HasOne(d => d.Show).WithMany(p => p.PriceAdjustments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PriceAdju__ShowI__48CFD27E");
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.ReservationId).HasName("PK__Reservat__B7EE5F24E50E1B09");

            entity.HasIndex(e => new { e.ShowId , e.SeatId } , "UX_Reservations_UniqueActive")
                .IsUnique()
                .HasFilter("([Status] IN ('HELD', 'BOOKED'))");

            entity.Property(e => e.CreatedAtUtc).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Total).HasComputedColumnSql("(case when ([Subtotal]-[Discount])<(0) then (0) else [Subtotal]-[Discount] end)" , true);

            entity.HasOne(d => d.Seat).WithMany(p => p.Reservations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reservati__SeatI__5165187F");

            entity.HasOne(d => d.Show).WithMany(p => p.Reservations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reservati__ShowI__5070F446");

            entity.HasOne(d => d.User).WithMany(p => p.Reservations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reservati__UserI__52593CB8");
        });

        modelBuilder.Entity<Screen>(entity =>
        {
            entity.HasKey(e => e.ScreenId).HasName("PK__Screens__0AB60FA588B8B9D9");
        });

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(e => e.SeatId).HasName("PK__Seats__311713F3B114E906");

            entity.HasOne(d => d.Screen).WithMany(p => p.Seats)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Seats__ScreenId__3F466844");
        });

        modelBuilder.Entity<Show>(entity =>
        {
            entity.HasKey(e => e.ShowId).HasName("PK__Shows__6DE3E0B29DB6CDA4");

            entity.HasOne(d => d.Movie).WithMany(p => p.Shows)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Shows__MovieId__440B1D61");

            entity.HasOne(d => d.Screen).WithMany(p => p.Shows)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Shows__ScreenId__44FF419A");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C9A66635C");
        });

        modelBuilder.Entity<VShowOccupancy>(entity =>
        {
            entity.ToView("v_ShowOccupancy");
        });

        modelBuilder.Entity<VShowSeatAvailability>(entity =>
        {
            entity.ToView("v_ShowSeatAvailability");
        });

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.HasKey(e => e.VoucherId).HasName("PK__Vouchers__3AEE79219A9516D1");

            entity.Property(e => e.Active).HasDefaultValue(true);
        });

        modelBuilder.Entity<VoucherRedemption>(entity =>
        {
            entity.HasKey(e => e.RedemptionId).HasName("PK__VoucherR__410680B1351D20CB");

            entity.Property(e => e.RedeemedAtUtc).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Reservation).WithOne(p => p.VoucherRedemption)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__VoucherRe__Reser__5BE2A6F2");

            entity.HasOne(d => d.Voucher).WithMany(p => p.VoucherRedemptions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__VoucherRe__Vouch__5CD6CB2B");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
