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
            var subscriber = new AggregateAsync<TMessage, TComplete, TAccumulate, TResult>(seed, func, resultSelector, cancellationToken);
            source.Subscribe(subscriber);
            return subscriber.Task;
        }

        public static Task<TResult> AggregateAsync<TMessage, TComplete, TAccumulate, TResult>
            (this CompletableEvent<TMessage, Result<TComplete>> source,
            TAccumulate seed,
            Func<TAccumulate, TMessage, TAccumulate> func,
            Func<TAccumulate, Result<TComplete>, TResult> resultSelector,
            CancellationToken cancellationToken = default)
        {
            var subscriber = new AggregateAsyncR<TMessage, TComplete, TAccumulate, TResult>(seed, func, resultSelector, cancellationToken);
            source.Subscribe(subscriber);
            return subscriber.Task;
        }
    }
}

namespace R3.Operators
{
    internal sealed class AggregateAsync<TMessage, TComplete, TAccumulate, TResult>(
        TAccumulate seed,
        Func<TAccumulate, TMessage, TAccumulate> func,
        Func<TAccumulate, TComplete, TResult> resultSelector,
        CancellationToken cancellationToken)
        : TaskSubscriberBase<TMessage, TComplete, TResult>(cancellationToken)
    {
        TAccumulate value = seed;

        protected override void OnNextCore(TMessage message)
        {
            value = func(value, message); // OnNext error is route to OnErrorResumeCore
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            TrySetException(error);
        }

        protected override void OnCompletedCore(TComplete complete)
        {
            try
            {
                var result = resultSelector(value, complete); // trap this resultSelector exception
                TrySetResult(result);
            }
            catch (Exception ex)
            {
                TrySetException(ex);
            }
        }
    }

    internal sealed class AggregateAsyncR<TMessage, TComplete, TAccumulate, TResult>(
        TAccumulate seed,
        Func<TAccumulate, TMessage, TAccumulate> func,
        Func<TAccumulate, Result<TComplete>, TResult> resultSelector,
        CancellationToken cancellationToken) : TaskSubscriberBase<TMessage, Result<TComplete>, TResult>(cancellationToken)
    {
        TAccumulate value = seed;

        protected override void OnNextCore(TMessage message)
        {
            value = func(value, message);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            TrySetException(error);
        }

        protected override void OnCompletedCore(Result<TComplete> complete)
        {
            try
            {
                var result = resultSelector(value, complete); // trap this resultSelector exception
                if (complete.IsSuccess)
                {
                    TrySetResult(result);
                }
                else
                {
                    TrySetException(complete.Exception);
                }
            }
            catch (Exception ex)
            {
                TrySetException(ex);
            }
        }
    }
}
