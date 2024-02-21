namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> ThrottleFirstLastFrame<T>(this Observable<T> source, int frameCount)
    {
        return new ThrottleFirstLastFrame<T>(source, frameCount, ObservableSystem.DefaultFrameProvider);
    }

    public static Observable<T> ThrottleFirstLastFrame<T>(this Observable<T> source, int frameCount, FrameProvider frameProvider)
    {
        return new ThrottleFirstLastFrame<T>(source, frameCount, frameProvider);
    }
}

internal sealed class ThrottleFirstLastFrame<T>(Observable<T> source, int frameCount, FrameProvider frameProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _ThrottleFirstLastFrame(observer, frameCount.NormalizeFrame(), frameProvider));
    }

    sealed class _ThrottleFirstLastFrame : Observer<T>, IFrameRunnerWorkItem
    {
        readonly Observer<T> observer;
        readonly FrameProvider frameProvider;
        readonly int frameCount;
        readonly object gate = new object();
        T? lastValue;
        bool hasValue;
        int currentFrame;
        bool running;

        public _ThrottleFirstLastFrame(Observer<T> observer, int frameCount, FrameProvider frameProvider)
        {
            this.observer = observer;
            this.frameCount = frameCount;
            this.frameProvider = frameProvider;
        }

        protected override void OnNextCore(T value)
        {
            lock (gate)
            {
                if (!running)
                {
                    running = true;
                    currentFrame = 0;
                    frameProvider.Register(this);
                    observer.OnNext(value);
                }
                else
                {
                    hasValue = true;
                    lastValue = value;
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
                if (++currentFrame == frameCount)
                {
                    if (hasValue)
                    {
                        observer.OnNext(lastValue!);
                        lastValue = default;
                    }
                    running = false;
                    return false;
                }
            }

            return true;
        }
    }
}
