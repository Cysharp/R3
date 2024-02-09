namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> DelayFrame<T>(this Observable<T> source, int frameCount)
    {
        return new DelayFrame<T>(source, frameCount, ObservableSystem.DefaultFrameProvider);
    }

    public static Observable<T> DelayFrame<T>(this Observable<T> source, int frameCount, FrameProvider frameProvider)
    {
        return new DelayFrame<T>(source, frameCount, frameProvider);
    }
}

internal sealed class DelayFrame<T>(Observable<T> source, int frameCount, FrameProvider frameProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _DelayFrame(observer, frameCount.NormalizeFrame(), frameProvider));
    }

    sealed class _DelayFrame : Observer<T>, IFrameRunnerWorkItem
    {
        readonly Observer<T> observer;
        readonly int frameCount;
        readonly FrameProvider frameProvider;
        readonly Queue<(long timestamp, Notification<T> value)> queue = new(); // lock gate
        bool running;
        long currentFrame;

        protected override bool AutoDisposeOnCompleted => false;

        public _DelayFrame(Observer<T> observer, int frameCount, FrameProvider frameProvider)
        {
            this.frameCount = frameCount;
            this.observer = observer;
            this.frameProvider = frameProvider;
        }

        protected override void OnNextCore(T value)
        {
            lock (queue)
            {
                queue.Enqueue((frameProvider.GetFrameCount(), new(value)));
                if (queue.Count == 1 && !running)
                {
                    running = true;
                    currentFrame = frameProvider.GetFrameCount();
                    frameProvider.Register(this);
                }
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            lock (queue)
            {
                queue.Enqueue((frameProvider.GetFrameCount(), new(error)));
                if (queue.Count == 1 && !running)
                {
                    running = true;
                    currentFrame = frameProvider.GetFrameCount();
                    frameProvider.Register(this);
                }
            }
        }

        protected override void OnCompletedCore(Result result)
        {
            lock (queue)
            {
                queue.Enqueue((frameProvider.GetFrameCount(), new(result)));
                if (queue.Count == 1 && !running)
                {
                    running = true;
                    currentFrame = frameProvider.GetFrameCount();
                    frameProvider.Register(this);
                }
            }
        }

        protected override void DisposeCore()
        {
            lock (queue)
            {
                queue.Clear();
            }
        }

        bool IFrameRunnerWorkItem.MoveNext(long _)
        {
            currentFrame++; // incr frame manually in local queue

            Notification<T> value;
            while (true)
            {
                if (IsDisposed) return false;

                lock (queue)
                {
                    if (!queue.TryPeek(out var msg))
                    {
                        running = false;
                        return false;
                    }

                    // check timestamp
                    var elapsed = currentFrame - msg.timestamp;
                    if (elapsed >= frameCount)
                    {
                        value = queue.Dequeue().value;
                    }
                    else
                    {
                        // continue next frame
                        return true;
                    }
                }

                try
                {
                    switch (value.Kind)
                    {
                        case NotificationKind.OnNext:
                            observer.OnNext(value.Value!);
                            break;
                        case NotificationKind.OnErrorResume:
                            observer.OnErrorResume(value.Error!);
                            break;
                        case NotificationKind.OnCompleted:
                            try
                            {
                                observer.OnCompleted(value.Result!);
                            }
                            finally
                            {
                                Dispose();
                            }
                            break;
                        default:
                            break;
                    }

                    continue; // loop to drain all messages
                }
                catch (Exception ex)
                {
                    ObservableSystem.GetUnhandledExceptionHandler().Invoke(ex);
                    continue;
                }
            }
        }
    }
}
