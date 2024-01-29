namespace R3;

public static partial class ObservableExtensions
{
    public static Task<List<T>> ToListAsync<T>(this Observable<T> source, CancellationToken cancellationToken = default)
    {
        var method = new ToListAsync<T>(cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }
}

internal sealed class ToListAsync<T>(CancellationToken cancellationToken) : TaskObserverBase<T, List<T>>(cancellationToken)
{
    readonly List<T> list = [];

    protected override void OnNextCore(T value)
    {
        list.Add(value);
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
        TrySetResult(list);
    }
}
