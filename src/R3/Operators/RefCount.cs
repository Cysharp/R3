namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> RefCount<T>(this ConnectableObservable<T> source)
    {
        return new RefCount<T>(source);
    }
}

internal sealed class RefCount<T>(ConnectableObservable<T> source) : Observable<T>
{
    readonly object gate = new object();
    int refCount = 0;
    IDisposable? connection;

    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        lock (gate)
        {
            // incr refCount before Subscribe(completed source decrement refCount in Subscribe)
            ++refCount;
            bool needConnect = refCount == 1;
            var coObserver = new _RefCount(this, observer);
            var subcription = source.Subscribe(coObserver);
            if (needConnect && !coObserver.IsDisposed)
            {
                connection = source.Connect();
            }
            return subcription;
        }
    }

    sealed class _RefCount(RefCount<T> parent, Observer<T> observer) : Observer<T>
    {
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

        protected override void DisposeCore()
        {
            lock (parent.gate)
            {
                if (--parent.refCount == 0)
                {
                    parent.connection?.Dispose();
                    parent.connection = null;
                }
            }
        }
    }
}
