namespace R3;

public static partial class Observable
{
    public static Observable<T> ToObservable<T>(this Task<T> task)
    {
        return new TaskToObservable<T>(task);
    }

    public static Observable<T> ToObservable<T>(this IEnumerable<T> source, CancellationToken cancellationToken = default)
    {
        return new EnumerableToObservable<T>(source, cancellationToken);
    }

    public static Observable<T> ToObservable<T>(this IAsyncEnumerable<T> source)
    {
        return new AsyncEnumerableToObservable<T>(source);
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

internal class EnumerableToObservable<T>(IEnumerable<T> source, CancellationToken cancellationToken) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        foreach (var message in source)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                observer.OnCompleted();
                return Disposable.Empty;
            }
            observer.OnNext(message);
        }
        observer.OnCompleted();
        return Disposable.Empty;
    }
}

internal class AsyncEnumerableToObservable<T>(IAsyncEnumerable<T> source) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var cancellationDisposable = new CancellationDisposable();
        RunAsync(observer, cancellationDisposable.Token);
        return cancellationDisposable;
    }

    async void RunAsync(Observer<T> observer, CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var message in source.WithCancellation(cancellationToken))
            {
                observer.OnNext(message);
            }
            observer.OnCompleted();
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            ObservableSystem.GetUnhandledExceptionHandler().Invoke(ex);
        }
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
