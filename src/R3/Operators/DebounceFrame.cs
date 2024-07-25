namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> DebounceFrame<T>(this Observable<T> source, int frameCount)
    {
        return new DebounceFrame<T>(source, frameCount, ObservableSystem.DefaultFrameProvider);
    }

    public static Observable<T> DebounceFrame<T>(this Observable<T> source, int frameCount, FrameProvider frameProvider)
    {
        return new DebounceFrame<T>(source, frameCount, frameProvider);
    }
}

// DebounceFrame
internal sealed class DebounceFrame<T>(Observable<T> source, int frameCount, FrameProvider frameProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _DebounceFrame(observer, frameCount.NormalizeFrame(), frameProvider));
    }

    sealed class _DebounceFrame : Observer<T>, IFrameRunnerWorkItem
    {
        readonly Observer<T> observer;
        readonly int frameCount;
        readonly object gate = new object();
        readonly FrameProvider frameProvider;
        T? latestValue;
        bool hasvalue;
        int currentFrame;
        bool isRunning;

        public _DebounceFrame(Observer<T> observer, int frameCount, FrameProvider frameProvider)
        {
            this.observer = observer;
            this.frameCount = frameCount;
            this.frameProvider = frameProvider;
        }

        protected override void OnNextCore(T value)
        {
            lock (gate)
            {
                latestValue = value;
                hasvalue = true;
                currentFrame = 0;

                if (!isRunning)
                {
                    isRunning = true;
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
            lock (gate)
            {
                if (hasvalue)
                {
                    observer.OnNext(latestValue!);
                    hasvalue = false;
                    latestValue = default;
                }
            }
            observer.OnCompleted(result);
        }

        bool IFrameRunnerWorkItem.MoveNext(long _)
        {
            if (this.IsDisposed) return false;

            lock (gate)
            {
                if (hasvalue)
                {
                    if (++currentFrame == frameCount)
                    {
                        observer.OnNext(latestValue!);
                        hasvalue = false;
                        latestValue = default;
                        currentFrame = 0;
                        isRunning = false;
                        return false;
                    }
                }
                else
                {
                    currentFrame = 0;
                    isRunning = false;
                    return false;
                }
            }

            return true;
        }
    }
}
