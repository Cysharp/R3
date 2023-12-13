namespace R3;

public static partial class EventExtensions
{
    // TODO: doOnSubscribed

    // Finally
    public static Event<TMessage, TComplete> DoOnDisposed<TMessage, TComplete>(this Event<TMessage, TComplete> source, Action action)
    {
        return new DoOnDisposed<TMessage, TComplete>(source, action);
    }

    public static Event<TMessage, TComplete> DoOnDisposed<TMessage, TComplete, TState>(this Event<TMessage, TComplete> source, Action<TState> action, TState state)
    {
        return new DoOnDisposed<TMessage, TComplete, TState>(source, action, state);
    }
}

internal sealed class DoOnDisposed<TMessage, TComplete>(Event<TMessage, TComplete> source, Action action) : Event<TMessage, TComplete>
{
    protected override IDisposable SubscribeCore(Subscriber<TMessage, TComplete> subscriber)
    {
        var method = new _DoOnDisposed(subscriber, action);
        source.Subscribe(method);
        return method;
    }

    class _DoOnDisposed(Subscriber<TMessage, TComplete> subscriber, Action action) : Subscriber<TMessage, TComplete>, IDisposable
    {
        Action? action = action;

        protected override void OnNextCore(TMessage message)
        {
            subscriber.OnNext(message);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            subscriber.OnErrorResume(error);
        }

        protected override void OnCompletedCore(TComplete complete)
        {
            subscriber.OnCompleted(complete);
        }

        protected override void DisposeCore()
        {
            Interlocked.Exchange(ref action, null)?.Invoke();
        }
    }
}

internal sealed class DoOnDisposed<TMessage, TComplete, TState>(Event<TMessage, TComplete> source, Action<TState> action, TState state) : Event<TMessage, TComplete>
{
    protected override IDisposable SubscribeCore(Subscriber<TMessage, TComplete> subscriber)
    {
        var method = new _DoOnDisposed(subscriber, action, state);
        source.Subscribe(method);
        return method;
    }

    class _DoOnDisposed(Subscriber<TMessage, TComplete> subscriber, Action<TState> action, TState state) : Subscriber<TMessage, TComplete>, IDisposable
    {
        Action<TState>? action = action;
        TState state = state;

        protected override void OnNextCore(TMessage message)
        {
            subscriber.OnNext(message);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            subscriber.OnErrorResume(error);
        }

        protected override void OnCompletedCore(TComplete complete)
        {
            subscriber.OnCompleted(complete);
        }

        protected override void DisposeCore()
        {
            Interlocked.Exchange(ref action, null)?.Invoke(state);
            state = default!;
        }
    }
}
