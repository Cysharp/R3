namespace R3;

public static partial class ObservableExtensions
{
    public static Task<HashSet<T>> ToHashSetAsync<T>(this Observable<T> source, CancellationToken cancellationToken = default)
    {
        return ToHashSetAsync(source, null, cancellationToken);
    }

    public static Task<HashSet<T>> ToHashSetAsync<T>(this Observable<T> source, IEqualityComparer<T>? comparer, CancellationToken cancellationToken = default)
    {
        var method = new ToHashSetAsync<T>(comparer, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }
}

internal sealed class ToHashSetAsync<T>(IEqualityComparer<T>? comparer, CancellationToken cancellationToken) : TaskObserverBase<T, HashSet<T>>(cancellationToken)
{
    readonly HashSet<T> hashSet = new(comparer);

    protected override void OnNextCore(T value)
    {
        hashSet.Add(value);
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
        TrySetResult(hashSet);
    }
}
