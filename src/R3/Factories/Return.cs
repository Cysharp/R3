namespace R3;

public static partial class Observable
{
    public static Observable<T> Return<T>(T value)
    {
        return new ImmediateScheduleReturn<T>(value); // immediate
    }

    public static Observable<T> Return<T>(T value, TimeProvider timeProvider)
    {
        return Return(value, TimeSpan.Zero, timeProvider);
    }

    public static Observable<T> Return<T>(T value, TimeSpan dueTime, TimeProvider timeProvider)
    {
        if (dueTime == TimeSpan.Zero)
        {
            if (timeProvider == TimeProvider.System)
            {
                return new ThreadPoolScheduleReturn<T>(value); // optimize for SystemTimeProvidr, use ThreadPool.UnsafeQueueUserWorkItem
            }
        }

        return new Return<T>(value, dueTime, timeProvider); // use ITimer
    }
}

internal class Return<T>(T value, TimeSpan dueTime, TimeProvider timeProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var method = new _Return(value, observer);
        method.Timer = timeProvider.CreateStoppedTimer(_Return.timerCallback, method);
        method.Timer.InvokeOnce(dueTime);
        return method;
    }

    sealed class _Return(T value, Observer<T> observer) : IDisposable
    {
        public static readonly TimerCallback timerCallback = NextTick;

        readonly T value = value;
        readonly Observer<T> observer = observer;

        public ITimer? Timer { get; set; }

        static void NextTick(object? state)
        {
            var self = (_Return)state!;
            self.observer.OnNext(self.value);
            self.observer.OnCompleted();
        }

        public void Dispose()
        {
            Timer?.Dispose();
            Timer = null;
        }
    }
}

internal class ImmediateScheduleReturn<T>(T value) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        observer.OnNext(value);
        observer.OnCompleted();
        return Disposable.Empty;
    }
}

internal class ThreadPoolScheduleReturn<T>(T value) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var method = new _Return(value, observer);
        ThreadPool.UnsafeQueueUserWorkItem(method, preferLocal: false);
        return method;
    }

    sealed class _Return(T value, Observer<T> observer) : IDisposable, IThreadPoolWorkItem
    {
        bool stop;

        public void Execute()
        {
            if (stop) return;

            observer.OnNext(value);
            observer.OnCompleted();
        }

        public void Dispose()
        {
            stop = true;
        }
    }
}
