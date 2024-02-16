namespace R3;

public static partial class ObservableExtensions
{
    public static Task<IEnumerable<KeyValuePair<TKey, TAccumulate>>> AggregateByAsync<TSource, TKey, TAccumulate>(
        this Observable<TSource> source,
        Func<TSource, TKey> keySelector,
        TAccumulate seed,
        Func<TAccumulate, TSource, TAccumulate> func,
        IEqualityComparer<TKey>? keyComparer = null,
        CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        var observer = new AggregateByAsync<TSource, TKey, TAccumulate>(keySelector, seed, func, keyComparer, cancellationToken);
        source.Subscribe(observer);
        return observer.Task;
    }

    public static Task<IEnumerable<KeyValuePair<TKey, TAccumulate>>> AggregateByAsync<TSource, TKey, TAccumulate>(
        this Observable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TKey, TAccumulate> seedSelector,
        Func<TAccumulate, TSource, TAccumulate> func,
        IEqualityComparer<TKey>? keyComparer = null,
        CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        var observer = new AggregateByAsyncSeedSelector<TSource, TKey, TAccumulate>(keySelector, seedSelector, func, keyComparer, cancellationToken);
        source.Subscribe(observer);
        return observer.Task;
    }
}

internal sealed class AggregateByAsync<TSource, TKey, TAccumulate>(
    Func<TSource, TKey> keySelector,
    TAccumulate seed,
    Func<TAccumulate, TSource, TAccumulate> func,
    IEqualityComparer<TKey>? keyComparer,
    CancellationToken cancellationToken)
    : TaskObserverBase<TSource, IEnumerable<KeyValuePair<TKey, TAccumulate>>>(cancellationToken)
    where TKey : notnull
{
    readonly Dictionary<TKey, TAccumulate> dictionary = new(keyComparer);

    protected override void OnNextCore(TSource value)
    {
        var key = keySelector(value);
        if (!dictionary.TryGetValue(key, out var currentAccumulate))
        {
            currentAccumulate = seed;
        }
        dictionary[key] = func(currentAccumulate, value);
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
        TrySetResult(dictionary);
    }
}

internal sealed class AggregateByAsyncSeedSelector<TSource, TKey, TAccumulate>(
    Func<TSource, TKey> keySelector,
    Func<TKey, TAccumulate> seedSelector,
    Func<TAccumulate, TSource, TAccumulate> func,
    IEqualityComparer<TKey>? keyComparer,
    CancellationToken cancellationToken)
    : TaskObserverBase<TSource, IEnumerable<KeyValuePair<TKey, TAccumulate>>>(cancellationToken)
    where TKey : notnull
{
    readonly Dictionary<TKey, TAccumulate> dictionary = new(keyComparer);

    protected override void OnNextCore(TSource value)
    {
        var key = keySelector(value);
        if (!dictionary.TryGetValue(key, out var currentAccumulate))
        {
            currentAccumulate = seedSelector(key);
        }
        dictionary[key] = func(currentAccumulate, value);
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
        TrySetResult(dictionary);
    }
}

