namespace R2;

public static partial class EventExtensions
{
    // TODO: doOnSubscribed

    // Finally
    public static IEvent<TMessage> DoOnDisposed<TMessage>(this IEvent<TMessage> source, Action action)
    {
        return new DoOnDisposed<TMessage>(source, action);
    }

    public static IEvent<TMessage> DoOnDisposed<TMessage, TState>(this IEvent<TMessage> source, Action<TState> action, TState state)
    {
        return new DoOnDisposed<TMessage, TState>(source, action, state);
    }

    public static ICompletableEvent<TMessage, TComplete> DoOnDisposed<TMessage, TComplete>(this ICompletableEvent<TMessage, TComplete> source, Action action)
    {
        return new DoOnDisposed2<TMessage, TComplete>(source, action);
    }

    public static ICompletableEvent<TMessage, TComplete> DoOnDisposed<TMessage, TComplete, TState>(this ICompletableEvent<TMessage, TComplete> source, Action<TState> action, TState state)
    {
        return new DoOnDisposed2<TMessage, TComplete, TState>(source, action, state);
    }
}

internal sealed class DoOnDisposed<TMessage>(IEvent<TMessage> source, Action action) : IEvent<TMessage>
{
    public IDisposable Subscribe(ISubscriber<TMessage> subscriber)
    {
        var method = new _DoOnDisposed(subscriber, action);
        method.SourceSubscription = source.Subscribe(method);
        return method;
    }

    class _DoOnDisposed(ISubscriber<TMessage> subscriber, Action action) : ISubscriber<TMessage>, IDisposable
    {
        Action? action = action;
        public IDisposable? SourceSubscription;

        public void OnNext(TMessage message)
        {
            subscriber.OnNext(message);
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref action, null)?.Invoke();
            SourceSubscription?.Dispose();
        }
    }
}

internal sealed class DoOnDisposed<TMessage, TState>(IEvent<TMessage> source, Action<TState> action, TState state) : IEvent<TMessage>
{
    public IDisposable Subscribe(ISubscriber<TMessage> subscriber)
    {
        var method = new _DoOnDisposed(subscriber, action, state);
        method.SourceSubscription = source.Subscribe(method);
        return method;
    }

    class _DoOnDisposed(ISubscriber<TMessage> subscriber, Action<TState> action, TState state) : ISubscriber<TMessage>, IDisposable
    {
        Action<TState>? action = action;
        TState state = state;
        public IDisposable? SourceSubscription;

        public void OnNext(TMessage message)
        {
            subscriber.OnNext(message);
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref action, null)?.Invoke(state);
            state = default!;
            SourceSubscription?.Dispose();
        }
    }
}

internal sealed class DoOnDisposed2<TMessage, TComplete>(ICompletableEvent<TMessage, TComplete> source, Action action) : ICompletableEvent<TMessage, TComplete>
{
    public IDisposable Subscribe(ISubscriber<TMessage, TComplete> subscriber)
    {
        var method = new _DoOnDisposed(subscriber, action);
        method.SourceSubscription.Disposable = source.Subscribe(method);
        return method;
    }

    class _DoOnDisposed(ISubscriber<TMessage, TComplete> subscriber, Action action) : ISubscriber<TMessage, TComplete>, IDisposable
    {
        Action? action = action;
        public SingleAssignmentDisposableCore SourceSubscription;

        public void OnNext(TMessage message)
        {
            subscriber.OnNext(message);
        }

        public void OnCompleted(TComplete complete)
        {
            try
            {
                subscriber.OnCompleted(complete);
            }
            finally
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref action, null)?.Invoke();
            SourceSubscription.Dispose();
        }
    }
}

internal sealed class DoOnDisposed2<TMessage, TComplete, TState>(ICompletableEvent<TMessage, TComplete> source, Action<TState> action, TState state) : ICompletableEvent<TMessage, TComplete>
{
    public IDisposable Subscribe(ISubscriber<TMessage, TComplete> subscriber)
    {
        var method = new _DoOnDisposed(subscriber, action, state);
        method.SourceSubscription.Disposable = source.Subscribe(method);
        return method;
    }

    class _DoOnDisposed(ISubscriber<TMessage, TComplete> subscriber, Action<TState> action, TState state) : ISubscriber<TMessage, TComplete>, IDisposable
    {
        Action<TState>? action = action;
        TState state = state;
        public SingleAssignmentDisposableCore SourceSubscription;

        public void OnNext(TMessage message)
        {
            subscriber.OnNext(message);
        }

        public void OnCompleted(TComplete complete)
        {
            try
            {
                subscriber.OnCompleted(complete);
            }
            finally
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref action, null)?.Invoke(state);
            state = default!;
            SourceSubscription.Dispose();
        }
    }
}