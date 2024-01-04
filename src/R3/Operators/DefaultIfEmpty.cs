namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T?> DefaultIfEmpty<T>(this Observable<T> source)
    {
        return DefaultIfEmpty(source, default!);
    }

    public static Observable<T?> DefaultIfEmpty<T>(this Observable<T> source, T? defaultValue)
    {
        return new DefaultIfEmpty<T>(source, defaultValue);
    }
}

internal sealed class DefaultIfEmpty<T>(Observable<T> source, T? defaultValue) : Observable<T?>
{
    protected override IDisposable SubscribeCore(Observer<T?> observer)
    {
        return source.Subscribe(new _DefaultIfEmpty(observer, defaultValue));
    }

    sealed class _DefaultIfEmpty(Observer<T?> observer, T? defaultValue) : Observer<T>
    {
        bool hasValue;

        protected override void OnNextCore(T value)
        {
            hasValue = true;
            observer.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            if (!hasValue)
            {
                observer.OnNext(defaultValue);
            }
            observer.OnCompleted(result);
        }
    }
}
