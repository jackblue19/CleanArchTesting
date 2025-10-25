USE master;
go

CREATE DATABASE Cinema;
GO

Use Cinema;
go


-- ============================================================================
-- Core Catalogs
-- ============================================================================

CREATE TABLE dbo.Users (
    UserId       BIGINT IDENTITY(1,1) PRIMARY KEY,
    Email        NVARCHAR(256) NOT NULL UNIQUE
);

CREATE TABLE dbo.Movies (
    MovieId      BIGINT IDENTITY(1,1) PRIMARY KEY,
    Title        NVARCHAR(256) NOT NULL,
    AgeRating    INT NOT NULL
);

CREATE TABLE dbo.Screens (
    ScreenId     BIGINT IDENTITY(1,1) PRIMARY KEY,
    TheaterId    BIGINT NOT NULL,
    [Name]       NVARCHAR(128) NOT NULL,
    ScreenType   NVARCHAR(16)  NOT NULL  -- 2D / 3D / IMAX
);

CREATE TABLE dbo.Seats (
    SeatId            BIGINT IDENTITY(1,1) PRIMARY KEY,
    ScreenId          BIGINT NOT NULL REFERENCES dbo.Screens(ScreenId),
    RowLabel          NVARCHAR(8) NOT NULL,
    SeatNumber        INT NOT NULL,
    SeatType          NVARCHAR(16) NOT NULL,  -- STANDARD / VIP / COUPLE_LEFT / COUPLE_RIGHT
    SeatPairGroupId   BIGINT NULL,            -- NOT NULL iff COUPLE_*
    CONSTRAINT UQ_Seat UNIQUE (ScreenId, RowLabel, SeatNumber),
    CONSTRAINT CK_SeatType CHECK (SeatType IN ('STANDARD','VIP','COUPLE_LEFT','COUPLE_RIGHT')),
    CONSTRAINT CK_SeatPairConsistency CHECK (
        (SeatType LIKE 'COUPLE_%' AND SeatPairGroupId IS NOT NULL)
        OR
        (SeatType NOT LIKE 'COUPLE_%' AND SeatPairGroupId IS NULL)
    )
);

CREATE TABLE dbo.Shows (
    ShowId       BIGINT IDENTITY(1,1) PRIMARY KEY,
    MovieId      BIGINT NOT NULL REFERENCES dbo.Movies(MovieId),
    ScreenId     BIGINT NOT NULL REFERENCES dbo.Screens(ScreenId),
    StartAtUtc   DATETIME2(0) NOT NULL,
    EndAtUtc     DATETIME2(0) NOT NULL,
    BasePrice    DECIMAL(10,2) NOT NULL,
    IsPeak       BIT NOT NULL DEFAULT 0
);

-- ============================================================================
-- Pricing (đủ dùng cho rule cơ bản + voucher)
-- ============================================================================

-- Điều chỉnh giá theo loại ghế / loại màn hình / toàn cục cho show
CREATE TABLE dbo.PriceAdjustments (
    AdjustmentId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ShowId       BIGINT NOT NULL REFERENCES dbo.Shows(ShowId),
    Target       NVARCHAR(16) NOT NULL,   -- GLOBAL / SEAT_TYPE / SCREEN_TYPE
    [Mode]       NVARCHAR(16) NOT NULL,   -- PERCENT / FIXED
    [Key]        NVARCHAR(16) NOT NULL,   -- 'VIP' / 'IMAX' / '*'
    Amount       DECIMAL(10,2) NOT NULL
);

-- Voucher/coupon tối giản (một booking dùng tối đa 1 voucher)
CREATE TABLE dbo.Vouchers (
    VoucherId    BIGINT IDENTITY(1,1) PRIMARY KEY,
    Code         NVARCHAR(32) NOT NULL UNIQUE,
    [Type]       NVARCHAR(10) NOT NULL,       -- PERCENT / FIXED
    [Value]      DECIMAL(10,2) NOT NULL,
    MaxDiscount  DECIMAL(10,2) NULL,
    MinSpend     DECIMAL(10,2) NULL,
    ValidFromUtc DATETIME2(0) NOT NULL,
    ValidToUtc   DATETIME2(0) NOT NULL,
    Active       BIT NOT NULL DEFAULT 1,
    CONSTRAINT CK_VoucherType CHECK ([Type] IN ('PERCENT','FIXED'))
);

