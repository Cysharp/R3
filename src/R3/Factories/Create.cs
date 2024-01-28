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
