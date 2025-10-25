using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Keyless]
public partial class VShowOccupancy
{
    public long ShowId { get; set; }

    public int? FreeSeats { get; set; }

    public int? HeldSeats { get; set; }

    public int? BookedSeats { get; set; }

    public int? TotalSeats { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? OccupancyPercent { get; set; }
}
