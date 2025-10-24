using System.ComponentModel.DataAnnotations;

namespace Cinema.API.Models.Requests;

public sealed class BookingRequest
{
    [Required]
    [Range(1, long.MaxValue)]
    public long ShowId { get; init; }

    [Required]
    [Range(1, long.MaxValue)]
    public long SeatId { get; init; }

    [Required]
    [Range(1, long.MaxValue)]
    public long UserId { get; init; }

    [Range(0, 10000)]
    public decimal? Discount { get; init; }

    [StringLength(64)]
    public string? PaymentIntentId { get; init; }

    [StringLength(64)]
    public string? IdempotencyKey { get; init; }
}
