using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index("Status", "HoldExpiresAtUtc", Name = "IX_Reservations_HoldExpiry")]
[Index("ShowId", "Status", Name = "IX_Reservations_Show_Status")]
public partial class Reservation
{
    [Key]
    public long ReservationId { get; set; }

    public long ShowId { get; set; }

    public long SeatId { get; set; }

    public long UserId { get; set; }

    [StringLength(12)]
    public string Status { get; set; } = null!;

    [Precision(0)]
    public DateTime CreatedAtUtc { get; set; }

    [Precision(0)]
    public DateTime? HoldExpiresAtUtc { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Discount { get; set; }

    [Column(TypeName = "decimal(11, 2)")]
    public decimal? Total { get; set; }

    [StringLength(64)]
    public string? PaymentIntentId { get; set; }

    [StringLength(64)]
    public string? IdempotencyKey { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("SeatId")]
    [InverseProperty("Reservations")]
    public virtual Seat Seat { get; set; } = null!;

    [ForeignKey("ShowId")]
    [InverseProperty("Reservations")]
    public virtual Show Show { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Reservations")]
    public virtual User User { get; set; } = null!;

    [InverseProperty("Reservation")]
    public virtual VoucherRedemption? VoucherRedemption { get; set; }
}
