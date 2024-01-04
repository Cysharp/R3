namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> IgnoreElements<T>(this Observable<T> source)
    {
        return new IgnoreElements<T>(source, null);
    }

    public static Observable<T> IgnoreElements<T>(this Observable<T> source, Action<T> doOnNext)
    {
        return new IgnoreElements<T>(source, doOnNext);
    }
}

internal sealed class IgnoreElements<T>(Observable<T> source, Action<T>? doOnNext) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _IgnoreElements(observer, doOnNext));
    }

    sealed class _IgnoreElements(Observer<T> observer, Action<T>? doOnNext) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            doOnNext?.Invoke(value);
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
