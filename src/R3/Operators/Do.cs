namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> Do<T>(this Observable<T> source, Action<T>? onNext = null, Action<Exception>? onErrorResume = null, Action<Result>? onCompleted = null, Action? onDispose = null, Action? onSubscribe = null)
    {
        return new Do<T>(source, onNext, onErrorResume, onCompleted, onDispose, onSubscribe);
    }

    public static Observable<T> Do<T, TState>(this Observable<T> source, TState state, Action<T, TState>? onNext = null, Action<Exception, TState>? onErrorResume = null, Action<Result, TState>? onCompleted = null, Action<TState>? onDispose = null, Action<TState>? onSubscribe = null)
    {
        return new Do<T, TState>(source, state, onNext, onErrorResume, onCompleted, onDispose, onSubscribe);
    }

    public static Observable<T> DoCancelOnCompleted<T>(this Observable<T> source, CancellationTokenSource cancellationTokenSource)
    {
        return Do(source, cancellationTokenSource, onCompleted: static (_, state) => state.Cancel());
    }
}

internal sealed class Do<T>(Observable<T> source, Action<T>? onNext, Action<Exception>? onErrorResume, Action<Result>? onCompleted, Action? onDispose, Action? onSubscribe) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        onSubscribe?.Invoke();
        return source.Subscribe(new _Do(observer, onNext, onErrorResume, onCompleted, onDispose));
    }

    internal sealed class _Do(Observer<T> observer, Action<T>? onNext, Action<Exception>? onErrorResume, Action<Result>? onCompleted, Action? onDispose) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            onNext?.Invoke(value);
            observer.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            onErrorResume?.Invoke(error);
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            onCompleted?.Invoke(result);
            observer.OnCompleted(result);
        }

        protected override void DisposeCore()
        {
            onDispose?.Invoke();
        }
    }
}

internal sealed class Do<T, TState>(Observable<T> source, TState state, Action<T, TState>? onNext, Action<Exception, TState>? onErrorResume, Action<Result, TState>? onCompleted, Action<TState>? onDispose, Action<TState>? onSubscribe) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        onSubscribe?.Invoke(state);
        return source.Subscribe(new _Do(observer, state, onNext, onErrorResume, onCompleted, onDispose));
    }

    internal sealed class _Do(Observer<T> observer, TState state, Action<T, TState>? onNext, Action<Exception, TState>? onErrorResume, Action<Result, TState>? onCompleted, Action<TState>? onDispose) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            onNext?.Invoke(value, state);
            observer.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            onErrorResume?.Invoke(error, state);
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            onCompleted?.Invoke(result, state);
            observer.OnCompleted(result);
        }

        protected override void DisposeCore()
        {
            onDispose?.Invoke(state);
        }
    }
}
