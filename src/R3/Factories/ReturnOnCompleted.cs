namespace R3;

public static partial class Observable
{
    public static Observable<T> ReturnOnCompleted<T>(Result result)
    {
        return new ImmediateScheduleReturnOnCompleted<T>(result); // immediate
    }

    public static Observable<T> ReturnOnCompleted<T>(Result result, TimeProvider timeProvider)
    {
        return ReturnOnCompleted<T>(result, TimeSpan.Zero, timeProvider);
    }

    public static Observable<T> ReturnOnCompleted<T>(Result result, TimeSpan dueTime, TimeProvider timeProvider)
    {
        if (dueTime == TimeSpan.Zero)
        {
            if (timeProvider == TimeProvider.System)
            {
                return new ThreadPoolScheduleReturnOnCompleted<T>(result); // optimize for SystemTimeProvidr, use ThreadPool.UnsafeQueueUserWorkItem
            }
        }

        return new ReturnOnCompleted<T>(result, dueTime, timeProvider); // use ITimer
    }
}

internal class ImmediateScheduleReturnOnCompleted<T>(Result result) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> subscriber)
    {
        subscriber.OnCompleted(result);
        return Disposable.Empty;
    }
}

internal class ReturnOnCompleted<T>(Result complete, TimeSpan dueTime, TimeProvider timeProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> subscriber)
    {
        var method = new _ReturnOnCompleted(complete, subscriber);
        method.Timer = timeProvider.CreateStoppedTimer(_ReturnOnCompleted.timerCallback, method);
        method.Timer.InvokeOnce(dueTime);
        return method;
    }

    sealed class _ReturnOnCompleted(Result result, Observer<T> subscriber) : IDisposable
    {
        public static readonly TimerCallback timerCallback = NextTick;

        readonly Result result = result;
        readonly Observer<T> subscriber = subscriber;

        public ITimer? Timer { get; set; }

        static void NextTick(object? state)
        {
            var self = (_ReturnOnCompleted)state!;
            try
            {
                self.subscriber.OnCompleted(self.result);
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

internal class ThreadPoolScheduleReturnOnCompleted<T>(Result result) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> subscriber)
    {
        var method = new _ReturnOnCompleted(result, subscriber);
        ThreadPool.UnsafeQueueUserWorkItem(method, preferLocal: false);
        return method;
    }

    sealed class _ReturnOnCompleted(Result result, Observer<T> subscriber) : IDisposable, IThreadPoolWorkItem
    {
        bool stop;

        public void Execute()
        {
            if (stop) return;

            subscriber.OnCompleted(result);
        }

        public void Dispose()
        {
            stop = true;
        }
    }
}