-- ============================================================================
-- Reservations = nơi ghi nhận HOLD/BOOKED/EXPIRED/RELEASED (chống overbooking)
-- ============================================================================

CREATE TABLE dbo.Reservations (
    ReservationId     BIGINT IDENTITY(1,1) PRIMARY KEY,
    ShowId            BIGINT NOT NULL REFERENCES dbo.Shows(ShowId),
    SeatId            BIGINT NOT NULL REFERENCES dbo.Seats(SeatId),
    UserId            BIGINT NOT NULL REFERENCES dbo.Users(UserId),

    [Status]          NVARCHAR(12) NOT NULL,  -- HELD / BOOKED / EXPIRED / RELEASED
    CreatedAtUtc      DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    HoldExpiresAtUtc  DATETIME2(0) NULL,      -- deadline thanh toán khi HELD

    -- Giá sẽ được "đóng băng" khi BOOKED (Total là cột tính)
    Subtotal          DECIMAL(10,2) NOT NULL DEFAULT (0),
    Discount          DECIMAL(10,2) NOT NULL DEFAULT (0),
    Total AS (CASE WHEN Subtotal - Discount < 0 THEN 0 ELSE Subtotal - Discount END) PERSISTED,

    PaymentIntentId   NVARCHAR(64) NULL,
    IdempotencyKey    NVARCHAR(64) NULL,

    RowVersion        ROWVERSION,

    CONSTRAINT CK_ResStatus CHECK ([Status] IN ('HELD','BOOKED','EXPIRED','RELEASED')),
    CONSTRAINT CK_ResHeldHasExpiry CHECK (
        [Status] <> 'HELD' OR HoldExpiresAtUtc IS NOT NULL
    ),
    CONSTRAINT CK_ResMoneyNonNegative CHECK (Subtotal >= 0 AND Discount >= 0)
);

-- Ghế của một suất chiếu chỉ có tối đa 1 dòng HELD/BOOKED tại mọi thời điểm
CREATE UNIQUE INDEX UX_Reservations_UniqueActive
    ON dbo.Reservations(ShowId, SeatId)
    WHERE [Status] IN ('HELD','BOOKED');

CREATE INDEX IX_Reservations_Show_Status ON dbo.Reservations(ShowId, [Status]);
CREATE INDEX IX_Reservations_HoldExpiry   ON dbo.Reservations([Status], HoldExpiresAtUtc);

-- 1 booking ↔ 1 voucher (nếu muốn cho nhiều, bỏ UNIQUE)
CREATE TABLE dbo.VoucherRedemptions (
    RedemptionId     BIGINT IDENTITY(1,1) PRIMARY KEY,
    ReservationId    BIGINT NOT NULL REFERENCES dbo.Reservations(ReservationId),
    VoucherId        BIGINT NOT NULL REFERENCES dbo.Vouchers(VoucherId),
    RedeemedAtUtc    DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    DiscountApplied  DECIMAL(10,2) NOT NULL,
    CONSTRAINT UQ_VoucherPerReservation UNIQUE (ReservationId)
);

-- ============================================================================
-- Idempotency store (giữ kết quả cho cùng 1 request key)
-- ============================================================================

CREATE TABLE dbo.Idempotency (
    [Key]           NVARCHAR(64) NOT NULL PRIMARY KEY,
    CreatedAtUtc    DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    RequestType     NVARCHAR(16) NOT NULL,      -- HOLD / CONFIRM
    ResultStatus    NVARCHAR(32) NOT NULL,      -- SUCCESS / PAYMENT_FAILED / EXPIRED / CONFLICT / ERROR
    Payload         NVARCHAR(MAX) NULL          -- tuỳ bạn lưu JSON kết quả để trả lại
);

-- ============================================================================
-- Views phục vụ test/integration (đếm FREE/HELD/BOOKED nhanh gọn)
-- ============================================================================

-- === v_ShowSeatAvailability ===
IF OBJECT_ID(N'dbo.v_ShowSeatAvailability', N'V') IS NOT NULL
    DROP VIEW dbo.v_ShowSeatAvailability;
