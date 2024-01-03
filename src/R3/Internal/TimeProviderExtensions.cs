namespace R3.Internal;

internal static class TimeProviderExtensions
{
    public static ITimer CreateStoppedTimer(this TimeProvider timeProvider, TimerCallback timerCallback, object? state)
    {
        return timeProvider.CreateTimer(timerCallback, state, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public static void RestartImmediately(this ITimer timer)
    {
        timer.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);
    }

    public static void InvokeOnce(this ITimer timer, TimeSpan dueTime)
    {
        timer.Change(dueTime, Timeout.InfiniteTimeSpan);
    }

    public static void Stop(this ITimer timer)
    {
        timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }
}


