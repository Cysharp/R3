namespace R3;

public static partial class ObservableExtensions
{
    public static Task<T[]> ToArrayAsync<T>(this Observable<T> source, CancellationToken cancellationToken = default)
    {
        var method = new ToArrayAsync<T>(cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }
}

internal sealed class ToArrayAsync<T>(CancellationToken cancellationToken) : TaskObserverBase<T, T[]>(cancellationToken)
{
    readonly List<T> buffer = [];

    protected override void OnNextCore(T value)
    {
        buffer.Add(value);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        TrySetException(error);
    }

    protected override void OnCompletedCore(Result result)
    {
        if (result.IsFailure)
        {
            TrySetException(result.Exception);
            return;
        }
        TrySetResult(buffer.ToArray());
    }
}
