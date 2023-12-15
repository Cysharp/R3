namespace R3;

public static partial class EventExtensions
{
    public static Observable<T> Take<T>(this Observable<T> source, int count)
    {
        if (count == 0)
        {
            return Observable.Empty<T>();
        }

        return new Take<T>(source, count);
    }

    // TimeBased

    public static Observable<T> Take<T>(this Observable<T> source, TimeSpan duration)
    {
        return Take(source, duration, EventSystem.DefaultTimeProvider);
    }

    public static Observable<T> Take<T>(this Observable<T> source, TimeSpan duration, TimeProvider timeProvider)
    {
        return new TakeTime<T>(source, duration, timeProvider);
    }

    // TakeFrame

    public static Observable<T> TakeFrame<T>(this Observable<T> source, int frameCount)
    {
        return TakeFrame(source, frameCount, EventSystem.DefaultFrameProvider);
    }

    public static Observable<T> TakeFrame<T>(this Observable<T> source, int frameCount, FrameProvider frameProvider)
    {
        return new TakeFrame<T>(source, frameCount, frameProvider);
    }
}

internal sealed class Take<T>(Observable<T> source, int count) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> subscriber)
    {
        return source.Subscribe(new _Take(subscriber, count));
    }

    sealed class _Take(Observer<T> subscriber, int count) : Observer<T>, IDisposable
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

internal sealed class TakeTime<T>(Observable<T> source, TimeSpan duration, TimeProvider timeProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> subscriber)
    {
        return source.Subscribe(new _TakeTime(subscriber, duration, timeProvider));
    }

    sealed class _TakeTime : Observer<T>, IDisposable
    {
        static readonly TimerCallback timerCallback = TimerStopped;

        readonly Observer<T> subscriber;
        readonly ITimer timer;
        readonly object gate = new object();

        public _TakeTime(Observer<T> subscriber, TimeSpan duration, TimeProvider timeProvider)
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

internal sealed class TakeFrame<T>(Observable<T> source, int frameCount, FrameProvider frameProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> subscriber)
    {
        return source.Subscribe(new _TakeFrame(subscriber, frameCount, frameProvider));
    }

    sealed class _TakeFrame : Observer<T>, IDisposable, IFrameRunnerWorkItem
    {
        readonly Observer<T> subscriber;
        long remaining;
        readonly object gate = new object();

        public _TakeFrame(Observer<T> subscriber, int frameCount, FrameProvider frameProvider)
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
