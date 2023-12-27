namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> IgnoreElements<T>(this Observable<T> source)
    {
        return new IgnoreElements<T>(source);
    }
}

internal sealed class IgnoreElements<T>(Observable<T> source) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _IgnoreElements(observer));
    }

    sealed class _IgnoreElements(Observer<T> observer) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
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
