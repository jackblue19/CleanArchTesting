using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class Show
{
    [Key]
    public long ShowId { get; set; }

    public long MovieId { get; set; }

    public long ScreenId { get; set; }

    [Precision(0)]
    public DateTime StartAtUtc { get; set; }

    [Precision(0)]
    public DateTime EndAtUtc { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal BasePrice { get; set; }

    public bool IsPeak { get; set; }

    [ForeignKey("MovieId")]
    [InverseProperty("Shows")]
    public virtual Movie Movie { get; set; } = null!;

    [InverseProperty("Show")]
    public virtual ICollection<PriceAdjustment> PriceAdjustments { get; set; } = new List<PriceAdjustment>();

    [InverseProperty("Show")]
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    [ForeignKey("ScreenId")]
    [InverseProperty("Shows")]
    public virtual Screen Screen { get; set; } = null!;
}
