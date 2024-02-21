namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<(long Interval, T Value)> FrameInterval<T>(this Observable<T> source)
    {
        return new FrameInterval<T>(source, ObservableSystem.DefaultFrameProvider);
    }

    public static Observable<(long Interval, T Value)> FrameInterval<T>(this Observable<T> source, FrameProvider frameProvider)
    {
        return new FrameInterval<T>(source, frameProvider);
    }
}

internal sealed class FrameInterval<T>(Observable<T> source, FrameProvider frameProvider) : Observable<(long Interval, T Value)>
{
    protected override IDisposable SubscribeCore(Observer<(long Interval, T Value)> observer)
    {
        return source.Subscribe(new _FrameInterval(observer, frameProvider));
    }

    sealed class _FrameInterval(Observer<(long Interval, T Value)> observer, FrameProvider frameProvider) : Observer<T>
    {
        long previousFrameCount = frameProvider.GetFrameCount();

        protected override void OnNextCore(T value)
        {
            var currentFrameCount = frameProvider.GetFrameCount();
            var elapsed = currentFrameCount - previousFrameCount;
            this.previousFrameCount = currentFrameCount;

            observer.OnNext((elapsed, value));
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