GO
CREATE VIEW dbo.v_ShowSeatAvailability
AS
SELECT
    s.ScreenId,
    sh.ShowId,
    s.SeatId,
    s.RowLabel,
    s.SeatNumber,
    s.SeatType,
    CASE
        WHEN EXISTS (SELECT 1 FROM dbo.Reservations r
                     WHERE r.ShowId = sh.ShowId AND r.SeatId = s.SeatId
                       AND r.[Status] = 'BOOKED') THEN 'BOOKED'
        WHEN EXISTS (SELECT 1 FROM dbo.Reservations r
                     WHERE r.ShowId = sh.ShowId AND r.SeatId = s.SeatId
                       AND r.[Status] = 'HELD') THEN 'HELD'
        ELSE 'FREE'
    END AS SeatState
FROM dbo.Seats s
JOIN dbo.Screens sc ON sc.ScreenId = s.ScreenId
JOIN dbo.Shows   sh ON sh.ScreenId = sc.ScreenId;
GO

-- === v_ShowOccupancy (phụ thuộc v_ShowSeatAvailability) ===
IF OBJECT_ID(N'dbo.v_ShowOccupancy', N'V') IS NOT NULL
    DROP VIEW dbo.v_ShowOccupancy;
GO
CREATE VIEW dbo.v_ShowOccupancy
AS
SELECT
    v.ShowId,
    SUM(CASE WHEN v.SeatState = 'FREE'   THEN 1 ELSE 0 END) AS FreeSeats,
    SUM(CASE WHEN v.SeatState = 'HELD'   THEN 1 ELSE 0 END) AS HeldSeats,
    SUM(CASE WHEN v.SeatState = 'BOOKED' THEN 1 ELSE 0 END) AS BookedSeats,
    COUNT(*) AS TotalSeats,
    CAST(
        (100.0 * SUM(CASE WHEN v.SeatState IN ('HELD','BOOKED') THEN 1 ELSE 0 END))
        / NULLIF(COUNT(*),0) AS DECIMAL(5,2)
    ) AS OccupancyPercent
FROM dbo.v_ShowSeatAvailability v
GROUP BY v.ShowId;
GO


-- CREATE VIEW dbo.v_ShowSeatAvailability
-- AS
-- SELECT
--     s.ScreenId,
--     sh.ShowId,
--     s.SeatId,
--     s.RowLabel,
--     s.SeatNumber,
--     s.SeatType,
--     CASE
--         WHEN EXISTS (SELECT 1 FROM dbo.Reservations r
--                      WHERE r.ShowId = sh.ShowId AND r.SeatId = s.SeatId
--                        AND r.[Status] = 'BOOKED') THEN 'BOOKED'
--         WHEN EXISTS (SELECT 1 FROM dbo.Reservations r
--                      WHERE r.ShowId = sh.ShowId AND r.SeatId = s.SeatId
--                        AND r.[Status] = 'HELD') THEN 'HELD'
--         ELSE 'FREE'
--     END AS SeatState
-- FROM dbo.Seats s
-- JOIN dbo.Screens sc ON sc.ScreenId = s.ScreenId
-- JOIN dbo.Shows   sh ON sh.ScreenId = sc.ScreenId;
-- GO

-- CREATE VIEW dbo.v_ShowOccupancy
-- AS
-- SELECT
--     sh.ShowId,
--     SUM(CASE WHEN v.SeatState='FREE'   THEN 1 ELSE 0 END) AS FreeSeats,
--     SUM(CASE WHEN v.SeatState='HELD'   THEN 1 ELSE 0 END) AS HeldSeats,
--     SUM(CASE WHEN v.SeatState='BOOKED' THEN 1 ELSE 0 END) AS BookedSeats,
--     COUNT(*) AS TotalSeats,
--     CAST( (100.0 * SUM(CASE WHEN v.SeatState IN ('HELD','BOOKED') THEN 1 ELSE 0 END)) / NULLIF(COUNT(*),0) AS DECIMAL(5,2) ) AS OccupancyPercent
-- FROM dbo.v_ShowSeatAvailability v
-- JOIN dbo.Shows sh ON sh.ShowId = v.ShowId
-- GROUP BY sh.ShowId;
-- GO
