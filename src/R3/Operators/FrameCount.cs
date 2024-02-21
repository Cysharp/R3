namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<(long FrameCount, T Value)> FrameCount<T>(this Observable<T> source)
    {
        return new FrameCount<T>(source, ObservableSystem.DefaultFrameProvider);
    }

    public static Observable<(long FrameCount, T Value)> FrameCount<T>(this Observable<T> source, FrameProvider frameProvider)
    {
        return new FrameCount<T>(source, frameProvider);
    }
}

internal sealed class FrameCount<T>(Observable<T> source, FrameProvider frameProvider) : Observable<(long FrameCount, T Value)>
{
    protected override IDisposable SubscribeCore(Observer<(long FrameCount, T Value)> observer)
    {
        return source.Subscribe(new _FrameCount(observer, frameProvider));
    }

    sealed class _FrameCount(Observer<(long FrameCount, T Value)> observer, FrameProvider frameProvider) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            observer.OnNext((frameProvider.GetFrameCount(), value));
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
