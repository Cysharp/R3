namespace R3;

public static partial class Observable
{
    public static Observable<Unit> ToObservable(this Task task, bool configureAwait = true)
    {
        return new TaskToObservable(task, configureAwait);
    }

    public static Observable<T> ToObservable<T>(this Task<T> task, bool configureAwait = true)
    {
        return new TaskToObservable<T>(task, configureAwait);
    }

    public static Observable<Unit> ToObservable(this ValueTask task, bool configureAwait = true)
    {
        return new ValueTaskToObservable(task, configureAwait);
    }

    public static Observable<T> ToObservable<T>(this ValueTask<T> task, bool configureAwait = true)
    {
        return new ValueTaskToObservable<T>(task, configureAwait);
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

internal sealed class TaskToObservable(Task task, bool configureAwait) : Observable<Unit>
{
    protected override IDisposable SubscribeCore(Observer<Unit> observer)
    {
        SubscribeTask(observer);
        return Disposable.Empty; // no need to return subscription
    }

    async void SubscribeTask(Observer<Unit> observer)
    {
        try
        {
            await task.ConfigureAwait(configureAwait);
        }
        catch (Exception ex)
        {
            if (!observer.IsDisposed)
            {
                observer.OnCompleted(ex);
            }
            return;
        }

        if (!observer.IsDisposed)
        {
            observer.OnNext(Unit.Default);
            observer.OnCompleted();
        }
    }
}

internal sealed class TaskToObservable<T>(Task<T> task, bool configureAwait) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        SubscribeTask(observer);
        return Disposable.Empty; // no need to return subscription
    }

    async void SubscribeTask(Observer<T> observer)
    {
        T? result;
        try
        {
            result = await task.ConfigureAwait(configureAwait);
        }
        catch (Exception ex)
        {
            if (!observer.IsDisposed)
            {
                observer.OnCompleted(ex);
            }
            return;
        }

        if (!observer.IsDisposed)
        {
            observer.OnNext(result);
            observer.OnCompleted();
        }
    }
}

internal sealed class ValueTaskToObservable(ValueTask task, bool configureAwait) : Observable<Unit>
{
    protected override IDisposable SubscribeCore(Observer<Unit> observer)
    {
        SubscribeTask(observer);
        return Disposable.Empty; // no need to return subscription
    }

    async void SubscribeTask(Observer<Unit> observer)
    {
        try
        {
            await task.ConfigureAwait(configureAwait);
        }
        catch (Exception ex)
        {
            if (!observer.IsDisposed)
            {
                observer.OnCompleted(ex);
            }
            return;
        }

        if (!observer.IsDisposed)
        {
            observer.OnNext(Unit.Default);
            observer.OnCompleted();
        }
    }
}

internal sealed class ValueTaskToObservable<T>(ValueTask<T> task, bool configureAwait) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        SubscribeTask(observer);
        return Disposable.Empty; // no need to return subscription
    }

    async void SubscribeTask(Observer<T> observer)
    {
        T? result;
        try
        {
            result = await task.ConfigureAwait(configureAwait);
        }
        catch (Exception ex)
        {
            if (!observer.IsDisposed)
            {
                observer.OnCompleted(ex);
            }
            return;
        }

        if (!observer.IsDisposed)
        {
            observer.OnNext(result);
            observer.OnCompleted();
        }
    }
}

internal sealed class EnumerableToObservable<T>(IEnumerable<T> source, CancellationToken cancellationToken) : Observable<T>
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

internal sealed class AsyncEnumerableToObservable<T>(IAsyncEnumerable<T> source) : Observable<T>
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
        catch (Exception ex)
        {
            if (ex is OperationCanceledException oce && oce.CancellationToken == cancellationToken) // disposed.
            {
                return;
            }

            observer.OnCompleted(Result.Failure(ex));
        }
    }
}

internal sealed class IObservableToObservable<T>(IObservable<T> source) : Observable<T>
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
