using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index("Code", Name = "UQ__Vouchers__A25C5AA77DD5B134", IsUnique = true)]
public partial class Voucher
{
    [Key]
    public long VoucherId { get; set; }

    [StringLength(32)]
    public string Code { get; set; } = null!;

    [StringLength(10)]
    public string Type { get; set; } = null!;

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Value { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? MaxDiscount { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? MinSpend { get; set; }

    [Precision(0)]
    public DateTime ValidFromUtc { get; set; }

    [Precision(0)]
    public DateTime ValidToUtc { get; set; }

    public bool Active { get; set; }

    [InverseProperty("Voucher")]
    public virtual ICollection<VoucherRedemption> VoucherRedemptions { get; set; } = new List<VoucherRedemption>();
}
