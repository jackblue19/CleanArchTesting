using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class Screen
{
    [Key]
    public long ScreenId { get; set; }

    public long TheaterId { get; set; }

    [StringLength(128)]
    public string Name { get; set; } = null!;

    [StringLength(16)]
    public string ScreenType { get; set; } = null!;

    [InverseProperty("Screen")]
    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();

    [InverseProperty("Screen")]
    public virtual ICollection<Show> Shows { get; set; } = new List<Show>();
}
