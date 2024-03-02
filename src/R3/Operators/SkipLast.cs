namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> SkipLast<T>(this Observable<T> source, int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException("count");
        return new SkipLast<T>(source, count);
    }

    // TimeBased

    public static Observable<T> SkipLast<T>(this Observable<T> source, TimeSpan duration)
    {
        return SkipLast(source, duration, ObservableSystem.DefaultTimeProvider);
    }

    public static Observable<T> SkipLast<T>(this Observable<T> source, TimeSpan duration, TimeProvider timeProvider)
    {
        return new SkipLastTime<T>(source, duration.Normalize(), timeProvider);
    }

    // SkipLastFrame

    public static Observable<T> SkipLastFrame<T>(this Observable<T> source, int frameCount)
    {
        return SkipLastFrame(source, frameCount, ObservableSystem.DefaultFrameProvider);
    }

    public static Observable<T> SkipLastFrame<T>(this Observable<T> source, int frameCount, FrameProvider frameProvider)
    {
        return new SkipLastFrame<T>(source, frameCount.NormalizeFrame(), frameProvider);
    }
}

internal sealed class SkipLast<T>(Observable<T> source, int count) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _SkipLast(observer, count));
    }

    sealed class _SkipLast(Observer<T> observer, int count) : Observer<T>, IDisposable
    {
        Queue<T> queue = new Queue<T>(count);

        protected override void OnNextCore(T value)
        {
            queue.Enqueue(value);
            if (queue.Count > count)
            {
                observer.OnNext(queue.Dequeue());
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

        protected override void DisposeCore()
        {
            queue.Clear();
        }
    }
}

internal sealed class SkipLastTime<T>(Observable<T> source, TimeSpan duration, TimeProvider timeProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _SkipLastTime(observer, duration, timeProvider));
    }

    sealed class _SkipLastTime : Observer<T>, IDisposable
    {
        readonly Observer<T> observer;
        readonly Queue<(long timestamp, T value)> queue = new();
        readonly TimeSpan duration;
        readonly TimeProvider timeProvider;

        public _SkipLastTime(Observer<T> observer, TimeSpan duration, TimeProvider timeProvider)
        {
            this.observer = observer;
            this.timeProvider = timeProvider;
            this.duration = duration;
        }

        protected override void OnNextCore(T value)
        {
            var current = timeProvider.GetTimestamp();
            queue.Enqueue((current, value));
            while (queue.Count > 0 && timeProvider.GetElapsedTime(queue.Peek().timestamp, current) >= duration)
            {
                observer.OnNext(queue.Dequeue().value);
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            var current = timeProvider.GetTimestamp();
            while (queue.Count > 0 && timeProvider.GetElapsedTime(queue.Peek().timestamp, current) >= duration)
            {
                observer.OnNext(queue.Dequeue().value);
            }
            observer.OnCompleted(result);
        }

        protected override void DisposeCore()
        {
            queue.Clear();
        }
    }
}

internal sealed class SkipLastFrame<T>(Observable<T> source, int frameCount, FrameProvider frameProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _SkipLastFrame(observer, frameCount, frameProvider));
    }

    sealed class _SkipLastFrame : Observer<T>, IDisposable
    {
        readonly Observer<T> observer;
        readonly Queue<(long frameCount, T value)> queue = new();
        readonly int frameCount;
        readonly FrameProvider frameProvider;

        public _SkipLastFrame(Observer<T> observer, int frameCount, FrameProvider frameProvider)
        {
            this.observer = observer;
            this.frameCount = frameCount;
            this.frameProvider = frameProvider;
        }

        protected override void OnNextCore(T value)
        {
            var current = frameProvider.GetFrameCount();
            queue.Enqueue((current, value));
            while (queue.Count > 0 && current - queue.Peek().frameCount >= frameCount)
            {
                observer.OnNext(queue.Dequeue().value);
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            var current = frameProvider.GetFrameCount();
            while (queue.Count > 0 && current - queue.Peek().frameCount >= frameCount)
            {
                observer.OnNext(queue.Dequeue().value);
            }
            observer.OnCompleted(result);
        }

        protected override void DisposeCore()
        {
            queue.Clear();
        }
    }
}
