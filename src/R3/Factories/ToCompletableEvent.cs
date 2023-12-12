﻿namespace R3
{
    public static partial class EventFactory
    {
        public static CompletableEvent<TMessage, Result<Unit>> ToCompletableEvent<TMessage>(this Task<TMessage> task)
        {
            return new R3.Factories.ToCompletableEvent<TMessage>(task);
        }
    }
}

namespace R3.Factories
{
    internal sealed class ToCompletableEvent<TMessage>(Task<TMessage> task) : CompletableEvent<TMessage, Result<Unit>>
    {
        protected override IDisposable SubscribeCore(Subscriber<TMessage, Result<Unit>> subscriber)
        {
            var subscription = new CancellationDisposable();
            SubscribeTask(subscriber, subscription.Token);
            return subscription;
        }

        async void SubscribeTask(Subscriber<TMessage, Result<Unit>> subscriber, CancellationToken cancellationToken)
        {
            TMessage? result;
            try
            {
                result = await task.WaitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                subscriber.OnCompleted(Result.Failure<Unit>(ex));
                return;
            }

            subscriber.OnNext(result);
            subscriber.OnCompleted(Result.Success<Unit>(default));
        }
    }
}