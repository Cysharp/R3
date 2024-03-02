namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> Take<T>(this Observable<T> source, int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException("count");

        if (count == 0)
        {
            return Observable.Empty<T>();
        }

        return new Take<T>(source, count);
    }

    // TimeBased

    public static Observable<T> Take<T>(this Observable<T> source, TimeSpan duration)
    {
        return Take(source, duration, ObservableSystem.DefaultTimeProvider);
    }

    public static Observable<T> Take<T>(this Observable<T> source, TimeSpan duration, TimeProvider timeProvider)
    {
        return new TakeTime<T>(source, duration.Normalize(), timeProvider);
    }

    // TakeFrame

    public static Observable<T> TakeFrame<T>(this Observable<T> source, int frameCount)
    {
        return TakeFrame(source, frameCount, ObservableSystem.DefaultFrameProvider);
    }

    public static Observable<T> TakeFrame<T>(this Observable<T> source, int frameCount, FrameProvider frameProvider)
    {
        return new TakeFrame<T>(source, frameCount.NormalizeFrame(), frameProvider);
    }
}

internal sealed class Take<T>(Observable<T> source, int count) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _Take(observer, count));
    }

    sealed class _Take(Observer<T> observer, int count) : Observer<T>, IDisposable
    {
        int remaining = count;

        protected override void OnNextCore(T value)
        {
            if (remaining > 0)
            {
                remaining--;
                observer.OnNext(value);
                if (remaining == 0)
                {
                    observer.OnCompleted();
                }
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            observer.OnCompleted(result);
        }
    }
}

internal sealed class TakeTime<T>(Observable<T> source, TimeSpan duration, TimeProvider timeProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _TakeTime(observer, duration, timeProvider));
    }

    sealed class _TakeTime : Observer<T>, IDisposable
    {
        static readonly TimerCallback timerCallback = TimerStopped;

        readonly Observer<T> observer;
        readonly ITimer timer;
        readonly object gate = new object();

        public _TakeTime(Observer<T> observer, TimeSpan duration, TimeProvider timeProvider)
        {
            this.observer = observer;
            this.timer = timeProvider.CreateStoppedTimer(timerCallback, this);
            timer.InvokeOnce(duration);
        }

        protected override void OnNextCore(T value)
        {
            lock (gate)
            {
                observer.OnNext(value);
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            lock (gate)
            {
                observer.OnErrorResume(error);
            }
        }

        protected override void OnCompletedCore(Result result)
        {
            lock (gate)
            {
                observer.OnCompleted(result);
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
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _TakeFrame(observer, frameCount, frameProvider));
    }

    sealed class _TakeFrame : Observer<T>, IDisposable, IFrameRunnerWorkItem
    {
        readonly Observer<T> observer;
        long remaining;
        readonly object gate = new object();

        public _TakeFrame(Observer<T> observer, int frameCount, FrameProvider frameProvider)
        {
            this.observer = observer;
            this.remaining = frameProvider.GetFrameCount() + frameCount;
            frameProvider.Register(this);
        }

        protected override void OnNextCore(T value)
        {
            lock (gate)
            {
                observer.OnNext(value);
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            lock (gate)
            {
                observer.OnErrorResume(error);
            }
        }

        protected override void OnCompletedCore(Result result)
        {
            lock (gate)
            {
                observer.OnCompleted(result);
            }
        }

        bool IFrameRunnerWorkItem.MoveNext(long _)
        {
            if (this.IsDisposed) return false;

            if (remaining > 0)
            {
                remaining--;
                if (remaining == 0)
                {
                    OnCompleted(Result.Success);
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }
    }
}
