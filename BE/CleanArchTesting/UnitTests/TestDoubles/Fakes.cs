// UnitTests/TestDoubles/Fakes.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;

namespace UnitTests.TestDoubles;

public sealed class FakeClock : IClock
{
    public DateTime UtcNow { get; private set; }
    public FakeClock(DateTime start) => UtcNow = start;
    public void Advance(TimeSpan by) => UtcNow = UtcNow.Add(by);
}

// Example lock primitives for unit-level concurrency simulation in tests only
public interface ILockService
{
    Task<bool> AcquireAsync(string key, TimeSpan ttl, CancellationToken ct);
    Task ReleaseAsync(string key);
}

public sealed class InMemoryLockService : ILockService
{
    private readonly HashSet<string> _locks = new();
    public Task<bool> AcquireAsync(string key, TimeSpan ttl, CancellationToken ct)
    {
        lock (_locks)
        {
            return Task.FromResult(_locks.Add(key));
        }
    }
    public Task ReleaseAsync(string key)
    {
        lock (_locks)
        {
            _locks.Remove(key);
            return Task.CompletedTask;
        }
    }
}
