namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> IgnoreOnErrorResume<T>(this Observable<T> source)
    {
        return new IgnoreOnErrorResume<T>(source, null);
    }

    public static Observable<T> IgnoreOnErrorResume<T>(this Observable<T> source, Action<Exception>? doOnErrorResume)
    {
        return new IgnoreOnErrorResume<T>(source, doOnErrorResume);
    }
}

internal sealed class IgnoreOnErrorResume<T>(Observable<T> source, Action<Exception>? doOnErrorResume) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _IgnoreOnErrorResume(observer, doOnErrorResume));
    }

    sealed class _IgnoreOnErrorResume(Observer<T> observer, Action<Exception>? doOnErrorResume) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            observer.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            doOnErrorResume?.Invoke(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            observer.OnCompleted(result);
        }
    }
}
