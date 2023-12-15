﻿namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> TakeLast<T>(this Observable<T> source, int count)
    {
        return new TakeLast<T>(source, count);
    }

    // TimeBased

    public static Observable<T> TakeLast<T>(this Observable<T> source, TimeSpan duration)
    {
        return TakeLast(source, duration, ObservableSystem.DefaultTimeProvider);
    }

    public static Observable<T> TakeLast<T>(this Observable<T> source, TimeSpan duration, TimeProvider timeProvider)
    {
        return new TakeLastTime<T>(source, duration, timeProvider);
    }

    // TakeLastFrame

    public static Observable<T> TakeLastFrame<T>(this Observable<T> source, int frameCount)
    {
        return TakeLastFrame(source, frameCount, ObservableSystem.DefaultFrameProvider);
    }

    public static Observable<T> TakeLastFrame<T>(this Observable<T> source, int frameCount, FrameProvider frameProvider)
    {
        return new TakeLastFrame<T>(source, frameCount, frameProvider);
    }
}

internal sealed class TakeLast<T>(Observable<T> source, int count) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _TakeLast(observer, count));
    }

    sealed class _TakeLast(Observer<T> observer, int count) : Observer<T>, IDisposable
    {
        Queue<T> queue = new Queue<T>(count);

        protected override void OnNextCore(T value)
        {
            if (queue.Count == count && queue.Count != 0)
            {
                queue.Dequeue();
            }
            queue.Enqueue(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            if (result.IsFailure)
            {
                observer.OnCompleted(result);
                return;
            }

            foreach (var item in queue)
            {
                observer.OnNext(item);
            }
            observer.OnCompleted();
        }

        protected override void DisposeCore()
        {
            queue.Clear();
        }
    }
}

internal sealed class TakeLastTime<T>(Observable<T> source, TimeSpan duration, TimeProvider timeProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _TakeLastTime(observer, duration, timeProvider));
    }

    sealed class _TakeLastTime : Observer<T>, IDisposable
    {
        readonly Observer<T> observer;
        readonly object gate = new object();
        readonly Queue<(long timestamp, T value)> queue = new();
        readonly TimeSpan duration;
        readonly TimeProvider timeProvider;

        public _TakeLastTime(Observer<T> observer, TimeSpan duration, TimeProvider timeProvider)
        {
            this.observer = observer;
            this.timeProvider = timeProvider;
            this.duration = duration;
        }

        protected override void OnNextCore(T value)
        {
            lock (gate)
            {
                var current = timeProvider.GetTimestamp();
                queue.Enqueue((current, value));
                Trim(current);
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
                if (result.IsFailure)
                {
                    observer.OnCompleted(result);
                    return;
                }

                Trim(timeProvider.GetTimestamp());
                foreach (var item in queue)
                {
                    observer.OnNext(item.value);
                }
                observer.OnCompleted();
            }
        }

        protected override void DisposeCore()
        {
            lock (gate)
            {
                queue.Clear();
            }
        }

        void Trim(long currentTimestamp)
        {
            while (queue.Count > 0 && timeProvider.GetElapsedTime(queue.Peek().timestamp, currentTimestamp) > duration)
            {
                queue.Dequeue();
            }
        }
    }
}

internal sealed class TakeLastFrame<T>(Observable<T> source, int frameCount, FrameProvider frameProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _TakeLastFrame(observer, frameCount, frameProvider));
    }

    sealed class _TakeLastFrame : Observer<T>, IDisposable
    {
        readonly Observer<T> observer;
        readonly object gate = new object();
        readonly Queue<(long frameCount, T value)> queue = new();
        readonly int frameCount;
        readonly FrameProvider frameProvider;

        public _TakeLastFrame(Observer<T> observer, int frameCount, FrameProvider frameProvider)
        {
            this.observer = observer;
            this.frameCount = frameCount;
            this.frameProvider = frameProvider;
        }

        protected override void OnNextCore(T value)
        {
            lock (gate)
            {
                var current = frameProvider.GetFrameCount();
                queue.Enqueue((current, value));
                Trim(current);
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
                if (result.IsFailure)
                {
                    observer.OnCompleted(result);
                    return;
                }

                Trim(frameProvider.GetFrameCount());
                foreach (var item in queue)
                {
                    observer.OnNext(item.value);
                }
                observer.OnCompleted();
            }
        }

        void Trim(long currentFrameCount)
        {
            while (queue.Count > 0 && currentFrameCount - queue.Peek().frameCount > frameCount)
            {
                queue.Dequeue();
            }
        }
    }
}
