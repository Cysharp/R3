namespace R3;

public static partial class EventExtensions
{
    public static Event<T> TakeLast<T>(this Event<T> source, int count)
    {
        return new TakeLast<T>(source, count);
    }

    // TimeBased

    public static Event<T> TakeLast<T>(this Event<T> source, TimeSpan duration)
    {
        return TakeLast(source, duration, EventSystem.DefaultTimeProvider);
    }

    public static Event<T> TakeLast<T>(this Event<T> source, TimeSpan duration, TimeProvider timeProvider)
    {
        return new TakeLastTime<T>(source, duration, timeProvider);
    }

    // TakeLastFrame

    public static Event<T> TakeLastFrame<T>(this Event<T> source, int frameCount)
    {
        return TakeLastFrame(source, frameCount, EventSystem.DefaultFrameProvider);
    }

    public static Event<T> TakeLastFrame<T>(this Event<T> source, int frameCount, FrameProvider frameProvider)
    {
        return new TakeLastFrame<T>(source, frameCount, frameProvider);
    }
}

internal sealed class TakeLast<T>(Event<T> source, int count) : Event<T>
{
    protected override IDisposable SubscribeCore(Subscriber<T> subscriber)
    {
        return source.Subscribe(new _TakeLast(subscriber, count));
    }

    sealed class _TakeLast(Subscriber<T> subscriber, int count) : Subscriber<T>, IDisposable
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
            subscriber.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            if (result.IsFailure)
            {
                subscriber.OnCompleted(result);
                return;
            }

            foreach (var item in queue)
            {
                subscriber.OnNext(item);
            }
            subscriber.OnCompleted();
        }

        protected override void DisposeCore()
        {
            queue.Clear();
        }
    }
}

internal sealed class TakeLastTime<T>(Event<T> source, TimeSpan duration, TimeProvider timeProvider) : Event<T>
{
    protected override IDisposable SubscribeCore(Subscriber<T> subscriber)
    {
        return source.Subscribe(new _TakeLastTime(subscriber, duration, timeProvider));
    }

    sealed class _TakeLastTime : Subscriber<T>, IDisposable
    {
        readonly Subscriber<T> subscriber;
        readonly object gate = new object();
        readonly Queue<(long timestamp, T value)> queue = new();
        readonly TimeSpan duration;
        readonly TimeProvider timeProvider;

        public _TakeLastTime(Subscriber<T> subscriber, TimeSpan duration, TimeProvider timeProvider)
        {
            this.subscriber = subscriber;
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
                subscriber.OnErrorResume(error);
            }
        }

        protected override void OnCompletedCore(Result result)
        {
            lock (gate)
            {
                if (result.IsFailure)
                {
                    subscriber.OnCompleted(result);
                    return;
                }

                Trim(timeProvider.GetTimestamp());
                foreach (var item in queue)
                {
                    subscriber.OnNext(item.value);
                }
                subscriber.OnCompleted();
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

internal sealed class TakeLastFrame<T>(Event<T> source, int frameCount, FrameProvider frameProvider) : Event<T>
{
    protected override IDisposable SubscribeCore(Subscriber<T> subscriber)
    {
        return source.Subscribe(new _TakeLastFrame(subscriber, frameCount, frameProvider));
    }

    sealed class _TakeLastFrame : Subscriber<T>, IDisposable
    {
        readonly Subscriber<T> subscriber;
        readonly object gate = new object();
        readonly Queue<(long frameCount, T value)> queue = new();
        readonly int frameCount;
        readonly FrameProvider frameProvider;

        public _TakeLastFrame(Subscriber<T> subscriber, int frameCount, FrameProvider frameProvider)
        {
            this.subscriber = subscriber;
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
                subscriber.OnErrorResume(error);
            }
        }

        protected override void OnCompletedCore(Result result)
        {
            lock (gate)
            {
                if (result.IsFailure)
                {
                    subscriber.OnCompleted(result);
                    return;
                }

                Trim(frameProvider.GetFrameCount());
                foreach (var item in queue)
                {
                    subscriber.OnNext(item.value);
                }
                subscriber.OnCompleted();
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
