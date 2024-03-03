namespace R3;

public sealed class SynchronizationContextTimerProvider : TimeProvider
{
    readonly SynchronizationContext? synchronizationContext;
    readonly TimeProvider timeProvider;

    public SynchronizationContextTimerProvider()
        : this(SynchronizationContext.Current)
    {
    }

    public SynchronizationContextTimerProvider(SynchronizationContext? synchronizationContext)
        : this(synchronizationContext, TimeProvider.System)
    {
    }

    public SynchronizationContextTimerProvider(SynchronizationContext? synchronizationContext, TimeProvider timeProvider)
    {
        this.synchronizationContext = synchronizationContext;
        this.timeProvider = timeProvider;
    }

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        return new SynchronizationContextTimerTimer(timeProvider, synchronizationContext, callback, state, dueTime, period);
    }
}

internal sealed class SynchronizationContextTimerTimer : ITimer
{
    static readonly TimerCallback wrappedCallback = InvokeCallback;
    static readonly SendOrPostCallback postCallback = PostCallback;

    readonly ITimer timer;
    readonly SynchronizationContext? synchronizationContext;
    readonly TimerCallback callback;
    readonly object? state;

    public SynchronizationContextTimerTimer(TimeProvider timeProvider, SynchronizationContext? synchronizationContext, TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        this.timer = timeProvider.CreateTimer(wrappedCallback, this, dueTime, period);
        this.synchronizationContext = synchronizationContext;
        this.callback = callback;
        this.state = state;
    }

    static void InvokeCallback(object? state)
    {
        var self = (SynchronizationContextTimerTimer)state!;

        if (self.synchronizationContext == null)
        {
            self.callback.Invoke(self.state);
        }
        else
        {
            self.synchronizationContext.Post(postCallback, self);
        }
    }

    static void PostCallback(object? state)
    {
        var self = (SynchronizationContextTimerTimer)state!;
        self.callback.Invoke(self.state);
    }

    public bool Change(TimeSpan dueTime, TimeSpan period)
    {
        return this.timer.Change(dueTime, period);
    }

    public void Dispose()
    {
        this.timer.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        this.timer.Dispose();
        return default;
    }
}
