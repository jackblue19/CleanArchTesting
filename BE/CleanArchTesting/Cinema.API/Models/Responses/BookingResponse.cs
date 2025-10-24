using System;

namespace Cinema.API.Models.Responses;

public sealed record BookingResponse(
    long ReservationId,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? HoldExpiresAtUtc,
    decimal Subtotal,
    decimal Discount,
    decimal Total,
    long ShowId,
    string MovieTitle,
    DateTime ShowStartAtUtc,
    DateTime ShowEndAtUtc,
    long SeatId,
    string SeatLabel,
    string SeatType,
    long UserId,
    string UserEmail
);
