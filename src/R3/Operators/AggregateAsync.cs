namespace R3
{
    public static partial class EventExtensions
    {
        public static Task<TResult> AggregateAsync<TMessage, TComplete, TAccumulate, TResult>
            (this CompletableEvent<TMessage, TComplete> source,
            TAccumulate seed,
            Func<TAccumulate, TMessage, TAccumulate> func,
            Func<TAccumulate, TComplete, TResult> resultSelector,
            CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<TResult>();

            var subscriber = new AggregateAsync<TMessage, TComplete, TAccumulate, TResult>(tcs, seed, func, resultSelector, cancellationToken);

            // before Subscribe, register and set CancellationTokenRegistration
            subscriber.tokenRegistration = cancellationToken.UnsafeRegister(static state =>
            {
                var s = ((AggregateAsync<TMessage, TComplete, TAccumulate, TResult>)state!);

                s.Dispose(); // subscriber is subscription, dispose
                s.tcs.TrySetCanceled(s.cancellationToken);
            }, subscriber);

            source.Subscribe(subscriber); // return subscriber self so ignore subscription

            // when canceled, throws TaskCanceledException in here and subscription.Dispose() is called.
            return tcs.Task;
        }

        public static Task<TResult> AggregateAsync<TMessage, TComplete, TAccumulate, TResult>
            (this CompletableEvent<TMessage, Result<TComplete>> source,
            TAccumulate seed,
            Func<TAccumulate, TMessage, TAccumulate> func,
            Func<TAccumulate, Result<TComplete>, TResult> resultSelector,
            CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<TResult>();

            var subscriber = new AggregateAsyncR<TMessage, TComplete, TAccumulate, TResult>(tcs, seed, func, resultSelector, cancellationToken);

            // before Subscribe, register and set CancellationTokenRegistration
            subscriber.tokenRegistration = cancellationToken.UnsafeRegister(static state =>
            {
                var s = ((AggregateAsyncR<TMessage, TComplete, TAccumulate, TResult>)state!);

                s.Dispose(); // subscriber is subscription, dispose
                s.tcs.TrySetCanceled(s.cancellationToken);
            }, subscriber);

            source.Subscribe(subscriber); // return subscriber self so ignore subscription

            // when canceled, throws TaskCanceledException in here and subscription.Dispose() is called.
            return tcs.Task;
        }
    }
}

namespace R3.Operators
{
    internal sealed class AggregateAsync<TMessage, TComplete, TAccumulate, TResult>(
       TaskCompletionSource<TResult> tcs,
       TAccumulate seed,
       Func<TAccumulate, TMessage, TAccumulate> func,
       Func<TAccumulate, TComplete, TResult> resultSelector,
       CancellationToken cancellationToken) : Subscriber<TMessage, TComplete>
    {
        // hold state for CancellationToken.Register
        internal TaskCompletionSource<TResult> tcs = tcs;
        internal CancellationToken cancellationToken = cancellationToken;
        internal CancellationTokenRegistration tokenRegistration;

        TAccumulate value = seed;

        protected override void OnNextCore(TMessage message)
        {
            value = func(value, message); // OnNext error is route to OnErrorResumeCore
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            tcs.TrySetException(error);
        }

        protected override void OnCompletedCore(TComplete complete)
        {
            try
            {
                var result = resultSelector(value, complete);
                tcs.TrySetResult(result);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }

        protected override void DisposeCore()
        {
            tokenRegistration.Dispose();
        }
    }

    internal sealed class AggregateAsyncR<TMessage, TComplete, TAccumulate, TResult>(
        TaskCompletionSource<TResult> tcs,
        TAccumulate seed,
        Func<TAccumulate, TMessage, TAccumulate> func,
        Func<TAccumulate, Result<TComplete>, TResult> resultSelector,
        CancellationToken cancellationToken) : Subscriber<TMessage, Result<TComplete>>
    {
        // hold state for CancellationToken.Register
        internal TaskCompletionSource<TResult> tcs = tcs;
        internal CancellationToken cancellationToken = cancellationToken;
        internal CancellationTokenRegistration tokenRegistration;

        TAccumulate value = seed;

        protected override void OnNextCore(TMessage message)
        {
            value = func(value, message);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            tcs.TrySetException(error);
        }

        protected override void OnCompletedCore(Result<TComplete> complete)
        {
            try
            {
                var result = resultSelector(value, complete);
                if (complete.IsSuccess)
                {
                    tcs.TrySetResult(result);
                }
                else
                {
                    tcs.TrySetException(complete.Exception);
                }
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }

        protected override void DisposeCore()
        {
            tokenRegistration.Dispose();
        }
    }
}
