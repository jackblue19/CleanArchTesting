using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class PriceAdjustment
{
    [Key]
    public long AdjustmentId { get; set; }

    public long ShowId { get; set; }

    [StringLength(16)]
    public string Target { get; set; } = null!;

    [StringLength(16)]
    public string Mode { get; set; } = null!;

    [StringLength(16)]
    public string Key { get; set; } = null!;

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Amount { get; set; }

    [ForeignKey("ShowId")]
    [InverseProperty("PriceAdjustments")]
    public virtual Show Show { get; set; } = null!;
}
