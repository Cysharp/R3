namespace R3;

public static partial class ObservableExtensions
{
    // TODO: doOnSubscribed

    // Finally
    public static Observable<T> DoOnDisposed<T>(this Observable<T> source, Action action)
    {
        return new DoOnDisposed<T>(source, action);
    }

    public static Observable<T> DoOnDisposed<T, TState>(this Observable<T> source, TState state, Action<TState> action)
    {
        return new DoOnDisposed<T, TState>(source, action, state);
    }
}

sealed class DoOnDisposed<T>(Observable<T> source, Action action) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var method = new _DoOnDisposed(observer, action);
        source.Subscribe(method);
        return method;
    }

    class _DoOnDisposed(Observer<T> observer, Action action) : Observer<T>, IDisposable
    {
        Action? action = action;

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
            Interlocked.Exchange(ref action, null)?.Invoke();
        }
    }
}

internal sealed class DoOnDisposed<T, TState>(Observable<T> source, Action<TState> action, TState state) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var method = new _DoOnDisposed(observer, action, state);
        source.Subscribe(method);
        return method;
    }

    class _DoOnDisposed(Observer<T> observer, Action<TState> action, TState state) : Observer<T>, IDisposable
    {
        Action<TState>? action = action;
        TState state = state;

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
            Interlocked.Exchange(ref action, null)?.Invoke(state);
            state = default!;
        }
    }
}
