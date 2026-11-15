using Microsoft.Extensions.Time.Testing;

namespace Ecommerce.IntegrationTests.Fixtures;

// FakeTimeProvider cannot go back in time, so each test gets a fresh one behind this single registered instance
public sealed class TestTimeProvider : TimeProvider
{
    public FakeTimeProvider Current { get; private set; } = new();

    public void Reset(DateTimeOffset now) => Current = new FakeTimeProvider(now);

    public override DateTimeOffset GetUtcNow() => Current.GetUtcNow();

    public override long GetTimestamp() => Current.GetTimestamp();

    public override long TimestampFrequency => Current.TimestampFrequency;

    public override TimeZoneInfo LocalTimeZone => Current.LocalTimeZone;

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period) =>
        Current.CreateTimer(callback, state, dueTime, period);
}
