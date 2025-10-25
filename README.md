# CleanArchTesting Unit Test Suite

Cinema booking service covering seat reservations, bookings, and voucher redemptions. This document focuses on the Domain + Application unit test strategy that supports the core BookingService workflows and is intended to help readers replicate the same approach in their own codebases.

---

## Scope

- Target layers: Application (use cases, validators) and Domain (value objects, domain events).
- Excluded: Infrastructure adapters, transport layers, integration tests.
- Deterministic execution only: freeze time, stub randomness/IDs, no external I/O or threading.

## Core Feature Coverage

The unit test suite exercises the behavior of the BookingService orchestration layer and the most critical validation/value object rules in the domain.

- `BookingService.HoldSeatsAsync` – reserves seats with a short hold TTL while enforcing seating policies (distinct seat ids, couple seating pairing, screen alignment, availability race detection).
- `BookingService.ConfirmBookingAsync` – promotes held reservations to confirmed bookings, calculates pricing adjustments, applies voucher discounts, and preserves idempotent replays.
- `BookingService.ReleaseHoldAsync` – releases previously held reservations, clearing the TTL and communicating cancellation reasons.
- Validators (`HoldSeatsRequestValidator`, `ConfirmBookingRequestValidator`, `ReleaseHoldRequestValidator`) – guard application boundaries before any repository interaction.
- Domain value object `SeatPosition` – ensures seat coordinates remain valid and easily readable.
- Domain events (`SeatsHeldEvent`, `SeatsBookedEvent`, `SeatsReleasedEvent`) – helper APIs used by services for assertions and transformations.

## Tech Stack

- .NET 8, C# 12
- xUnit
- Moq (strict behavior)
- Moq.EntityFrameworkCore for DbSet stubbing
- Custom fakes: `FakeClock`, `InMemoryLockService`

## Cross-Cutting Test Principles

- Determinism: use `FakeClock` and deterministic IDs.
- Concurrency simulation: `InMemoryLockService`, Moq `InSequence` to assert ordering and exactly-once writes.
- Validation-first: reject invalid requests prior to repository calls.
- Naming convention: `Method_UnderTest_Condition_ExpectedResult`.

## Project Layout

- `BE/CleanArchTesting/Application` – Requests, DTOs, use cases, validators (system under test).
- `BE/CleanArchTesting/Domain` – Entities, value objects, domain events.
- `BE/CleanArchTesting/Infrastructure` – Persistence wiring, repositories, external services (out of unit-test scope but referenced by interfaces).
- `BE/CleanArchTesting/Cinema.API` – Minimal API shell hosting the use cases (outside unit-test scope, helpful for context).
- `BE/CleanArchTesting/UnitTests` – xUnit project with per-feature folders (`BookingServiceTests`, `Validators`, `Domain`). Shared fixtures live under `TestDoubles`.

Keeping tests aligned with the production namespace structure allows quick navigation between source and coverage.

## Solution Structure

The solution groups projects by responsibility. Use this as a map when exploring or porting the suite to another repository.

- `CleanArchTesting.sln` – ties together Domain, Application, Infrastructure, API, and test projects.
- `Application/Application.csproj` – application layer orchestrating use cases and validators.
- `Domain/Domain.csproj` – pure domain entities, value objects, and events.
- `Infrastructure/Infrastructure.csproj` – adapters for data access/external services (kept out of unit tests but referenced via interfaces).
- `Cinema.API/Cinema.API.csproj` – ASP.NET Core host wiring the Application layer.
- `UnitTests/UnitTests.csproj` – xUnit project using Moq and supporting fakes.
- `IntegrationTests/IntegrationTests.csproj` – placeholder for higher-level verification (not covered here).
- `DB/` – SQL scripts and notes backing the cinema schema.
- `FE/` – demo front-end artifacts.

High-level folder view:

