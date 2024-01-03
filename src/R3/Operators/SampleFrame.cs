namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> SampleFrame<T>(this Observable<T> source, int frameCount)
    {
        return new SampleFrame<T>(source, frameCount, ObservableSystem.DefaultFrameProvider);
    }

    public static Observable<T> SampleFrame<T>(this Observable<T> source, int frameCount, FrameProvider frameProvider)
    {
        return new SampleFrame<T>(source, frameCount, frameProvider);
    }
}

internal sealed class SampleFrame<T>(Observable<T> source, int frameCount, FrameProvider frameProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _SampleFrame(observer, frameCount.NormalizeFrame(), frameProvider));
    }

    sealed class _SampleFrame : Observer<T>, IFrameRunnerWorkItem
    {
        readonly Observer<T> observer;
        readonly int frameCount;
        readonly object gate = new object();
        T? lastValue;
        bool hasValue;
        int currentFrame;

        public _SampleFrame(Observer<T> observer, int frameCount, FrameProvider frameProvider)
        {
            this.observer = observer;
            this.frameCount = frameCount;
            frameProvider.Register(this);
        }

        protected override void OnNextCore(T value)
        {
            lock (gate)
            {
                hasValue = true;
                lastValue = value;
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
                        hasValue = false;
                        lastValue = default;
                        currentFrame = 0;
                    }
                }
            }

            return true;
        }
    }
}
