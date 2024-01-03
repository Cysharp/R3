namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> DelaySubscriptionFrame<T>(this Observable<T> source, int frameCount)
    {
        return new DelaySubscriptionFrame<T>(source, frameCount, ObservableSystem.DefaultFrameProvider);
    }

    public static Observable<T> DelaySubscriptionFrame<T>(this Observable<T> source, int frameCount, FrameProvider frameProvider)
    {
        return new DelaySubscriptionFrame<T>(source, frameCount, frameProvider);
    }
}

internal sealed class DelaySubscriptionFrame<T>(Observable<T> source, int frameCount, FrameProvider frameProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return new _DelaySubscription(observer, source, frameCount.NormalizeFrame(), frameProvider).Run();
    }

    sealed class _DelaySubscription : Observer<T>, IFrameRunnerWorkItem
    {
        readonly Observer<T> observer;
        readonly Observable<T> source;
        readonly int frameCount;
        readonly FrameProvider frameProvider;
        int currentFrame;

        public _DelaySubscription(Observer<T> observer, Observable<T> source, int frameCount, FrameProvider frameProvider)
        {
            this.observer = observer;
            this.source = source;
            this.frameCount = frameCount;
            this.frameProvider = frameProvider;
        }

        public IDisposable Run()
        {
            frameProvider.Register(this);
            return this;
        }

        protected override void OnNextCore(T value)
        {
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
            if (IsDisposed) return false;

            if (++currentFrame == frameCount)
            {
                try
                {
                    source.Subscribe(this); // subscribe self.
                }
                catch (Exception ex)
                {
                    ObservableSystem.GetUnhandledExceptionHandler().Invoke(ex);
                    Dispose();
                }
                return false;
            }

            return true;
        }
    }
}
