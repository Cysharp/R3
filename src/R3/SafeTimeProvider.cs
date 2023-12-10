namespace R3;

public sealed class SafeTimerTimeProvider : TimeProvider
{
    readonly TimeProvider timeProvider;
    readonly Action<Exception> unhandledExceptionHandler;
    readonly TimerCallback wrappedCallback;

    internal bool IsSystemTimeProvider => timeProvider == TimeProvider.System;
    internal Action<Exception> UnhandledExceptionHandler => unhandledExceptionHandler;

    public SafeTimerTimeProvider(TimeProvider timeProvider, Action<Exception> unhandledExceptionHandler)
    {
        this.timeProvider = timeProvider;
        this.unhandledExceptionHandler = unhandledExceptionHandler;
        this.wrappedCallback = CallbackWithHandleUnhandledException;
    }

    public override long GetTimestamp() => timeProvider.GetTimestamp();

    public override DateTimeOffset GetUtcNow() => timeProvider.GetUtcNow();

    public override TimeZoneInfo LocalTimeZone => timeProvider.LocalTimeZone;

    public override long TimestampFrequency => timeProvider.TimestampFrequency;

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        return timeProvider.CreateTimer(wrappedCallback, Tuple.Create(callback, state), dueTime, period);
    }

    void CallbackWithHandleUnhandledException(object? state)
    {
        try
        {
            var (originalCallback, originalState) = (Tuple<TimerCallback, object?>)state!;
            originalCallback.Invoke(originalState);
        }
        catch (Exception ex)
        {
            try
            {
                unhandledExceptionHandler(ex);
            }
            catch
            {
                // ignore
            }
        }
    }
}
