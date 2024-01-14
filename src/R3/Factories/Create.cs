namespace R3;

public static partial class Observable
{
    public static Observable<T> Create<T>(Func<Observer<T>, IDisposable> subscribe)
    {
        return new AnonymousObservable<T>(subscribe);
    }

    public static Observable<T> Create<T, TState>(TState state, Func<Observer<T>, TState, IDisposable> subscribe)
    {
        return new AnonymousObservable<T, TState>(state, subscribe);
    }
}

internal sealed class AnonymousObservable<T>(Func<Observer<T>, IDisposable> subscribe) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return subscribe(observer.Wrap());
    }
}

internal sealed class AnonymousObservable<T, TState>(TState state, Func<Observer<T>, TState, IDisposable> subscribe) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return subscribe(observer.Wrap(), state);
    }
}
