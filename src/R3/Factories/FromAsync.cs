namespace R3;

public static partial class Observable
{
    public static Observable<Unit> FromAsync(Func<CancellationToken, ValueTask> asyncFactory, bool configureAwait = true)
    {
        return new FromAsync(asyncFactory, configureAwait);
    }

    public static Observable<T> FromAsync<T>(Func<CancellationToken, ValueTask<T>> asyncFactory, bool configureAwait = true)
    {
        return new FromAsync<T>(asyncFactory, configureAwait);
    }
}

internal sealed class FromAsync(Func<CancellationToken, ValueTask> asyncFactory, bool configureAwait) : Observable<Unit>
{
    protected override IDisposable SubscribeCore(Observer<Unit> observer)
    {
        var cancellationDisposable = new CancellationDisposable();

        SubscribeTask(observer, cancellationDisposable.Token);

        return cancellationDisposable;
    }

    async void SubscribeTask(Observer<Unit> observer, CancellationToken cancellationToken)
    {
        try
        {
            await asyncFactory(cancellationToken).ConfigureAwait(configureAwait);
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException oce && oce.CancellationToken == cancellationToken)
            {
                return;
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                observer.OnCompleted(ex);
            }
            return;
        }

        if (!cancellationToken.IsCancellationRequested)
        {
            observer.OnNext(default);
            observer.OnCompleted();
        }
    }
}

internal sealed class FromAsync<T>(Func<CancellationToken, ValueTask<T>> asyncFactory, bool configureAwait) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var cancellationDisposable = new CancellationDisposable();

        SubscribeTask(observer, cancellationDisposable.Token);

        return cancellationDisposable;
    }

    async void SubscribeTask(Observer<T> observer, CancellationToken cancellationToken)
    {
        T? result;
        try
        {
            result = await asyncFactory(cancellationToken).ConfigureAwait(configureAwait);
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException oce && oce.CancellationToken == cancellationToken)
            {
                return;
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                observer.OnCompleted(ex);
            }
            return;
        }

        if (!cancellationToken.IsCancellationRequested)
        {
            observer.OnNext(result);
            observer.OnCompleted();
        }
    }
}
