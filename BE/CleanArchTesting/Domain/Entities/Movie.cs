using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class Movie
{
    [Key]
    public long MovieId { get; set; }

    [StringLength(256)]
    public string Title { get; set; } = null!;

    public int AgeRating { get; set; }

    [InverseProperty("Movie")]
    public virtual ICollection<Show> Shows { get; set; } = new List<Show>();
}
