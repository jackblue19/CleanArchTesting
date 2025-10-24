using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index("ScreenId", "RowLabel", "SeatNumber", Name = "UQ_Seat", IsUnique = true)]
public partial class Seat
{
    [Key]
    public long SeatId { get; set; }

    public long ScreenId { get; set; }

    [StringLength(8)]
    public string RowLabel { get; set; } = null!;

    public int SeatNumber { get; set; }

    [StringLength(16)]
    public string SeatType { get; set; } = null!;

    public long? SeatPairGroupId { get; set; }

    [InverseProperty("Seat")]
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    [ForeignKey("ScreenId")]
    [InverseProperty("Seats")]
    public virtual Screen Screen { get; set; } = null!;
}
