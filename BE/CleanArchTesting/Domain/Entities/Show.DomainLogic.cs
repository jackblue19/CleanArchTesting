using System;

namespace Domain.Entities;

public partial class Show
{
    public TimeSpan Duration => EndAtUtc - StartAtUtc;

    public bool Overlaps(DateTime start, DateTime end)
    {
        if (end <= start)
        {
            throw new ArgumentException("End must be after start.", nameof(end));
        }

        return start < EndAtUtc && end > StartAtUtc;
    }
}
