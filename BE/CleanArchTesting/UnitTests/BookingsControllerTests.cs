using System.Linq;
using Cinema.API.Controllers;
using Cinema.API.Models.Requests;
using Cinema.API.Models.Responses;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace UnitTests;

public sealed class BookingsControllerTests
{
    // Happy-path coverage ----------------------------------------------------

    [Fact]
    public async Task CreateBooking_HappyPath_ReturnsCreatedResult()
    {
        await using var dbContext = CreateDbContext();
        var (_, movie, show, seat, user) = await SeedStandardShowAsync(dbContext);
        var controller = new BookingsController(dbContext);
        var request = new BookingRequest
        {
            ShowId = show.ShowId,
            SeatId = seat.SeatId,
            UserId = user.UserId,
            Discount = 1.5m,
            PaymentIntentId = "pi_test",
            IdempotencyKey = "idem-123"
        };

        var actionResult = await controller.CreateBooking(request);

        var createdAt = actionResult.Result.Should().BeOfType<CreatedAtActionResult>().Which;
        createdAt.ActionName.Should().Be(nameof(BookingsController.GetBooking));

        var response = createdAt.Value.Should().BeOfType<BookingResponse>().Which;
        response.Status.Should().Be("BOOKED");
        response.ShowId.Should().Be(show.ShowId);
        response.UserId.Should().Be(user.UserId);
        response.SeatId.Should().Be(seat.SeatId);
        response.MovieTitle.Should().Be(movie.Title);
        response.Total.Should().BeApproximately(show.BasePrice - request.Discount.GetValueOrDefault(), 0.001m);

        var reservation = await dbContext.Reservations.SingleAsync();
        reservation.Status.Should().Be("BOOKED");
        reservation.PaymentIntentId.Should().Be("pi_test");
    }

    [Fact]
    public async Task GetBooking_HappyPath_ReturnsReservation()
    {
        await using var dbContext = CreateDbContext();
        var (_, movie, show, seat, user) = await SeedStandardShowAsync(dbContext);

        var reservation = new Reservation
        {
            ReservationId = 77,
            ShowId = show.ShowId,
            SeatId = seat.SeatId,
            UserId = user.UserId,
            Status = "BOOKED",
            CreatedAtUtc = DateTime.UtcNow,
            Subtotal = show.BasePrice,
            Discount = 0m
        };

        dbContext.Reservations.Add(reservation);
        await dbContext.SaveChangesAsync();

        var controller = new BookingsController(dbContext);
        var actionResult = await controller.GetBooking(reservation.ReservationId);

        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Which;
        var payload = okResult.Value.Should().BeOfType<BookingResponse>().Which;
        payload.ReservationId.Should().Be(reservation.ReservationId);
        payload.MovieTitle.Should().Be(movie.Title);
    }

    [Fact]
    public async Task GetAvailableSeats_HappyPath_ReturnsOnlyFreeSeats()
    {
        await using var dbContext = CreateDbContext();
        var (screen, _, show, seat, user) = await SeedStandardShowAsync(dbContext);

        var secondSeat = CreateSeat(3001, screen.ScreenId, "A", 2);
        var thirdSeat = CreateSeat(3002, screen.ScreenId, "A", 3);
        dbContext.Seats.AddRange(secondSeat, thirdSeat);
        dbContext.Reservations.Add(new Reservation
        {
            ReservationId = 90,
            ShowId = show.ShowId,
            SeatId = seat.SeatId,
            UserId = user.UserId,
            Status = "BOOKED",
            CreatedAtUtc = DateTime.UtcNow,
            Subtotal = show.BasePrice,
            Discount = 0m
        });
        await dbContext.SaveChangesAsync();

        var controller = new BookingsController(dbContext);
        var actionResult = await controller.GetAvailableSeats(show.ShowId);

        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Which;
        var seats = okResult.Value.Should().BeAssignableTo<IEnumerable<SeatAvailabilityResponse>>().Which.ToList();
        seats.Should().HaveCount(2);
        seats.Select(s => s.SeatId).Should().Contain(new[] { secondSeat.SeatId, thirdSeat.SeatId }).And.NotContain(seat.SeatId);
    }

