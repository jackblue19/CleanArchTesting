using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Keyless]
public partial class VShowSeatAvailability
{
    public long ScreenId { get; set; }

    public long ShowId { get; set; }

    public long SeatId { get; set; }

    [StringLength(8)]
    public string RowLabel { get; set; } = null!;

    public int SeatNumber { get; set; }

    [StringLength(16)]
    public string SeatType { get; set; } = null!;

    [StringLength(6)]
    [Unicode(false)]
    public string SeatState { get; set; } = null!;
}
