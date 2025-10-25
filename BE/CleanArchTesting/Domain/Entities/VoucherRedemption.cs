using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index("ReservationId", Name = "UQ_VoucherPerReservation", IsUnique = true)]
public partial class VoucherRedemption
{
    [Key]
    public long RedemptionId { get; set; }

    public long ReservationId { get; set; }

    public long VoucherId { get; set; }

    [Precision(0)]
    public DateTime RedeemedAtUtc { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal DiscountApplied { get; set; }

    [ForeignKey("ReservationId")]
    [InverseProperty("VoucherRedemption")]
    public virtual Reservation Reservation { get; set; } = null!;

    [ForeignKey("VoucherId")]
    [InverseProperty("VoucherRedemptions")]
    public virtual Voucher Voucher { get; set; } = null!;
}
