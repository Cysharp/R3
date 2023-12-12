namespace R3
{
    using static FirstLastSingleOperation;

    public static partial class EventExtensions
    {
        public static Task<TMessage> FirstAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, CancellationToken cancellationToken = default) => FirstAsync(source, static _ => true, cancellationToken);
        public static Task<TMessage> FirstOrDefaultAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, TMessage? defaultValue = default, CancellationToken cancellationToken = default) => FirstOrDefaultAsync(source, static _ => true, defaultValue, cancellationToken);
        public static Task<TMessage> LastAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, CancellationToken cancellationToken = default) => LastAsync(source, static _ => true, cancellationToken);
        public static Task<TMessage> LastOrDefaultAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, TMessage? defaultValue = default, CancellationToken cancellationToken = default) => LastOrDefaultAsync(source, static _ => true, defaultValue, cancellationToken);
        public static Task<TMessage> SingleAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, CancellationToken cancellationToken = default) => SingleAsync(source, static _ => true, cancellationToken);
        public static Task<TMessage> SingleOrDefaultAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, TMessage? defaultValue = default, CancellationToken cancellationToken = default) => SingleOrDefaultAsync(source, static _ => true, defaultValue, cancellationToken);

        // with predicate

        public static Task<TMessage> FirstAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, Func<TMessage, bool> predicate, CancellationToken cancellationToken = default) => FirstLastSingleAsync(source, First, false, default, predicate, cancellationToken);
        public static Task<TMessage> FirstOrDefaultAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, Func<TMessage, bool> predicate, TMessage? defaultValue = default, CancellationToken cancellationToken = default) => FirstLastSingleAsync(source, First, true, defaultValue, predicate, cancellationToken);
        public static Task<TMessage> LastAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, Func<TMessage, bool> predicate, CancellationToken cancellationToken = default) => FirstLastSingleAsync(source, Last, false, default, predicate, cancellationToken);
        public static Task<TMessage> LastOrDefaultAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, Func<TMessage, bool> predicate, TMessage? defaultValue = default, CancellationToken cancellationToken = default) => FirstLastSingleAsync(source, Last, true, defaultValue, predicate, cancellationToken);
        public static Task<TMessage> SingleAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, Func<TMessage, bool> predicate, CancellationToken cancellationToken = default) => FirstLastSingleAsync(source, Single, false, default, predicate, cancellationToken);
        public static Task<TMessage> SingleOrDefaultAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, Func<TMessage, bool> predicate, TMessage? defaultValue = default, CancellationToken cancellationToken = default) => FirstLastSingleAsync(source, Single, true, defaultValue, predicate, cancellationToken);

        // no complete, only use First
        public static Task<TMessage> FirstAsync<TMessage>(this Event<TMessage> source, CancellationToken cancellationToken = default) => FirstAsync(source, static _ => true, cancellationToken);

        public static Task<TMessage> FirstAsync<TMessage>(this Event<TMessage> source, Func<TMessage, bool> predicate, CancellationToken cancellationToken = default)
        {
            var subscriber = new First<TMessage>(predicate, cancellationToken);
            source.Subscribe(subscriber);
            return subscriber.Task;
        }

        static Task<TMessage> FirstLastSingleAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, FirstLastSingleOperation operation, bool useDefaultIfEmpty, TMessage? defaultValue, Func<TMessage, bool> predicate, CancellationToken cancellationToken)
        {
            var subscriber = new FirstLastSingle<TMessage, TComplete>(operation, useDefaultIfEmpty, defaultValue, predicate, cancellationToken);
            source.Subscribe(subscriber);
            return subscriber.Task;
        }
    }
}

namespace R3.Operators
{
    internal sealed class First<TMessage>(Func<TMessage, bool> predicate, CancellationToken cancellationToken)
        : TaskSubscriberBase<TMessage, TMessage>(cancellationToken)
    {
        protected override void OnNextCore(TMessage message)
        {
            if (!predicate(message)) return;

            tcs.TrySetResult(message); // First
            Dispose();
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            tcs.TrySetException(error);
        }
    }

    internal sealed class FirstLastSingle<TMessage, TComplete>(FirstLastSingleOperation operation, bool useDefaultIfEmpty, TMessage? defaultValue, Func<TMessage, bool> predicate, CancellationToken cancellationToken)
        : TaskSubscriberBase<TMessage, TComplete, TMessage>(cancellationToken)
    {
        bool hasValue;
        TMessage? latestValue = defaultValue;

        protected override void OnNextCore(TMessage message)
        {
            if (!predicate(message)) return;

            if (operation == FirstLastSingleOperation.Single && hasValue)
            {
                tcs.TrySetException(new InvalidOperationException("Sequence contains more than one element."));
            }

            hasValue = true;
            if (operation == FirstLastSingleOperation.First)
            {
                tcs.TrySetResult(message); // First / FirstOrDefault
                Dispose();
            }
            else
            {
                latestValue = message;
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            tcs.TrySetException(error);
        }

        protected override void OnCompletedCore(TComplete complete)
        {
            if (hasValue || useDefaultIfEmpty)
            {
                tcs.TrySetResult(latestValue!); // FirstOrDefault / Last / LastOrDefault / Single / SingleOrDefault
                return;
            }

            tcs.TrySetException(new InvalidOperationException("Sequence contains no elements."));
        }
    }

    internal enum FirstLastSingleOperation
    {
        First,
        Last,
        Single
    }
}
