namespace R3;

public static partial class Observable
{
    // TODO: No Provider overload?

    public static Observable<Unit> Timer(TimeSpan dueTime, TimeProvider timeProvider)
    {
        return new Timer(dueTime, timeProvider);
    }
}

internal sealed class Timer : Observable<Unit>
{
    readonly TimeSpan dueTime;
    readonly TimeProvider timeProvider;

    public Timer(TimeSpan dueTime, TimeProvider timeProvider)
    {
        this.dueTime = dueTime;
        this.timeProvider = timeProvider;
    }

    protected override IDisposable SubscribeCore(Observer<Unit> observer)
    {
        var method = new _Timer(observer);
        method.Timer = timeProvider.CreateStoppedTimer(_Timer.timerCallback, method);
        method.Timer.InvokeOnce(dueTime);
        return method;
    }

    sealed class _Timer(Observer<Unit> observer) : IDisposable
    {
        public static readonly TimerCallback timerCallback = NextTick;

        Observer<Unit> observer = observer;

        public ITimer? Timer { get; set; }

        static void NextTick(object? state)
        {
            var self = (_Timer)state!;
            try
            {
                self.observer.OnNext(default);
                self.observer.OnCompleted();
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
