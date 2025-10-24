using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index("Email", Name = "UQ__Users__A9D10534CEC6C1E9", IsUnique = true)]
public partial class User
{
    [Key]
    public long UserId { get; set; }

    [StringLength(256)]
    public string Email { get; set; } = null!;

    [InverseProperty("User")]
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
