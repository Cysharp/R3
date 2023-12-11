namespace R3
{
    public static partial class EventExtensions
    {
        // TODO: doOnSubscribed

        // Finally
        public static Event<TMessage> DoOnDisposed<TMessage>(this Event<TMessage> source, Action action)
        {
            return new DoOnDisposed<TMessage>(source, action);
        }

        public static Event<TMessage> DoOnDisposed<TMessage, TState>(this Event<TMessage> source, Action<TState> action, TState state)
        {
            return new DoOnDisposed<TMessage, TState>(source, action, state);
        }

        public static CompletableEvent<TMessage, TComplete> DoOnDisposed<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, Action action)
        {
            return new DoOnDisposed2<TMessage, TComplete>(source, action);
        }

        public static CompletableEvent<TMessage, TComplete> DoOnDisposed<TMessage, TComplete, TState>(this CompletableEvent<TMessage, TComplete> source, Action<TState> action, TState state)
        {
            return new DoOnDisposed2<TMessage, TComplete, TState>(source, action, state);
        }
    }
}

namespace R3.Operators
{
    internal sealed class DoOnDisposed<TMessage>(Event<TMessage> source, Action action) : Event<TMessage>
    {
        protected override IDisposable SubscribeCore(Subscriber<TMessage> subscriber)
        {
            var method = new _DoOnDisposed(subscriber, action);
            source.Subscribe(method);
            return method;
        }

        class _DoOnDisposed(Subscriber<TMessage> subscriber, Action action) : Subscriber<TMessage>, IDisposable
        {
            Action? action = action;

            public override void OnNextCore(TMessage message)
            {
                subscriber.OnNext(message);
            }

            protected override void DisposeCore()
            {
                Interlocked.Exchange(ref action, null)?.Invoke(); base.DisposeCore();
            }
        }
    }

    internal sealed class DoOnDisposed<TMessage, TState>(Event<TMessage> source, Action<TState> action, TState state) : Event<TMessage>
    {
        protected override IDisposable SubscribeCore(Subscriber<TMessage> subscriber)
        {
            var method = new _DoOnDisposed(subscriber, action, state);
            source.Subscribe(method);
            return method;
        }

        class _DoOnDisposed(Subscriber<TMessage> subscriber, Action<TState> action, TState state) : Subscriber<TMessage>, IDisposable
        {
            Action<TState>? action = action;
            TState state = state;

            public override void OnNextCore(TMessage message)
            {
                subscriber.OnNext(message);
            }

            protected override void DisposeCore()
            {
                Interlocked.Exchange(ref action, null)?.Invoke(state);
                state = default!;
            }
        }
    }

    internal sealed class DoOnDisposed2<TMessage, TComplete>(CompletableEvent<TMessage, TComplete> source, Action action) : CompletableEvent<TMessage, TComplete>
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

            public override void OnNextCore(TMessage message)
            {
                subscriber.OnNext(message);
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

    internal sealed class DoOnDisposed2<TMessage, TComplete, TState>(CompletableEvent<TMessage, TComplete> source, Action<TState> action, TState state) : CompletableEvent<TMessage, TComplete>
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

            public override void OnNextCore(TMessage message)
            {
                subscriber.OnNext(message);
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
}
