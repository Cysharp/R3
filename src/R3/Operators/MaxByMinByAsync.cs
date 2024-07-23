namespace R3;

public static partial class ObservableExtensions
{
    public static Task<T> MaxByAsync<T, TKey>(this Observable<T> source, Func<T, TKey> keySelector, CancellationToken cancellationToken = default)
    {
        return MaxByAsync(source, keySelector, Comparer<TKey>.Default, cancellationToken);
    }

    public static Task<T> MaxByAsync<T, TKey>(this Observable<T> source, Func<T, TKey> keySelector, IComparer<TKey> comparer, CancellationToken cancellationToken = default)
    {
        var method = new MaxByAsync<T, TKey>(keySelector, comparer, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<T> MinByAsync<T, TKey>(this Observable<T> source, Func<T, TKey> keySelector, CancellationToken cancellationToken = default)
    {
        return MinByAsync(source, keySelector, Comparer<TKey>.Default, cancellationToken);
    }

    public static Task<T> MinByAsync<T, TKey>(this Observable<T> source, Func<T, TKey> keySelector, IComparer<TKey> comparer, CancellationToken cancellationToken = default)
    {
        var method = new MinByAsync<T, TKey>(keySelector, comparer, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }
}

// MaxByAsync
internal sealed class MaxByAsync<T, TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer, CancellationToken cancellationToken)
: TaskObserverBase<T, T>(cancellationToken)
{
    T? latestValue;
    TKey? latestKey;
    bool hasValue;

    protected override void OnNextCore(T value)
    {
        if (!hasValue)
        {
            hasValue = true;
            latestValue = value;
            latestKey = keySelector(value);
            return;
        }

        var key = keySelector(value);
        if (comparer.Compare(key, latestKey!) > 0)
        {
            latestValue = value;
            latestKey = key;
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

        if (hasValue)
        {
            TrySetResult(latestValue!);
        }
        else
        {
            TrySetException(new InvalidOperationException("no elements"));
        }
    }
}

// MinByAsync
internal sealed class MinByAsync<T, TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer, CancellationToken cancellationToken)
    : TaskObserverBase<T, T>(cancellationToken)
{
    T? latestValue;
    TKey? latestKey;
    bool hasValue;

    protected override void OnNextCore(T value)
    {
        if (!hasValue)
        {
            hasValue = true;
            latestValue = value;
            latestKey = keySelector(value);
            return;
        }

        var key = keySelector(value);
        if (comparer.Compare(key, latestKey!) < 0)
        {
            latestValue = value;
            latestKey = key;
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

        if (hasValue)
        {
            TrySetResult(latestValue!);
        }
        else
        {
            TrySetException(new InvalidOperationException("no elements"));
        }
    }
}
