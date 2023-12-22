namespace R3;

public static partial class Observable
{
    public static Observable<T> ToObservable<T>(this Task<T> task)
    {
        return new TaskToObservable<T>(task);
    }

    // TODO: CancellationToken
    public static Observable<T> ToObservable<T>(this IEnumerable<T> source)
    {
        return new EnumerableToObservable<T>(source);
    }

    public static Observable<T> ToObservable<T>(this IObservable<T> source)
    {
        return new IObservableToObservable<T>(source);
    }
}

internal sealed class TaskToObservable<T>(Task<T> task) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var subscription = new CancellationDisposable();
        SubscribeTask(observer, subscription.Token);
        return subscription;
    }

    async void SubscribeTask(Observer<T> observer, CancellationToken cancellationToken)
    {
        T? result;
        try
        {
            result = await task.WaitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException oce && oce.CancellationToken == cancellationToken) // disposed.
            {
                return;
            }

            observer.OnCompleted(ex);
            return;
        }

        observer.OnNext(result);
        observer.OnCompleted();
    }
}

internal class EnumerableToObservable<T>(IEnumerable<T> source) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        foreach (var message in source)
        {
            observer.OnNext(message);
        }
        observer.OnCompleted();
        return Disposable.Empty;
    }
}

internal class IObservableToObservable<T>(IObservable<T> source) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new ObserverToObserver(observer));
    }

    sealed class ObserverToObserver(Observer<T> observer) : IObserver<T>
    {
        public void OnNext(T value)
        {
            observer.OnNext(value);
        }

        public void OnError(Exception error)
        {
            observer.OnCompleted(error);
        }

        public void OnCompleted()
        {
            observer.OnCompleted();
        }
    }
}
