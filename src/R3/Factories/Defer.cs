namespace R3;

public static partial class Observable
{
    public static Observable<T> Defer<T>(Func<Observable<T>> observableFactory, bool rawObserver = false)
    {
        return new Defer<T>(observableFactory, rawObserver);
    }

    public static Observable<T> Defer<T, TState>(TState state, Func<TState, Observable<T>> observableFactory, bool rawObserver = false)
    {
        return new Defer<T, TState>(state, observableFactory, rawObserver);
    }
}

internal sealed class Defer<T>(Func<Observable<T>> observableFactory, bool rawObserver) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var observable = default(Observable<T>);
        try
        {
            observable = observableFactory();
        }
        catch (Exception ex)
        {
            observer.OnCompleted(ex); // when failed, return Completed(Error)
            return Disposable.Empty;
        }

        return observable.Subscribe(rawObserver ? observer : observer.Wrap());
    }
}

internal sealed class Defer<T, TState>(TState state, Func<TState, Observable<T>> observableFactory, bool rawObserver) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var observable = default(Observable<T>);
        try
        {
            observable = observableFactory(state);
        }
        catch (Exception ex)
        {
            observer.OnCompleted(ex); // when failed, return Completed(Error)
            return Disposable.Empty;
        }

        return observable.Subscribe(rawObserver ? observer : observer.Wrap());
    }
}
