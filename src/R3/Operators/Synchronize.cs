namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> Synchronize<T>(this Observable<T> source)
    {
        return new Synchronize<T>(source, new object());
    }

    public static Observable<T> Synchronize<T>(this Observable<T> source, object gate)
    {
        return new Synchronize<T>(source, gate);
    }
}


internal sealed class Synchronize<T>(Observable<T> source, object gate) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _Synchronize(observer, gate));
    }

    sealed class _Synchronize(Observer<T> observer, object gate) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            lock (gate)
            {
                observer.OnNext(value);
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            lock (gate)
            {
                observer.OnErrorResume(error);
            }
        }

        protected override void OnCompletedCore(Result result)
        {
            lock (gate)
            {
                observer.OnCompleted(result);
            }
        }
    }
}
