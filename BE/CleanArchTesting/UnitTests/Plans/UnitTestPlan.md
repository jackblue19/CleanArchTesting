# Ticket Booking System Unit Test Plan (Domain + Application)

Scope: Domain + Application layers only. Excludes infrastructure/transport.

Cross-cutting principles
- Determinism: freeze time with FakeClock; stub randomness/IDs.
- Concurrency (unit-level): simulate locks/atomic checks via InMemoryLockService; verify call order and exactly-once writes (strict mocks and Moq sequences).
- No I/O, no threads; use fakes only. Optional FsCheck properties.
- Naming: Method_UnderTest_Condition_ExpectedResult.

---

## Application.UseCases.BookingService.HoldSeatsAsync
1) Purpose: Hold seats for a show with a short TTL to prevent overbooking.
2) Inputs: HoldSeatsRequest
   - ShowId: long > 0
   - UserId: long > 0
   - SeatIds: non-null, non-empty, all distinct
   - IdempotencyKey: optional, <= 64
3) Outputs: HoldSeatsResult { Success, Status: HELD|INVALID|NOT_FOUND|CONFLICT, Message, ReservationId?, HoldExpiresAtUtc? }
   - Exceptions: none; maps to statuses.
4) State changes: IReservationRepository.AddAsync per seat; ICinemaDbContext.SaveChangesAsync; no direct cache/lock (future extension).
5) Collaborators to mock: ICinemaDbContext (Shows, Seats), IReservationRepository, IClock.
6) Test cases
- Happy path
  - HoldSeats_Valid_SingleSeat_Held | Show+Seat exist and available | (1,2,[10]) | HELD with expiry=now+5m | GetActive returns null, Add called once, SaveChanges once |
  - HoldSeats_Valid_CouplePair_Held | Pair seats same group | (1,2,[20,21]) | HELD | Add twice | Enforce pair rule
- Boundary/edge
  - HoldSeats_SeatRace_SecondCallerGetsConflict | First acquires, second detects | [10] twice | First HELD, second CONFLICT | Second GetActive returns active | Use sequence
  - HoldSeats_TTL_Expiry_NotApplicableHere | TTL is only on confirm | n/a | n/a | n/a | TTL asserted later on confirm
- Negative
  - HoldSeats_Couple_MissingMate_Invalid | COUPLE_* only one picked | [20] | INVALID | No Add/Save |
  - HoldSeats_ShowNotFound | show missing | | NOT_FOUND | |
  - HoldSeats_SeatsNotFound | seats missing | | NOT_FOUND | |
  - HoldSeats_SeatsWrongScreen_Invalid | mismatch screen | | INVALID | |
  - HoldSeats_DuplicateSeatIds_Invalid | duplicates | | INVALID | Validator blocks
7) Data sets: seat counts 1,2,3; COUPLE_* with/without mate; distinct vs duplicate; screens equal/unequal.

---

## Application.UseCases.BookingService.ConfirmBookingAsync
1) Purpose: Confirm a HELD reservation, compute pricing/fees, apply voucher, mark BOOKED.
2) Inputs: ConfirmBookingRequest
   - ReservationId > 0
   - VoucherCode: optional <= 32
   - PaymentIntentId: optional <= 64
3) Outputs: ConfirmBookingResult { Success, Status: BOOKED|INVALID_STATE|EXPIRED|VOUCHER_INVALID|NOT_FOUND, Message, Subtotal, Discount, Total }
4) State changes: Updates Reservation (Subtotal, Discount, PaymentIntentId, Status=BOOKED); SaveChanges.
5) Collaborators to mock: IReservationRepository, ICinemaDbContext (Shows, Seats, Screens, PriceAdjustments), IVoucherService, IClock.
6) Test cases
- Happy path
  - Confirm_NoAdjustments_NoVoucher_Booked | HELD not expired | | BOOKED, Subtotal=BasePrice | SaveChanges once |
  - Confirm_Adjustments_Chain_PercentAndFixed | Mixed GLOBAL/SEAT_TYPE/SCREEN_TYPE | | Subtotal chained and rounded | |
  - Confirm_Voucher_Valid | Voucher valid | | Discount applied | voucher.Validate called with now