```
CleanArchTesting-Tree/
+- BE/
	+- CleanArchTesting/
		|- CleanArchTesting.sln
		+- Application/
		|  |- DTOs/
		|  |  `- BookingDtos.cs
		|  |- UseCases/
		|  |  `- BookingService.cs
		|  `- Validators/
		|     |- HoldSeatsRequestValidator.cs
		|     |- ConfirmBookingRequestValidator.cs
		|     `- ReleaseHoldRequestValidator.cs
		+- Domain/
		|  |- Entities/
		|  |  |- Reservation.cs
		|  |  |- Seat.cs
		|  |  `- Voucher.cs
		|  |- DomainEvents/
		|  |  `- ReservationEvents.cs
		|  `- ValueObjects/
		|     |- Money.cs
		|     `- SeatPosition.cs
		+- Infrastructure/
		|  |- DependencyInjection.cs
		|  `- Repositories/
		+- Cinema.API/
		|  |- Program.cs
		|  `- Controllers/
		+- UnitTests/
		|  |- BookingServiceTests/
		|  |- Validators/
		|  |- Domain/
		|  `- TestDoubles/
		`- IntegrationTests/
			`- UnitTest1.cs
+- DB/
|  `- Cinema.sql
`- FE/
	`- index.html
```

Replace ellipses with additional feature folders as you extend coverage. Keep the README updated when new projects or test areas are introduced.

## Running the Tests

From the repository root:

```powershell
cd BE\CleanArchTesting\UnitTests
dotnet test
```

All mocks should be strict. A failed verification indicates either missing coverage or a behavior change that needs attention.

## Test Doubles

- `FakeClock` – deterministic `DateTime` injection for expiry logic.
- `InMemoryLockService` – coarse lock simulation for seat hold contention.
- Strict Moq mocks for repositories, services, and EF Core contexts (`UseDbSet` helpers from Moq.EntityFrameworkCore).

## Unit Test Implementation Guide

1. **Arrange**
   - Create strict mocks for `ICinemaDbContext`, `IReservationRepository`, `IVoucherService`, and `IClock`.
   - Seed context DbSets using `Moq.EntityFrameworkCore` so the use case can query shows, seats, or adjustments without touching a real database.
   - Configure `FakeClock` for deterministic time-sensitive assertions (hold expiry, voucher validation).
2. **Act**
   - Call the relevant BookingService method or validator with well-structured request objects. Prefer `[Theory]`/`[InlineData]` to cover equivalence classes.
3. **Assert**
   - Inspect result DTOs (`Status`, `Message`, `Totals`, `HoldExpiresAtUtc`).
   - Verify repository writes (`AddAsync`, `SaveChangesAsync`) occur exactly once when expected and not at all on validation failures.
   - Use `VerifyNoOtherCalls` to detect unintended interactions.

Repeat the pattern for validators and domain objects: instantiate directly, feed edge-case data, assert results or thrown exceptions.

---

## Test Inventory

### BookingService.HoldSeatsAsync

Purpose: Hold seats for a brief TTL to prevent overbooking.

| Name                                        | Preconditions                              | Inputs                         | Expected output/exception                | Mock setup & expectations                                          | Notes                             |
| ------------------------------------------- | ------------------------------------------ | ------------------------------ | ---------------------------------------- | ------------------------------------------------------------------ | --------------------------------- |
| HoldSeats_Valid_SingleSeat_Held             | Show and seat exist, seat free             | ShowId=1, UserId=2, Seats=[10] | `Status=HELD`, `HoldExpiresAtUtc=now+5m` | `GetActive` returns null, `AddAsync` once, `SaveChangesAsync` once | Baseline happy path               |
| HoldSeats_Valid_CouplePair_Held             | Couple seats available                     | Seats=[20,21]                  | `Status=HELD`                            | `AddAsync` twice, `SaveChangesAsync` once                          | Ensures pair rule                 |
| HoldSeats_SeatRace_SecondCallerGetsConflict | First hold already present                 | Seats=[10] (twice)             | First `HELD`, second `CONFLICT`          | `GetActive` returns null then active; `AddAsync` only for first    | Use `InSequence` to enforce order |
| HoldSeats_Couple_MissingMate_Invalid        | Seat type requires pair but second missing | Seats=[20]                     | `Status=INVALID`                         | No repository calls                                                | Validator rejection path          |
| HoldSeats_ShowNotFound_NotFound             | Show missing                               | ShowId not found               | `Status=NOT_FOUND`                       | DB set returns null; no writes                                     |                                   |
| HoldSeats_SeatsWrongScreen_Invalid          | Seat belongs to different screen           | Seat list mismatched           | `Status=INVALID`                         | `GetSeats` returns mismatched screen; no writes                    |                                   |
| HoldSeats_DuplicateSeatIds_Invalid          | Duplicate ids                              | Seats=[10,10]                  | `Status=INVALID`                         | Validator rejects                                                  | Covers distinct constraint        |

