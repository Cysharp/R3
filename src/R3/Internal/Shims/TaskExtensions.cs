#if NETSTANDARD2_0 || NETSTANDARD2_1

namespace R3;

internal static class TaskExtensions
{
    internal static Task<T> WaitAsync<T>(this Task<T> task, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<T>();
        var registration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));

        task.ContinueWith(t =>
        {
            registration.Dispose();
            if (t.IsFaulted)
            {
                tcs.TrySetException(t.Exception!.InnerExceptions);
            }
            else if (t.IsCanceled)
            {
                tcs.TrySetCanceled(cancellationToken);
            }
            else
            {
                tcs.TrySetResult(t.Result);
            }
        }, TaskScheduler.Default);

        return tcs.Task;
    }
}

#endif
