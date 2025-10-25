using System;
using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace UnitTests.Domain;

public class ShowTests
{
    private static Show CreateShow(DateTime? start = null, DateTime? end = null)
    {
        return new Show
        {
            ShowId = 1,
            MovieId = 1,
            ScreenId = 1,
            StartAtUtc = start ?? new DateTime(2025, 6, 1, 10, 0, 0, DateTimeKind.Utc),
            EndAtUtc = end ?? new DateTime(2025, 6, 1, 12, 30, 0, DateTimeKind.Utc),
            BasePrice = 12m,
            IsPeak = false
        };
    }

    [Fact]
    public void Duration_ComputesDifference()
    {
        var show = CreateShow();
        show.Duration.Should().Be(TimeSpan.FromMinutes(150));
    }

    [Fact]
    public void Overlaps_ReturnsTrue_WhenRangesOverlap()
    {
        var show = CreateShow();
        show.Overlaps(new DateTime(2025, 6, 1, 11, 0, 0, DateTimeKind.Utc), new DateTime(2025, 6, 1, 13, 0, 0, DateTimeKind.Utc)).Should().BeTrue();
    }

    [Fact]
    public void Overlaps_ReturnsFalse_WhenRangesDoNotOverlap()
    {
        var show = CreateShow();
        show.Overlaps(new DateTime(2025, 6, 1, 12, 30, 0, DateTimeKind.Utc), new DateTime(2025, 6, 1, 13, 30, 0, DateTimeKind.Utc)).Should().BeFalse();
    }

    [Fact]
    public void Overlaps_Throws_WhenEndBeforeStart()
    {
        var show = CreateShow();
        var action = () => show.Overlaps(new DateTime(2025, 6, 1, 9, 0, 0, DateTimeKind.Utc), new DateTime(2025, 6, 1, 9, 0, 0, DateTimeKind.Utc));
        action.Should().Throw<ArgumentException>();
    }
}
