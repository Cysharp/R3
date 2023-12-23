namespace R3;

public static partial class Observable
{
    public static Observable<Unit> Interval(TimeSpan period, CancellationToken cancellationToken = default)
    {
        return Timer(period, period, cancellationToken);
    }

    public static Observable<Unit> Interval(TimeSpan period, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        return Timer(period, period, timeProvider, cancellationToken);
    }

    public static Observable<Unit> Timer(TimeSpan dueTime, CancellationToken cancellationToken = default)
    {
        return Timer(dueTime, ObservableSystem.DefaultTimeProvider, cancellationToken);
    }

    public static Observable<Unit> Timer(DateTimeOffset dueTime, CancellationToken cancellationToken = default)
    {
        return Timer(dueTime, ObservableSystem.DefaultTimeProvider, cancellationToken);
    }

    public static Observable<Unit> Timer(TimeSpan dueTime, TimeSpan period, CancellationToken cancellationToken = default)
    {
        if (period < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(period));
        return Timer(dueTime, period, ObservableSystem.DefaultTimeProvider, cancellationToken);
    }

    public static Observable<Unit> Timer(DateTimeOffset dueTime, TimeSpan period, CancellationToken cancellationToken = default)
    {
        if (period < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(period));
        return Timer(dueTime, period, ObservableSystem.DefaultTimeProvider, cancellationToken);
    }

    public static Observable<Unit> Timer(TimeSpan dueTime, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        return new Timer(dueTime, null, timeProvider, cancellationToken);
    }

    public static Observable<Unit> Timer(DateTimeOffset dueTime, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        return new Timer(dueTime, null, timeProvider, cancellationToken);
    }

    public static Observable<Unit> Timer(TimeSpan dueTime, TimeSpan period, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        if (period < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(period));
        return new Timer(dueTime, period, timeProvider, cancellationToken);
    }

    public static Observable<Unit> Timer(DateTimeOffset dueTime, TimeSpan period, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        if (period < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(period));
        return new Timer(dueTime, period, timeProvider, cancellationToken);
    }
}

internal sealed class Timer : Observable<Unit>
{
    readonly TimeSpan? dueTime1;
    readonly DateTimeOffset? dueTime2;
    readonly TimeSpan? period;
    readonly TimeProvider timeProvider;
    readonly CancellationToken cancellationToken;

    public Timer(TimeSpan dueTime, TimeSpan? period, TimeProvider timeProvider, CancellationToken cancellationToken)
    {
        this.dueTime1 = dueTime;
        this.period = period;
        this.timeProvider = timeProvider;
        this.cancellationToken = cancellationToken;
    }

    public Timer(DateTimeOffset dueTime, TimeSpan? period, TimeProvider timeProvider, CancellationToken cancellationToken)
    {
        this.dueTime2 = dueTime;
        this.period = period;
        this.timeProvider = timeProvider;
        this.cancellationToken = cancellationToken;
    }

    protected override IDisposable SubscribeCore(Observer<Unit> observer)
    {
        var callback = (period == null) ? _Timer.singleTimerCallback : _Timer.periodicTimerCallback;
        var method = new _Timer(observer);
        method.Timer = timeProvider.CreateStoppedTimer(callback, method);

        var dueTime = (dueTime1 != null)
            ? dueTime1.Value
            : dueTime2!.Value - timeProvider.GetUtcNow();

        if (cancellationToken.CanBeCanceled)
        {
            method.cancellationTokenRegistration = cancellationToken.UnsafeRegister(static state =>
            {
                var s = (_Timer)state!;
                s.CompleteDispose();
            }, method);
        }

        if (period == null)
        {
            method.Timer.InvokeOnce(dueTime.Normalize());
        }
        else
        {
            method.Timer.Change(dueTime.Normalize(), period.Value);
        }

        return method;
    }

    sealed class _Timer(Observer<Unit> observer) : IDisposable
    {
        public static readonly TimerCallback singleTimerCallback = SingleTick;
        public static readonly TimerCallback periodicTimerCallback = PeriodicTick;

        internal CancellationTokenRegistration cancellationTokenRegistration;
        Observer<Unit> observer = observer;

        public ITimer? Timer { get; set; }

        static void SingleTick(object? state)
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

        static void PeriodicTick(object? state)
        {
            var self = (_Timer)state!;
            lock (self)
            {
                self.observer.OnNext(default);
            }
        }

        public void CompleteDispose()
        {
            observer.OnCompleted();
            Dispose();
        }

        public void Dispose()
        {
            cancellationTokenRegistration.Dispose();
            Timer?.Dispose();
            Timer = null;
        }
    }
}
