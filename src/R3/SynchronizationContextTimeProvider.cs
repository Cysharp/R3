namespace R3;

public sealed class SynchronizationContextTimeProvider : TimeProvider
{
    readonly Func<SynchronizationContext?> synchronizationContextAccessor;
    readonly TimeProvider timeProvider;

    public SynchronizationContextTimeProvider()
        : this(SynchronizationContext.Current)
    {
    }

    public SynchronizationContextTimeProvider(SynchronizationContext? synchronizationContext)
        : this(synchronizationContext, TimeProvider.System)
    {
    }

    public SynchronizationContextTimeProvider(Func<SynchronizationContext?> synchronizationContextAccessor)
        : this(synchronizationContextAccessor, TimeProvider.System)
    {
    }

    public SynchronizationContextTimeProvider(SynchronizationContext? synchronizationContext, TimeProvider timeProvider)
    {
        this.synchronizationContextAccessor = () => synchronizationContext;
        this.timeProvider = timeProvider;
    }

    public SynchronizationContextTimeProvider(Func<SynchronizationContext?> synchronizationContextAccessor, TimeProvider timeProvider)
    {
        this.synchronizationContextAccessor = synchronizationContextAccessor;
        this.timeProvider = timeProvider;
    }

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        return new SynchronizationContextTimer(timeProvider, synchronizationContextAccessor(), callback, state, dueTime, period);
    }
}

internal sealed class SynchronizationContextTimer : ITimer
{
    static readonly TimerCallback wrappedCallback = InvokeCallback;
    static readonly SendOrPostCallback postCallback = PostCallback;

    readonly ITimer timer;
    readonly SynchronizationContext? synchronizationContext;
    readonly TimerCallback callback;
    readonly object? state;

    public SynchronizationContextTimer(TimeProvider timeProvider, SynchronizationContext? synchronizationContext, TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        this.synchronizationContext = synchronizationContext;
        this.callback = callback;
        this.state = state;

        // CreateTimer call after all private field set
        this.timer = timeProvider.CreateTimer(wrappedCallback, this, dueTime, period);
    }

    static void InvokeCallback(object? state)
    {
        var self = (SynchronizationContextTimer)state!;

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
        var self = (SynchronizationContextTimer)state!;
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
