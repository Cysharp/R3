namespace R3;

public static partial class EventExtensions
{
    public static Event<T> Take<T>(this Event<T> source, int count)
    {
        if (count == 0)
        {
            return Event.Empty<T>();
        }

        return new Take<T>(source, count);
    }

    // TimeBased

    public static Event<T> Take<T>(this Event<T> source, TimeSpan duration)
    {
        return Take(source, duration, EventSystem.DefaultTimeProvider);
    }

    public static Event<T> Take<T>(this Event<T> source, TimeSpan duration, TimeProvider timeProvider)
    {
        return new TakeTime<T>(source, duration, timeProvider);
    }

    // TakeFrame

    public static Event<T> TakeFrame<T>(this Event<T> source, int frameCount)
    {
        return TakeFrame(source, frameCount, EventSystem.DefaultFrameProvider);
    }

    public static Event<T> TakeFrame<T>(this Event<T> source, int frameCount, FrameProvider frameProvider)
    {
        return new TakeFrame<T>(source, frameCount, frameProvider);
    }
}

internal sealed class Take<T>(Event<T> source, int count) : Event<T>
{
    protected override IDisposable SubscribeCore(Subscriber<T> subscriber)
    {
        return source.Subscribe(new _Take(subscriber, count));
    }

    sealed class _Take(Subscriber<T> subscriber, int count) : Subscriber<T>, IDisposable
    {
        int remaining = count;

        protected override void OnNextCore(T value)
        {
            if (remaining > 0)
            {
                remaining--;
                subscriber.OnNext(value);
            }
            else
            {
                subscriber.OnCompleted();
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            subscriber.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            subscriber.OnCompleted(result);
        }
    }
}

internal sealed class TakeTime<T>(Event<T> source, TimeSpan duration, TimeProvider timeProvider) : Event<T>
{
    protected override IDisposable SubscribeCore(Subscriber<T> subscriber)
    {
        return source.Subscribe(new _TakeTime(subscriber, duration, timeProvider));
    }

    sealed class _TakeTime : Subscriber<T>, IDisposable
    {
        static readonly TimerCallback timerCallback = TimerStopped;

        readonly Subscriber<T> subscriber;
        readonly ITimer timer;
        readonly object gate = new object();

        public _TakeTime(Subscriber<T> subscriber, TimeSpan duration, TimeProvider timeProvider)
        {
            this.subscriber = subscriber;
            this.timer = timeProvider.CreateStoppedTimer(timerCallback, this);
            timer.InvokeOnce(duration);
        }

        protected override void OnNextCore(T value)
        {
            lock (gate)
            {
                subscriber.OnNext(value);
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            lock (gate)
            {
                subscriber.OnErrorResume(error);
            }
        }

        protected override void OnCompletedCore(Result result)
        {
            lock (gate)
            {
                subscriber.OnCompleted(result);
            }
        }

        static void TimerStopped(object? state)
        {
            var self = (_TakeTime)state!;
            self.OnCompleted();
        }

        protected override void DisposeCore()
        {
            timer.Dispose();
        }
    }
}

internal sealed class TakeFrame<T>(Event<T> source, int frameCount, FrameProvider frameProvider) : Event<T>
{
    protected override IDisposable SubscribeCore(Subscriber<T> subscriber)
    {
        return source.Subscribe(new _TakeFrame(subscriber, frameCount, frameProvider));
    }

    sealed class _TakeFrame : Subscriber<T>, IDisposable, IFrameRunnerWorkItem
    {
        readonly Subscriber<T> subscriber;
        long remaining;
        readonly object gate = new object();

        public _TakeFrame(Subscriber<T> subscriber, int frameCount, FrameProvider frameProvider)
        {
            this.subscriber = subscriber;
            this.remaining = frameProvider.GetFrameCount() + frameCount;
            frameProvider.Register(this);
        }

        protected override void OnNextCore(T value)
        {
            lock (gate)
            {
                subscriber.OnNext(value);
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            lock (gate)
            {
                subscriber.OnErrorResume(error);
            }
        }

        protected override void OnCompletedCore(Result result)
        {
            lock (gate)
            {
                subscriber.OnCompleted(result);
            }
        }

        bool IFrameRunnerWorkItem.MoveNext(long _)
        {
            if (this.IsDisposed) return false;

            if (remaining > 0)
            {
                remaining--;
                return true;
            }
            else
            {
                OnCompleted(Result.Success);
                return false;
            }
        }
    }
}