- Boundary/edge
  - Confirm_AlreadyBooked_Idempotent | Status=BOOKED | | Success true, returns stored totals | No SaveChanges
  - Confirm_ExpiredHold_Expired | HoldExpiresAtUtc <= now | | EXPIRED | Clock frozen
  - Confirm_RoundingPrecision | Many percentages | | 2dp rounding | Assertions at 2dp
- Negative
  - Confirm_NotHeld_InvalidState | Status != HELD/BOOKED | | INVALID_STATE | |
  - Confirm_Voucher_Invalid | Voucher invalid | | VOUCHER_INVALID with reason | |
  - Confirm_MissingReservation_NotFound | Repo null | | NOT_FOUND | |
7) Data sets: basePrice [0.01, 100, 199.99, 1000]; vouchers [percent, fixed, capped]; adjust modes [PERCENT,FIXED]; target [GLOBAL,SEAT_TYPE,SCREEN_TYPE].

---

## Application.UseCases.BookingService.ReleaseHoldAsync
1) Purpose: Release a HELD reservation and clear TTL.
2) Inputs: ReleaseHoldRequest { ReservationId > 0, Reason not blank }
3) Outputs: ReleaseHoldResult { Success, Status: RELEASED|NOT_FOUND|INVALID_STATE, Message }
4) State changes: Reservation.Status="RELEASED", HoldExpiresAtUtc=null; SaveChanges.
5) Collaborators: IReservationRepository, ICinemaDbContext.
6) Test cases: Valid release; NotHeld -> INVALID_STATE; NotFound.
7) Data sets: reasons (user-cancel, timeout, system), statuses.

---

## Application.Validators
- HoldSeatsRequestValidator.Validate
  - Inputs: seat ids (null/empty/duplicates), ShowId/UserId ranges
  - Outputs: (ok,bool, error)
- ConfirmBookingRequestValidator.Validate
  - Inputs: ReservationId, VoucherCode length, PaymentIntentId length
- ReleaseHoldRequestValidator.Validate
  - Inputs: ReservationId, Reason whitespace

Test cases: empty seats; duplicates; invalid ids; long strings; valid paths. Prefer [Theory] where applicable.

---

## Domain.ValueObjects.SeatPosition
1) Purpose: Immutable value object for seat location.
2) Inputs: (rowLabel: non-empty), (seatNumber: > 0)
3) Outputs: ToString => RowLabel+SeatNumber; Exceptions on invalid inputs.
4) State changes: none.
5) Collaborators: none.
6) Test cases: valid tostring; empty row throws; zero/negative seat throws.

---

## Planned (skeleton-only) — to be implemented later

### Application.Query/Search
- SearchShowsQueryHandler, GetSeatAvailabilityQueryHandler
  - Inputs: date range, theater, movieId; showId; paging
  - Outputs: lists/projections
  - Mocks: ICinemaDbContext views (VShowSeatAvailabilities, VShowOccupancies)
  - Tests: filters, paging, capacity invariant (FsCheck optional)

### Application.Payment Orchestration
- IPaymentGateway interface (authorize/capture/refund) consumed by a coordinator
  - Tests: decline, timeout, retries, idempotent capture, compensation upon partial failure
  - Mocks: gateway, message bus, clock; verify exactly-once state write

### Application.Idempotency
- IdempotencyService using Idempotencies table
  - Tests: key reuse returns same response; no duplicate writes; replay safety

### Application.Cancel/Refund/Rebook
- CancelBookingService, RebookService
  - Tests: refund ok/failed, rebook capacity checks, rollback on any failure

Data sets for planned: boundaries for pagination, limits, retry counts, gateway error codes.

---

Test Doubles available
- FakeClock: deterministic time control for expiry logic.
- InMemoryLockService: simulate coarse-grained locks for seat selection/booking at unit level.
