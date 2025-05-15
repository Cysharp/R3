namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> TimeoutFrame<T>(this Observable<T> source, int frameCount)
    {
        return new TimeoutFrame<T>(source, frameCount, ObservableSystem.DefaultFrameProvider);
    }

    public static Observable<T> TimeoutFrame<T>(this Observable<T> source, int frameCount, FrameProvider frameProvider)
    {
        return new TimeoutFrame<T>(source, frameCount, frameProvider);
    }
}

internal sealed class TimeoutFrame<T>(Observable<T> source, int frameCount, FrameProvider frameProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _TimeoutFrame(observer, frameCount.NormalizeFrame(), frameProvider));
    }

    sealed class _TimeoutFrame : Observer<T>, IFrameRunnerWorkItem
    {
        readonly Observer<T> observer;
        readonly FrameProvider frameProvider;
        readonly int periodFrame;
        readonly Lock gate = new();
        int currentFrame;
        bool running;

        public _TimeoutFrame(Observer<T> observer, int frameCount, FrameProvider frameProvider)
        {
            this.observer = observer;
            this.periodFrame = frameCount;
            this.frameProvider = frameProvider;
        }

        protected override void OnNextCore(T value)
        {
            lock (gate)
            {
                observer.OnNext(value);
                currentFrame = 0; // reset current frame
                if (!running)
                {
                    running = true;
                    frameProvider.Register(this);
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

        bool IFrameRunnerWorkItem.MoveNext(long _)
        {
            if (this.IsDisposed) return false;

            lock (gate)
            {
                if (++currentFrame == periodFrame)
                {
                    this.OnCompleted(new TimeoutException());
                    running = false;
                    return false;
                }
            }

            return true;
        }
    }
}
