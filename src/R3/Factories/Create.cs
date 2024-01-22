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
