namespace Cinema.API.Models.Responses;

public sealed record SeatAvailabilityResponse(long SeatId, string RowLabel, int SeatNumber, string SeatType);
