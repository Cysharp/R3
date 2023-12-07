namespace R2;

public static partial class EventExtensions
{
    public static async Task<int> CountAsync<T, U>(this ICompletableEvent<T, U> source, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<int>();

        using var subscription = source.Subscribe(new CountAsync<T, U>(tcs));
        using var registration = cancellationToken.Register(static state =>
        {
            ((TaskCompletionSource<int>)state!).TrySetCanceled();
        }, tcs);

        return await tcs.Task.ConfigureAwait(false);
    }

    public static async Task<int> CountAsync<T, U>(this ICompletableEvent<T, Result<U>> source, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<int>();

        using var subscription = source.Subscribe(new CountUAsync<T, U>(tcs));
        using var registration = cancellationToken.Register(static state =>
        {
            ((TaskCompletionSource<int>)state!).TrySetCanceled();
        }, tcs);

        return await tcs.Task.ConfigureAwait(false);
    }
}

internal sealed class CountAsync<TMessage, TComplete>(TaskCompletionSource<int> tcs) : ISubscriber<TMessage, TComplete>
{
    int count;

    public void OnNext(TMessage message)
    {
        Interlocked.Increment(ref count);
    }

    public void OnCompleted(TComplete complete)
    {
        tcs.TrySetResult(count);
    }
}

internal sealed class CountUAsync<TMessage, TComplete>(TaskCompletionSource<int> tcs) : ISubscriber<TMessage, Result<TComplete>>
{
    int count;

    public void OnNext(TMessage message)
    {
        Interlocked.Increment(ref count);
    }

    public void OnCompleted(Result<TComplete> complete)
    {
        if (complete.HasValue)
        {
            tcs.TrySetResult(count);
        }
        else
        {
            tcs.TrySetException(complete.Exception);
        }
    }
}