### BookingService.ConfirmBookingAsync

Purpose: Confirm a held reservation, apply pricing and voucher adjustments, and mark as booked.

| Name                                      | Preconditions                             | Inputs                                                  | Expected output/exception                    | Mock setup & expectations                                                | Notes                                            |
| ----------------------------------------- | ----------------------------------------- | ------------------------------------------------------- | -------------------------------------------- | ------------------------------------------------------------------------ | ------------------------------------------------ |
| Confirm_NoAdjustments_NoVoucher_Booked    | Reservation held, not expired             | ReservationId=1001                                      | `Status=BOOKED`, totals match base price     | Repo returns held reservation, `SaveChangesAsync` once                   | Minimal happy path                               |
| Confirm_Adjustments_Chain_PercentAndFixed | Reservation held, price adjustments exist | Adjustments: GLOBAL +10%, SCREEN IMAX +5, SEAT VIP +20% | Correct subtotal rounded to 2dp              | Mock DbSets for adjustments, `SaveChangesAsync` once                     | Ensures order & rounding                         |
| Confirm_Voucher_ValidPercent              | Voucher accepted                          | VoucherCode=VIP10                                       | Discount applied, `Status=BOOKED`            | `IVoucherService.ValidateAsync` returns success; `SaveChangesAsync` once | Include fixed/capped variants via `[InlineData]` |
| Confirm_AlreadyBooked_Idempotent          | Reservation already `BOOKED`              | ReservationId=1001                                      | Returns stored totals, no writes             | Repo returns booked reservation; no `SaveChangesAsync`                   | Ensures idempotency                              |
| Confirm_ExpiredHold_Expired               | Hold TTL passed                           | `HoldExpiresAtUtc <= now`                               | `Status=EXPIRED`                             | `FakeClock` set beyond expiry; no writes                                 |                                                  |
| Confirm_NotHeld_InvalidState              | Status not HELD/BOOKED                    | Reservation status `RELEASED`                           | `Status=INVALID_STATE`                       | Repo returns entity; no writes                                           |                                                  |
| Confirm_Voucher_Invalid                   | Voucher fails validation                  | VoucherCode invalid                                     | `Status=VOUCHER_INVALID`, message propagated | `IVoucherService.ValidateAsync` returns failure; no state change         |                                                  |
| Confirm_MissingReservation_NotFound       | Repo returns null                         | ReservationId unmatched                                 | `Status=NOT_FOUND`                           | `FindAsync` returns null                                                 |                                                  |

### BookingService.ReleaseHoldAsync

Purpose: Release a held reservation and clear TTL.

| Name                                  | Preconditions               | Inputs                                    | Expected output/exception      | Mock setup & expectations                              | Notes      |
| ------------------------------------- | --------------------------- | ----------------------------------------- | ------------------------------ | ------------------------------------------------------ | ---------- |
| ReleaseHold_Valid_ReleasesReservation | Reservation held            | ReservationId valid, Reason="user-cancel" | `Status=RELEASED`, TTL cleared | Repo returns held reservation, `SaveChangesAsync` once | Happy path |
| ReleaseHold_NotHeld_InvalidState      | Reservation status not HELD | Reservation status `BOOKED`               | `Status=INVALID_STATE`         | Repo returns booked reservation; no writes             |            |
| ReleaseHold_NotFound                  | Reservation missing         | ReservationId missing                     | `Status=NOT_FOUND`             | Repo returns null                                      |            |

### Validators

