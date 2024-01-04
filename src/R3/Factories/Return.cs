namespace R3;

public static partial class Observable
{
    public static Observable<T> Return<T>(T value)
    {
        return new ImmediateScheduleReturn<T>(value); // immediate
    }

    public static Observable<T> Return<T>(T value, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        return Return(value, TimeSpan.Zero, timeProvider, cancellationToken);
    }

    public static Observable<T> Return<T>(T value, TimeSpan dueTime, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        if (dueTime == TimeSpan.Zero)
        {
            if (timeProvider == TimeProvider.System)
            {
                return new ThreadPoolScheduleReturn<T>(value, cancellationToken); // optimize for SystemTimeProvidr, use ThreadPool.UnsafeQueueUserWorkItem
            }
        }

        return new Return<T>(value, dueTime.Normalize(), timeProvider, cancellationToken); // use ITimer
    }

    // Optimized case

    public static Observable<Unit> ReturnUnit()
    {
        return R3.ReturnUnit.Instance; // singleton
    }

    public static Observable<Unit> Return(Unit value)
    {
        return R3.ReturnUnit.Instance;
    }

    public static Observable<bool> Return(bool value)
    {
        return value ? ReturnBoolean.True : ReturnBoolean.False; // singleton
    }

    public static Observable<int> Return(int value)
    {
        return ReturnInt32.GetObservable(value); // -1~9 singleton
    }

    // util

    public static Observable<Unit> Yield(CancellationToken cancellationToken = default)
    {
        return new ThreadPoolScheduleReturn<Unit>(default, cancellationToken);
    }

    public static Observable<Unit> Yield(TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        if (timeProvider == TimeProvider.System)
        {
            return new ThreadPoolScheduleReturn<Unit>(default, cancellationToken);
        }
        return new Return<Unit>(default, TimeSpan.Zero, timeProvider, cancellationToken);
    }
}

internal sealed class Return<T>(T value, TimeSpan dueTime, TimeProvider timeProvider, CancellationToken cancellationToken) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var method = new _Return(value, observer);
        method.Timer = timeProvider.CreateStoppedTimer(_Return.timerCallback, method);

        if (cancellationToken.CanBeCanceled)
        {
            method.cancellationTokenRegistration = cancellationToken.UnsafeRegister(static state =>
            {
                var s = (_Return)state!;
                s.CompleteDispose();
            }, method);
        }

        method.Timer.InvokeOnce(dueTime);
        return method;
    }

    sealed class _Return(T value, Observer<T> observer) : IDisposable
    {
        public static readonly TimerCallback timerCallback = NextTick;

        internal CancellationTokenRegistration cancellationTokenRegistration;

        readonly T value = value;
        readonly Observer<T> observer = observer;

        public ITimer? Timer { get; set; }

        static void NextTick(object? state)
        {
            var self = (_Return)state!;
            self.observer.OnNext(self.value);
            self.observer.OnCompleted();
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

internal sealed class ImmediateScheduleReturn<T>(T value) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        observer.OnNext(value);
        observer.OnCompleted();
        return Disposable.Empty;
    }
}

internal sealed class ThreadPoolScheduleReturn<T>(T value, CancellationToken cancellationToken) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var method = new _Return(value, observer);

        if (cancellationToken.CanBeCanceled)
        {
            method.cancellationTokenRegistration = cancellationToken.UnsafeRegister(static state =>
            {
                var s = (_Return)state!;
                s.CompleteDispose();
            }, method);
        }

        ThreadPool.UnsafeQueueUserWorkItem(method, preferLocal: false);
        return method;
    }

    sealed class _Return(T value, Observer<T> observer) : IDisposable, IThreadPoolWorkItem
    {
        bool stop;

        internal CancellationTokenRegistration cancellationTokenRegistration;

        public void Execute()
        {
            if (stop) return;

            observer.OnNext(value);
            observer.OnCompleted();
        }

        public void CompleteDispose()
        {
            observer.OnCompleted();
            Dispose();
        }

        public void Dispose()
        {
            cancellationTokenRegistration.Dispose();
            stop = true;
        }
    }
}

// Optimized case

internal sealed class ReturnUnit : Observable<Unit>
{
    internal static readonly Observable<Unit> Instance = new ReturnUnit();

    ReturnUnit()
    {
    }

    protected override IDisposable SubscribeCore(Observer<Unit> observer)
    {
        observer.OnNext(default);
        observer.OnCompleted();
        return Disposable.Empty;
    }
}

internal sealed class ReturnBoolean : Observable<bool>
{
    internal static readonly Observable<bool> True = new ReturnBoolean(true);
    internal static readonly Observable<bool> False = new ReturnBoolean(false);

    bool value;

    ReturnBoolean(bool value)
    {
        this.value = value;
    }

    protected override IDisposable SubscribeCore(Observer<bool> observer)
    {
        observer.OnNext(value);
        observer.OnCompleted();
        return Disposable.Empty;
    }
}

internal sealed class ReturnInt32 : Observable<int>
{
    internal static readonly Observable<int> _m1 = new ReturnInt32(-1);
    internal static readonly Observable<int> _0 = new ReturnInt32(0);
    internal static readonly Observable<int> _1 = new ReturnInt32(1);
    internal static readonly Observable<int> _2 = new ReturnInt32(2);
    internal static readonly Observable<int> _3 = new ReturnInt32(3);
    internal static readonly Observable<int> _4 = new ReturnInt32(4);
    internal static readonly Observable<int> _5 = new ReturnInt32(5);
    internal static readonly Observable<int> _6 = new ReturnInt32(6);
    internal static readonly Observable<int> _7 = new ReturnInt32(7);
    internal static readonly Observable<int> _8 = new ReturnInt32(8);
    internal static readonly Observable<int> _9 = new ReturnInt32(9);

    public static Observable<int> GetObservable(int value)
    {
        switch (value)
        {
            case -1: return _m1;
            case 0: return _0;
            case 1: return _1;
            case 2: return _2;
            case 3: return _3;
            case 4: return _4;
            case 5: return _5;
            case 6: return _6;
            case 7: return _7;
            case 8: return _8;
            case 9: return _9;
            default: return new ReturnInt32(value);
        }
    }

    int value;

    ReturnInt32(int value)
    {
        this.value = value;
    }

    protected override IDisposable SubscribeCore(Observer<int> observer)
    {
        observer.OnNext(value);
        observer.OnCompleted();
        return Disposable.Empty;
    }
}
