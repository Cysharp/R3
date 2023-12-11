namespace R3
{
    public static partial class EventExtensions
    {
        public static async Task<TMessage[]> ToArrayAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, CancellationToken cancellationToken = default)
        {
            return (await source.ToListAsync(cancellationToken).ConfigureAwait(false)).ToArray();
        }

        public static async Task<TMessage[]> ToArrayAsync<TMessage, TComplete>(this CompletableEvent<TMessage, Result<TComplete>> source, CancellationToken cancellationToken = default)
        {
            return (await source.ToListAsync(cancellationToken).ConfigureAwait(false)).ToArray();
        }

        public static async Task<List<TMessage>> ToListAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<List<TMessage>>();

            using var subscription = source.Subscribe(new ToListAsync<TMessage, TComplete>(tcs));
            using var registration = cancellationToken.UnsafeRegister(static state =>
            {
                ((TaskCompletionSource<List<TMessage>>)state!).TrySetCanceled();
            }, tcs);

            // when canceled, throws TaskCanceledException in here and subscription.Dispose() is called.
            return await tcs.Task.ConfigureAwait(false);
        }

        public static async Task<List<TMessage>> ToListAsync<TMessage, TComplete>(this CompletableEvent<TMessage, Result<TComplete>> source, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<List<TMessage>>();

            using var subscription = source.Subscribe(new ToListAsyncR<TMessage, TComplete>(tcs));
            using var registration = cancellationToken.UnsafeRegister(static state =>
            {
                ((TaskCompletionSource<List<TMessage>>)state!).TrySetCanceled();
            }, tcs);

            // when canceled, throws TaskCanceledException in here and subscription.Dispose() is called.
            return await tcs.Task.ConfigureAwait(false);
        }
    }
}


namespace R3.Operators
{
    internal sealed class ToListAsync<TMessage, TComplete>(TaskCompletionSource<List<TMessage>> tcs) : Subscriber<TMessage, TComplete>
    {
        List<TMessage> list = new List<TMessage>();

        protected override void OnNextCore(TMessage message)
        {
            list.Add(message);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            EventSystem.GetUnhandledExceptionHandler().Invoke(error);
        }

        protected override void OnCompletedCore(TComplete complete)
        {
            tcs.TrySetResult(list); // complete result is ignored.
        }
    }

    internal sealed class ToListAsyncR<TMessage, TComplete>(TaskCompletionSource<List<TMessage>> tcs) : Subscriber<TMessage, Result<TComplete>>
    {
        List<TMessage> list = new List<TMessage>();

        protected override void OnNextCore(TMessage message)
        {
            list.Add(message);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            EventSystem.GetUnhandledExceptionHandler().Invoke(error);
        }

        protected override void OnCompletedCore(Result<TComplete> complete)
        {
            if (complete.IsSuccess)
            {
                tcs.TrySetResult(list); // complete result is ignored.
            }
            else
            {
                tcs.TrySetException(complete.Exception);
            }
        }
    }
}
