namespace R3;

public static partial class Observable
{
    public static Observable<T> Create<T>(Func<Observer<T>, IDisposable> subscribe, bool rawObserver = false)
    {
        return new AnonymousObservable<T>(subscribe, rawObserver);
    }

    public static Observable<T> Create<T, TState>(TState state, Func<Observer<T>, TState, IDisposable> subscribe, bool rawObserver = false)
    {
        return new AnonymousObservable<T, TState>(state, subscribe, rawObserver);
    }

    public static Observable<T> Create<T>(Func<Observer<T>, CancellationToken, ValueTask> subscribe, bool rawObserver = false)
    {
        return new AsyncAnonymousObservable<T>(subscribe, rawObserver);
    }

    public static Observable<T> Create<T, TState>(TState state, Func<Observer<T>, TState, CancellationToken, ValueTask> subscribe, bool rawObserver = false)
    {
        return new AsyncAnonymousObservable<T, TState>(state, subscribe, rawObserver);
    }

    public static Observable<T> CreateFrom<T>(Func<CancellationToken, IAsyncEnumerable<T>> factory)
    {
        return new CreateFrom<T>(factory);
    }

    public static Observable<T> CreateFrom<T, TState>(TState state, Func<CancellationToken, TState, IAsyncEnumerable<T>> factory)
    {
        return new CreateFrom<T, TState>(state, factory);
    }
}

internal sealed class AnonymousObservable<T>(Func<Observer<T>, IDisposable> subscribe, bool rawObserver) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return subscribe(rawObserver ? observer : observer.Wrap());
    }
}

internal sealed class AnonymousObservable<T, TState>(TState state, Func<Observer<T>, TState, IDisposable> subscribe, bool rawObserver) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return subscribe(rawObserver ? observer : observer.Wrap(), state);
    }
}

internal sealed class AsyncAnonymousObservable<T>(Func<Observer<T>, CancellationToken, ValueTask> subscribe, bool rawObserver)
    : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var cancellationDisposable = new CancellationDisposable();
        subscribe(rawObserver ? observer : observer.Wrap(), cancellationDisposable.Token);
        return cancellationDisposable;
    }
}

internal sealed class AsyncAnonymousObservable<T, TState>(TState state, Func<Observer<T>, TState, CancellationToken, ValueTask> subscribe, bool rawObserver)
    : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var cancellationDisposable = new CancellationDisposable();
        subscribe(rawObserver ? observer : observer.Wrap(), state, cancellationDisposable.Token);
        return cancellationDisposable;
    }
}

internal sealed class CreateFrom<T>(Func<CancellationToken, IAsyncEnumerable<T>> factory) : Observable<T>
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
            await foreach (var message in factory(cancellationToken))
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

internal sealed class CreateFrom<T, TState>(TState state, Func<CancellationToken, TState, IAsyncEnumerable<T>> factory) : Observable<T>
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
            await foreach (var message in factory(cancellationToken, state))
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
