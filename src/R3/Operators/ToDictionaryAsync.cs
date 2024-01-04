namespace R3;

public static partial class ObservableExtensions
{
    public static Task<Dictionary<TKey, T>> ToDictionaryAsync<T, TKey>(this Observable<T> source, Func<T, TKey> keySelector, CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        return ToDictionaryAsync(source, keySelector, null, cancellationToken);
    }

    public static Task<Dictionary<TKey, T>> ToDictionaryAsync<T, TKey>(this Observable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey>? keyComparer, CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        var method = new ToDictionaryAsync<T, TKey>(keySelector, keyComparer, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<T, TKey, TElement>(this Observable<T> source, Func<T, TKey> keySelector, Func<T, TElement> elementSelector, CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        return ToDictionaryAsync(source, keySelector, elementSelector, null, cancellationToken);
    }

    public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<T, TKey, TElement>(this Observable<T> source, Func<T, TKey> keySelector, Func<T, TElement> elementSelector, IEqualityComparer<TKey>? keyComparer, CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        var method = new ToDictionaryAsync<T, TKey, TElement>(keySelector, elementSelector, keyComparer, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }
}

// ToDictionaryAsync
internal sealed class ToDictionaryAsync<T, TKey>(Func<T, TKey> keySelector, IEqualityComparer<TKey>? keyComparer, CancellationToken cancellationToken)
    : TaskObserverBase<T, Dictionary<TKey, T>>(cancellationToken)
    where TKey : notnull
{
    readonly Dictionary<TKey, T> dictionary = new(keyComparer);

    protected override void OnNextCore(T value)
    {
        var key = keySelector(value);
        dictionary.Add(key, value);
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

internal sealed class ToDictionaryAsync<T, TKey, TElement>(Func<T, TKey> keySelector, Func<T, TElement> elementSelector, IEqualityComparer<TKey>? keyComparer, CancellationToken cancellationToken)
    : TaskObserverBase<T, Dictionary<TKey, TElement>>(cancellationToken)
    where TKey : notnull
{
    readonly Dictionary<TKey, TElement> dictionary = new(keyComparer);

    protected override void OnNextCore(T value)
    {
        var key = keySelector(value);
        var element = elementSelector(value);
        dictionary.Add(key, element);
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
