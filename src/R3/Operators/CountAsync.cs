
namespace R3
{
    public static partial class EventExtensions
    {
        public static async Task<int> CountAsync<T, U>(this CompletableEvent<T, U> source, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<int>();

            using var subscription = source.Subscribe(new CountAsync<T, U>(tcs));
            using var registration = cancellationToken.UnsafeRegister(static state =>
            {
                ((TaskCompletionSource<int>)state!).TrySetCanceled();
            }, tcs);

            // when canceled, throws TaskCanceledException in here and subscription.Dispose() is called.
            return await tcs.Task.ConfigureAwait(false);
        }

        public static async Task<int> CountAsync<T, U>(this CompletableEvent<T, Result<U>> source, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<int>();

            using var subscription = source.Subscribe(new CountUAsync<T, U>(tcs));
            using var registration = cancellationToken.UnsafeRegister(static state =>
            {
                ((TaskCompletionSource<int>)state!).TrySetCanceled();
            }, tcs);

            // when canceled, throws TaskCanceledException in here and subscription.Dispose() is called.
            return await tcs.Task.ConfigureAwait(false);
        }
    }
}

namespace R3.Operators
{
    internal sealed class CountAsync<TMessage, TComplete>(TaskCompletionSource<int> tcs) : Subscriber<TMessage, TComplete>
    {
        int count;

        protected override void OnNextCore(TMessage message)
        {
            checked
            {
                count++;
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            tcs.TrySetException(error);
            this.Dispose(); // stop subscription.
        }

        protected override void OnCompletedCore(TComplete complete)
        {
            tcs.TrySetResult(count);
        }
    }

    internal sealed class CountUAsync<TMessage, TComplete>(TaskCompletionSource<int> tcs) : Subscriber<TMessage, Result<TComplete>>
    {
        int count;

        protected override void OnNextCore(TMessage message)
        {
            checked
            {
                count++;
            }
        }
        
        protected override void OnErrorResumeCore(Exception error)
        {
            tcs.TrySetException(error);
            this.Dispose(); // stop subscription.
        }

        protected override void OnCompletedCore(Result<TComplete> complete)
        {
            if (complete.IsSuccess)
            {
                tcs.TrySetResult(count);
            }
            else
            {
                tcs.TrySetException(complete.Exception);
            }
        }
    }
}