| Validator                        | Purpose                                                                         | Key cases                                                                 |
| -------------------------------- | ------------------------------------------------------------------------------- | ------------------------------------------------------------------------- |
| `HoldSeatsRequestValidator`      | Ensure seat hold requests contain valid ids, seat lists, idempotency key length | `[InlineData]` for null/empty seats, duplicates, negative IDs, valid case |
| `ConfirmBookingRequestValidator` | Guard confirm booking inputs                                                    | ReservationId <= 0, VoucherCode > 32, PaymentIntentId > 64, valid         |
| `ReleaseHoldRequestValidator`    | Guard release hold inputs                                                       | ReservationId <= 0, Reason whitespace/empty, valid                        |

### Domain.ValueObjects.SeatPosition

Purpose: Immutable seat location representation.

| Name                                       | Preconditions  | Inputs           | Expected output/exception     | Notes                   |
| ------------------------------------------ | -------------- | ---------------- | ----------------------------- | ----------------------- |
| SeatPosition_ToString_ReturnsConcatenation | Valid row/seat | Row="B", Seat=12 | `"B12"`                       | Happy path              |
| SeatPosition_EmptyRow_Throws               | Row empty      | Row=""           | `ArgumentException`           | Validate message text   |
| SeatPosition_NonPositiveSeat_Throws        | Seat <= 0      | Seat=0 or -1     | `ArgumentOutOfRangeException` | Cover zero and negative |

### Domain.DomainEvents

- `SeatsHeldEvent`, `SeatsBookedEvent`, `SeatsReleasedEvent` – verify helper methods (seat membership, user match, expiry evaluation) if events are consumed by domain logic.

---

## xUnit Skeletons

### Example: HoldSeats_Valid_SingleSeat_Held

```csharp
[Fact]
public async Task HoldSeats_Valid_SingleSeat_Held()
{
	// Arrange
	var clock = new FakeClock(DateTime.UtcNow);
	var repoMock = new Mock<IReservationRepository>(MockBehavior.Strict);
	var dbContextMock = new Mock<ICinemaDbContext>(MockBehavior.Strict);
	var bookingService = new BookingService(dbContextMock.Object, repoMock.Object, clock);

	// TODO: Seed DbSets via Moq.EntityFrameworkCore and set expectations

	var request = new HoldSeatsRequest
	{
		ShowId = 1,
		UserId = 2,
		SeatIds = new[] { 10L }
	};

	// Act
	var result = await bookingService.HoldSeatsAsync(request, CancellationToken.None);

	// Assert
	result.Status.Should().Be(HoldSeatsStatus.Held);
	repoMock.VerifyAll();
	dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
}
```

Follow similar Arrange–Act–Assert structure for each scenario, using `[Theory]` with `[InlineData]` for data-driven tests.

---

## Adapting the Approach to Your Project

1. Mirror the production namespace hierarchy inside your test project to keep features discoverable.
2. Collect the external dependencies for each use case (repositories, services, clocks) and wrap them with strict mocks so unnoticed interactions fail fast.
3. Introduce deterministic helpers early (fake clock, id generator) to avoid brittle assertions.
4. Write an explicit test plan (like the `UnitTests/Plans/UnitTestPlan.md`) to catalog scenarios before implementation; keep the plan synchronized with coverage metrics.
5. Treat validators and domain primitives as first-class citizens—unit test them in isolation to prevent invalid data from entering use cases.

By following the pattern above, teams with an existing Domain/Application stack can bootstrap high-value tests rapidly while maintaining clarity about scope and behavior.

---

## Planned Extensions (Skeleton Only)

- **Query/Search:** `SearchShowsQueryHandler`, `GetSeatAvailabilityQueryHandler` – focus on filters, paging, and capacity invariants using DbView fakes.
- **Payment Orchestration:** `IPaymentGateway` workflows covering declines, timeouts, retries, and compensation logic.
- **Idempotency:** `IdempotencyService` to ensure key reuse returns cached responses and prevents duplicate writes.
- **Cancel/Refund/Rebook:** Service suite validating capacity checks, refunds, and rollback behavior.

Document updates as coverage expands. Keep the README synchronized with the implemented test inventory.