    [Fact]
    public async Task GetBookingsForUser_HappyPath_ReturnsDescendingReservations()
    {
        await using var dbContext = CreateDbContext();
        var (_, _, show, seat, user) = await SeedStandardShowAsync(dbContext);

        var secondSeat = CreateSeat(4000, show.ScreenId, "B", 1);
        dbContext.Seats.Add(secondSeat);
        await dbContext.SaveChangesAsync();

        var futureReservation = new Reservation
        {
            ReservationId = 101,
            ShowId = show.ShowId,
            SeatId = secondSeat.SeatId,
            UserId = user.UserId,
            Status = "BOOKED",
            CreatedAtUtc = DateTime.UtcNow,
            Subtotal = show.BasePrice,
            Discount = 0m
        };

        var pastReservation = new Reservation
        {
            ReservationId = 102,
            ShowId = show.ShowId,
            SeatId = seat.SeatId,
            UserId = user.UserId,
            Status = "BOOKED",
            CreatedAtUtc = DateTime.UtcNow.AddHours(-2),
            Subtotal = show.BasePrice,
            Discount = 2m
        };

        dbContext.Reservations.AddRange(futureReservation, pastReservation);
        await dbContext.SaveChangesAsync();

        var controller = new BookingsController(dbContext);
        var actionResult = await controller.GetBookingsForUser(user.UserId);

        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Which;
        var bookings = okResult.Value.Should().BeAssignableTo<IEnumerable<BookingResponse>>().Which.ToList();
        bookings.Should().HaveCount(2);
        bookings.First().ReservationId.Should().Be(futureReservation.ReservationId);
        bookings.Last().ReservationId.Should().Be(pastReservation.ReservationId);
    }

    // Edge-case coverage -----------------------------------------------------

