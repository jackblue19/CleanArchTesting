using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Table("Idempotency")]
public partial class Idempotency
{
    [Key]
    [StringLength(64)]
    public string Key { get; set; } = null!;

    [Precision(0)]
    public DateTime CreatedAtUtc { get; set; }

    [StringLength(16)]
    public string RequestType { get; set; } = null!;

    [StringLength(32)]
    public string ResultStatus { get; set; } = null!;

    public string? Payload { get; set; }
}
