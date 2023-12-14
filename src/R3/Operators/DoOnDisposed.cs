namespace R3;

public static partial class EventExtensions
{
    // TODO: doOnSubscribed

    // Finally
    public static Event<T> DoOnDisposed<T>(this Event<T> source, Action action)
    {
        return new DoOnDisposed<T>(source, action);
    }

    public static Event<T> DoOnDisposed<T, TState>(this Event<T> source, Action<TState> action, TState state)
    {
        return new DoOnDisposed<T, TState>(source, action, state);
    }
}

sealed class DoOnDisposed<T>(Event<T> source, Action action) : Event<T>
{
    protected override IDisposable SubscribeCore(Subscriber<T> subscriber)
    {
        var method = new _DoOnDisposed(subscriber, action);
        source.Subscribe(method);
        return method;
    }

    class _DoOnDisposed(Subscriber<T> subscriber, Action action) : Subscriber<T>, IDisposable
    {
        Action? action = action;

        protected override void OnNextCore(T value)
        {
            subscriber.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            subscriber.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            subscriber.OnCompleted(result);
        }

        protected override void DisposeCore()
        {
            Interlocked.Exchange(ref action, null)?.Invoke();
        }
    }
}

internal sealed class DoOnDisposed<T, TState>(Event<T> source, Action<TState> action, TState state) : Event<T>
{
    protected override IDisposable SubscribeCore(Subscriber<T> subscriber)
    {
        var method = new _DoOnDisposed(subscriber, action, state);
        source.Subscribe(method);
        return method;
    }

    class _DoOnDisposed(Subscriber<T> subscriber, Action<TState> action, TState state) : Subscriber<T>, IDisposable
    {
        Action<TState>? action = action;
        TState state = state;

        protected override void OnNextCore(T value)
        {
            subscriber.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            subscriber.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            subscriber.OnCompleted(result);
        }

        protected override void DisposeCore()
        {
            Interlocked.Exchange(ref action, null)?.Invoke(state);
            state = default!;
        }
    }
}
