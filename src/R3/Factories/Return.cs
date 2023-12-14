namespace R3;

public static partial class Event
{
    public static Event<T> Return<T>(T value)
    {
        return new ImmediateScheduleReturn<T>(value); // immediate
    }

    public static Event<T> Return<T>(T value, TimeProvider timeProvider)
    {
        return Return(value, TimeSpan.Zero, timeProvider);
    }

    public static Event<T> Return<T>(T value, TimeSpan dueTime, TimeProvider timeProvider)
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

internal class Return<T>(T value, TimeSpan dueTime, TimeProvider timeProvider) : Event<T>
{
    protected override IDisposable SubscribeCore(Subscriber<T> subscriber)
    {
        var method = new _Return(value, subscriber);
        method.Timer = timeProvider.CreateStoppedTimer(_Return.timerCallback, method);
        method.Timer.InvokeOnce(dueTime);
        return method;
    }

    sealed class _Return(T value, Subscriber<T> subscriber) : IDisposable
    {
        public static readonly TimerCallback timerCallback = NextTick;

        readonly T value = value;
        readonly Subscriber<T> subscriber = subscriber;

        public ITimer? Timer { get; set; }

        static void NextTick(object? state)
        {
            var self = (_Return)state!;
            if (self.subscriber.OnNext(self.value))
            {
                self.subscriber.OnCompleted();
            }
            else
            {
                self.subscriber.OnCompleted(Result.Failure);
            }
        }

        public void Dispose()
        {
            Timer?.Dispose();
            Timer = null;
        }
    }
}

internal class ImmediateScheduleReturn<T>(T value) : Event<T>
{
    protected override IDisposable SubscribeCore(Subscriber<T> subscriber)
    {
        subscriber.OnNext(value);
        subscriber.OnCompleted();
        return Disposable.Empty;
    }
}

internal class ThreadPoolScheduleReturn<T>(T value) : Event<T>
{
    protected override IDisposable SubscribeCore(Subscriber<T> subscriber)
    {
        var method = new _Return(value, subscriber);
        ThreadPool.UnsafeQueueUserWorkItem(method, preferLocal: false);
        return method;
    }

    sealed class _Return(T value, Subscriber<T> subscriber) : IDisposable, IThreadPoolWorkItem
    {
        bool stop;

        public void Execute()
        {
            if (stop) return;

            subscriber.OnNext(value);
            subscriber.OnCompleted();
        }

        public void Dispose()
        {
            stop = true;
        }
    }
}
