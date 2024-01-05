namespace R3;

public static partial class ObservableExtensions
{
    public static Task<bool> ContainsAsync<T>(this Observable<T> source, T value, CancellationToken cancellationToken = default)
    {
        return ContainsAsync(source, value, EqualityComparer<T>.Default, cancellationToken);
    }

    public static Task<bool> ContainsAsync<T>(this Observable<T> source, T value, IEqualityComparer<T> equalityComparer, CancellationToken cancellationToken = default)
    {
        var observer = new ContainsAsync<T>(value, equalityComparer, cancellationToken);
        source.Subscribe(observer);
        return observer.Task;
    }
}

internal sealed class ContainsAsync<T>(T compareValue, IEqualityComparer<T> equalityComparer, CancellationToken cancellationToken)
: TaskObserverBase<T, bool>(cancellationToken)
{
    protected override void OnNextCore(T value)
    {
        if (equalityComparer.Equals(value, compareValue))
        {
            TrySetResult(true);
        }
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
        TrySetResult(false);
    }
}

