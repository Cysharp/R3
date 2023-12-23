namespace R3;

public static partial class Observable
{
    public static Observable<T> ReturnOnCompleted<T>(Result result)
    {
        if (result.IsSuccess)
        {
            return ImmediateScheduleReturnOnCompletedSuccess<T>.Instance; // singleton
        }
        else
        {
            return new ImmediateScheduleReturnOnCompleted<T>(result); // immediate
        }
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

internal class ImmediateScheduleReturnOnCompletedSuccess<T> : Observable<T>
{
    public static readonly Observable<T> Instance = new ImmediateScheduleReturnOnCompletedSuccess<T>();

    ImmediateScheduleReturnOnCompletedSuccess()
    {

    }

    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        observer.OnCompleted(Result.Success);
        return Disposable.Empty;
    }
}

internal class ImmediateScheduleReturnOnCompleted<T>(Result result) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        observer.OnCompleted(result);
        return Disposable.Empty;
    }
}

internal class ReturnOnCompleted<T>(Result complete, TimeSpan dueTime, TimeProvider timeProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var method = new _ReturnOnCompleted(complete, observer);
        method.Timer = timeProvider.CreateStoppedTimer(_ReturnOnCompleted.timerCallback, method);
        method.Timer.InvokeOnce(dueTime);
        return method;
    }

    sealed class _ReturnOnCompleted(Result result, Observer<T> observer) : IDisposable
    {
        public static readonly TimerCallback timerCallback = NextTick;

        readonly Result result = result;
        readonly Observer<T> observer = observer;

        public ITimer? Timer { get; set; }

        static void NextTick(object? state)
        {
            var self = (_ReturnOnCompleted)state!;
            try
            {
                self.observer.OnCompleted(self.result);
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
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var method = new _ReturnOnCompleted(result, observer);
        ThreadPool.UnsafeQueueUserWorkItem(method, preferLocal: false);
        return method;
    }

    sealed class _ReturnOnCompleted(Result result, Observer<T> observer) : IDisposable, IThreadPoolWorkItem
    {
        bool stop;

        public void Execute()
        {
            if (stop) return;

            observer.OnCompleted(result);
        }

        public void Dispose()
        {
            stop = true;
        }
    }
}
