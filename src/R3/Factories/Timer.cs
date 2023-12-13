namespace R3;

public static partial class Event
{
    public static Event<Unit, Unit> Timer(TimeSpan dueTime, TimeProvider timeProvider)
    {
        return new Timer(dueTime, timeProvider);
    }
}

internal sealed class Timer : Event<Unit, Unit>
{
    readonly TimeSpan dueTime;
    readonly TimeProvider timeProvider;

    public Timer(TimeSpan dueTime, TimeProvider timeProvider)
    {
        this.dueTime = dueTime;
        this.timeProvider = timeProvider;
    }

    protected override IDisposable SubscribeCore(Subscriber<Unit, Unit> subscriber)
    {
        var method = new _Timer(subscriber);
        method.Timer = timeProvider.CreateStoppedTimer(_Timer.timerCallback, method);
        method.Timer.InvokeOnce(dueTime);
        return method;
    }

    sealed class _Timer(Subscriber<Unit, Unit> subscriber) : IDisposable
    {
        public static readonly TimerCallback timerCallback = NextTick;

        Subscriber<Unit, Unit> subscriber = subscriber;

        public ITimer? Timer { get; set; }

        static void NextTick(object? state)
        {
            var self = (_Timer)state!;
            try
            {
                self.subscriber.OnNext(default);
                self.subscriber.OnCompleted();
            }
            finally
            {
                self.Dispose();
            }
        }

        public void Dispose()
        {
            Timer?.Dispose();
            Timer = null;
        }
    }
}