    [Fact]
    public async Task CreateBooking_EdgeCase_ReturnsNotFoundWhenShowMissing()
    {
        await using var dbContext = CreateDbContext();
        var screen = CreateScreen(200, "Screen 1");
        var seat = CreateSeat(3000, screen.ScreenId);
        var user = CreateUser(2000, "test-user@example.com");
        dbContext.AddRange(screen, seat, user);
        await dbContext.SaveChangesAsync();

        var controller = new BookingsController(dbContext);
        var request = new BookingRequest { ShowId = 9999, SeatId = seat.SeatId, UserId = user.UserId };

        var actionResult = await controller.CreateBooking(request);

        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateBooking_EdgeCase_ReturnsNotFoundWhenSeatMissing()
    {
        await using var dbContext = CreateDbContext();
        var (screen, _, show, _, user) = await SeedStandardShowAsync(dbContext);
        dbContext.Seats.RemoveRange(dbContext.Seats); // remove seeded seat
        await dbContext.SaveChangesAsync();

        var controller = new BookingsController(dbContext);
        var request = new BookingRequest { ShowId = show.ShowId, SeatId = 9999, UserId = user.UserId };

        var actionResult = await controller.CreateBooking(request);

        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateBooking_EdgeCase_ReturnsBadRequestWhenSeatFromAnotherScreen()
    {
        await using var dbContext = CreateDbContext();
        var (screen, _, show, seat, user) = await SeedStandardShowAsync(dbContext);
        var otherScreen = CreateScreen(400, "Screen 2");
        var foreignSeat = CreateSeat(5000, otherScreen.ScreenId, "B", 5);
        dbContext.Screens.Add(otherScreen);
        dbContext.Seats.Add(foreignSeat);
        await dbContext.SaveChangesAsync();

        var controller = new BookingsController(dbContext);
        var request = new BookingRequest { ShowId = show.ShowId, SeatId = foreignSeat.SeatId, UserId = user.UserId };

        var actionResult = await controller.CreateBooking(request);

        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateBooking_EdgeCase_ReturnsNotFoundWhenUserMissing()
    {
        await using var dbContext = CreateDbContext();
        var (_, _, show, seat, _) = await SeedStandardShowAsync(dbContext);
        dbContext.Users.RemoveRange(dbContext.Users);
        await dbContext.SaveChangesAsync();

        var controller = new BookingsController(dbContext);
        var request = new BookingRequest { ShowId = show.ShowId, SeatId = seat.SeatId, UserId = 9999 };

        var actionResult = await controller.CreateBooking(request);

        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateBooking_EdgeCase_ReturnsConflictWhenSeatAlreadyBooked()
    {
        await using var dbContext = CreateDbContext();
        var (_, _, show, seat, user) = await SeedStandardShowAsync(dbContext);
        var existingReservation = new Reservation
        {
            ReservationId = 55,
            ShowId = show.ShowId,
            SeatId = seat.SeatId,
            UserId = user.UserId,
            Status = "BOOKED",
            CreatedAtUtc = DateTime.UtcNow,
            Subtotal = show.BasePrice,
            Discount = 0m
        };
        dbContext.Reservations.Add(existingReservation);
        await dbContext.SaveChangesAsync();

        var controller = new BookingsController(dbContext);
        var request = new BookingRequest { ShowId = show.ShowId, SeatId = seat.SeatId, UserId = user.UserId };

        var actionResult = await controller.CreateBooking(request);

        actionResult.Result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task GetBooking_EdgeCase_ReturnsNotFoundForUnknownReservation()
    {
        await using var dbContext = CreateDbContext();
        await SeedStandardShowAsync(dbContext);
        var controller = new BookingsController(dbContext);

        var actionResult = await controller.GetBooking(9999);

        actionResult.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetAvailableSeats_EdgeCase_ReturnsNotFoundWhenShowMissing()
    {
        await using var dbContext = CreateDbContext();
        await SeedStandardShowAsync(dbContext);
        var controller = new BookingsController(dbContext);

        var actionResult = await controller.GetAvailableSeats(9999);

        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetBookingsForUser_EdgeCase_ReturnsNotFoundWhenUserMissing()
    {
        await using var dbContext = CreateDbContext();
        await SeedStandardShowAsync(dbContext);
        dbContext.Users.RemoveRange(dbContext.Users);
        await dbContext.SaveChangesAsync();

        var controller = new BookingsController(dbContext);
        var actionResult = await controller.GetBookingsForUser(9999);

        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // Rare-case coverage -----------------------------------------------------

    [Fact]
    public async Task CreateBooking_RareCase_AllowsBookingWithoutOptionalFields()
    {
        await using var dbContext = CreateDbContext();
        var (_, _, show, seat, user) = await SeedStandardShowAsync(dbContext, basePrice: 20m);
        var controller = new BookingsController(dbContext);
        var request = new BookingRequest { ShowId = show.ShowId, SeatId = seat.SeatId, UserId = user.UserId };

        var actionResult = await controller.CreateBooking(request);

        var createdAt = actionResult.Result.Should().BeOfType<CreatedAtActionResult>().Which;
        var response = createdAt.Value.Should().BeOfType<BookingResponse>().Which;
        response.Total.Should().Be(show.BasePrice);

        var reservation = await dbContext.Reservations.SingleAsync();
        reservation.Discount.Should().Be(0m);
        reservation.PaymentIntentId.Should().BeNull();
        reservation.IdempotencyKey.Should().BeNull();
    }

    [Fact]
    public async Task GetAvailableSeats_RareCase_ReturnsEmptyWhenAllSeatsTaken()
    {
        await using var dbContext = CreateDbContext();
        var (_, _, show, seat, user) = await SeedStandardShowAsync(dbContext);
        var secondSeat = CreateSeat(3100, show.ScreenId, "A", 2);
        dbContext.Seats.Add(secondSeat);
        await dbContext.SaveChangesAsync();

        dbContext.Reservations.AddRange(
            new Reservation
            {
                ReservationId = 501,
                ShowId = show.ShowId,
                SeatId = seat.SeatId,
                UserId = user.UserId,
                Status = "BOOKED",
                CreatedAtUtc = DateTime.UtcNow,
                Subtotal = show.BasePrice,
                Discount = 0m
            },
            new Reservation
            {
                ReservationId = 502,
                ShowId = show.ShowId,
                SeatId = secondSeat.SeatId,
                UserId = user.UserId,
                Status = "HELD",
                CreatedAtUtc = DateTime.UtcNow,
                HoldExpiresAtUtc = DateTime.UtcNow.AddMinutes(10),
                Subtotal = show.BasePrice,
                Discount = 0m
            });
        await dbContext.SaveChangesAsync();

        var controller = new BookingsController(dbContext);
        var actionResult = await controller.GetAvailableSeats(show.ShowId);

        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Which;
        var seats = okResult.Value.Should().BeAssignableTo<IEnumerable<SeatAvailabilityResponse>>().Which;
        seats.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBookingsForUser_RareCase_ReturnsEmptyListWhenUserHasNoReservations()
    {
        await using var dbContext = CreateDbContext();
        var (_, _, _, _, user) = await SeedStandardShowAsync(dbContext);
        dbContext.Reservations.RemoveRange(dbContext.Reservations);
        await dbContext.SaveChangesAsync();

        var controller = new BookingsController(dbContext);
        var actionResult = await controller.GetBookingsForUser(user.UserId);

        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Which;
        var bookings = okResult.Value.Should().BeAssignableTo<IEnumerable<BookingResponse>>().Which;
        bookings.Should().BeEmpty();
    }

    // Helpers ----------------------------------------------------------------

    private static CinemaDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CinemaDbContext>()
            .UseInMemoryDatabase($"BookingsControllerTests_{Guid.NewGuid():N}")
            .Options;

        var context = new CinemaDbContext(options);
        context.Database.EnsureCreated();

        context.SavingChanges += (_, _) =>
        {
            foreach (var entry in context.ChangeTracker.Entries<Reservation>())
            {
                if (entry.State == EntityState.Added)
                {
                    var rowVersion = entry.Property(nameof(Reservation.RowVersion));
                    if (rowVersion.CurrentValue is null)
                    {
                        rowVersion.CurrentValue = BitConverter.GetBytes(DateTime.UtcNow.Ticks);
                    }
                }
            }
        };

        return context;
    }

    private static async Task<(Screen Screen, Movie Movie, Show Show, Seat Seat, User User)> SeedStandardShowAsync(
        CinemaDbContext context,
        decimal basePrice = 15.50m)
    {
        var screen = CreateScreen(100, "Screen 1");
        var movie = CreateMovie(1000, "Test Movie", 12);
        var show = CreateShow(5000, movie.MovieId, screen.ScreenId, basePrice);
        var seat = CreateSeat(3000, screen.ScreenId, "A", 1);
        var user = CreateUser(2000, "test-user@example.com");

        context.AddRange(screen, movie, show, seat, user);
        await context.SaveChangesAsync();

        return (screen, movie, show, seat, user);
    }

    private static Screen CreateScreen(long id, string name) => new()
    {
        ScreenId = id,
        TheaterId = 1,
        Name = name,
        ScreenType = "Standard"
    };

    private static Seat CreateSeat(long id, long screenId, string rowLabel = "A", int seatNumber = 1) => new()
    {
        SeatId = id,
        ScreenId = screenId,
        RowLabel = rowLabel,
        SeatNumber = seatNumber,
        SeatType = "Standard"
    };

    private static Movie CreateMovie(long id, string title, int ageRating) => new()
    {
        MovieId = id,
        Title = title,
        AgeRating = ageRating
    };

    private static Show CreateShow(long id, long movieId, long screenId, decimal basePrice) => new()
    {
        ShowId = id,
        MovieId = movieId,
        ScreenId = screenId,
        StartAtUtc = DateTime.UtcNow.AddHours(1),
        EndAtUtc = DateTime.UtcNow.AddHours(3),
        BasePrice = basePrice,
        IsPeak = false
    };

    private static User CreateUser(long id, string email) => new()
    {
        UserId = id,
        Email = email
    };
}
