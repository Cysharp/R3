namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> Skip<T>(this Observable<T> source, int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException("count");

        return new Skip<T>(source, count);
    }

    // TimeBased

    public static Observable<T> Skip<T>(this Observable<T> source, TimeSpan duration)
    {
        return Skip(source, duration, ObservableSystem.DefaultTimeProvider);
    }

    public static Observable<T> Skip<T>(this Observable<T> source, TimeSpan duration, TimeProvider timeProvider)
    {
        return new SkipTime<T>(source, duration.Normalize(), timeProvider);
    }

    // SkipFrame

    public static Observable<T> SkipFrame<T>(this Observable<T> source, int frameCount)
    {
        return SkipFrame(source, frameCount, ObservableSystem.DefaultFrameProvider);
    }

    public static Observable<T> SkipFrame<T>(this Observable<T> source, int frameCount, FrameProvider frameProvider)
    {
        return new SkipFrame<T>(source, frameCount.NormalizeFrame(), frameProvider);
    }
}

internal sealed class Skip<T>(Observable<T> source, int count) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _Skip(observer, count));
    }

    sealed class _Skip(Observer<T> observer, int count) : Observer<T>, IDisposable
    {
        int remaining = count;

        protected override void OnNextCore(T value)
        {
            if (remaining > 0)
            {
                remaining--;
            }
            else
            {
                observer.OnNext(value);
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

internal sealed class SkipTime<T>(Observable<T> source, TimeSpan duration, TimeProvider timeProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _SkipTime(observer, duration, timeProvider));
    }

    sealed class _SkipTime : Observer<T>, IDisposable
    {
        static readonly TimerCallback timerCallback = TimerStopped;

        readonly Observer<T> observer;
        ITimer? timer; // when null, the timer has been stopped

        public _SkipTime(Observer<T> observer, TimeSpan duration, TimeProvider timeProvider)
        {
            this.observer = observer;
            this.timer = timeProvider.CreateStoppedTimer(timerCallback, this);
            timer.InvokeOnce(duration);
        }

        protected override void OnNextCore(T value)
        {
            if (Volatile.Read(ref timer) != null) return;
            observer.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            observer.OnCompleted(result);
        }

        static void TimerStopped(object? state)
        {
            var self = (_SkipTime)state!;
            Volatile.Read(ref self.timer)?.Dispose();
            Volatile.Write(ref self.timer, null);

        }

        protected override void DisposeCore()
        {
            Volatile.Read(ref timer)?.Dispose();
            Volatile.Write(ref timer, null);
        }
    }
}

internal sealed class SkipFrame<T>(Observable<T> source, int frameCount, FrameProvider frameProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _SkipFrame(observer, frameCount, frameProvider));
    }

    sealed class _SkipFrame : Observer<T>, IDisposable, IFrameRunnerWorkItem
    {
        readonly Observer<T> observer;
        long remaining;

        public _SkipFrame(Observer<T> observer, int frameCount, FrameProvider frameProvider)
        {
            this.observer = observer;
            this.remaining = frameCount;
            frameProvider.Register(this);
        }

        protected override void OnNextCore(T value)
        {
            if (Volatile.Read(ref remaining) > 0) return;
            observer.OnNext(value);

        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            observer.OnCompleted(result);
        }

        bool IFrameRunnerWorkItem.MoveNext(long _)
        {
            if (this.IsDisposed) return false;

            if (remaining > 0)
            {
                remaining--;
                return true;
            }

            return false;
        }
    }
}
