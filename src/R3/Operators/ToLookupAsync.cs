using System.Collections;

namespace R3;

public static partial class ObservableExtensions
{
    public static Task<ILookup<TKey, T>> ToLookupAsync<T, TKey>(this Observable<T> source, Func<T, TKey> keySelector, CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        return ToLookupAsync(source, keySelector, null, cancellationToken);
    }

    public static Task<ILookup<TKey, T>> ToLookupAsync<T, TKey>(this Observable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey>? keyComparer, CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        var method = new ToLookupAsync<T, TKey>(keySelector, keyComparer, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<ILookup<TKey, TElement>> ToLookupAsync<T, TKey, TElement>(this Observable<T> source, Func<T, TKey> keySelector, Func<T, TElement> elementSelector, CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        return ToLookupAsync(source, keySelector, elementSelector, null, cancellationToken);
    }

    public static Task<ILookup<TKey, TElement>> ToLookupAsync<T, TKey, TElement>(this Observable<T> source, Func<T, TKey> keySelector, Func<T, TElement> elementSelector, IEqualityComparer<TKey>? keyComparer, CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        var method = new ToLookupAsync<T, TKey, TElement>(keySelector, elementSelector, keyComparer, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }
}

// ToLookupAsync
internal sealed class ToLookupAsync<T, TKey>(Func<T, TKey> keySelector, IEqualityComparer<TKey>? keyComparer, CancellationToken cancellationToken)
    : TaskObserverBase<T, ILookup<TKey, T>>(cancellationToken)
    where TKey : notnull
{
    readonly Dictionary<TKey, List<T>> dictionary = new(keyComparer);

    protected override void OnNextCore(T value)
    {
        var key = keySelector(value);
        if (!dictionary.TryGetValue(key, out var list))
        {
            list = new List<T>();
            dictionary.Add(key, list);
        }
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

        TrySetResult(new Lookup<TKey, T>(dictionary));
    }
}


// ToLookpAsync with elementSelector
internal sealed class ToLookupAsync<T, TKey, TElement>(Func<T, TKey> keySelector, Func<T, TElement> elementSelector, IEqualityComparer<TKey>? keyComparer, CancellationToken cancellationToken)
    : TaskObserverBase<T, ILookup<TKey, TElement>>(cancellationToken)
    where TKey : notnull
{
    readonly Dictionary<TKey, List<TElement>> dictionary = new(keyComparer);

    protected override void OnNextCore(T value)
    {
        var key = keySelector(value);
        var element = elementSelector(value);
        if (!dictionary.TryGetValue(key, out var list))
        {
            list = new List<TElement>();
            dictionary.Add(key, list);
        }
        list.Add(element);
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

        TrySetResult(new Lookup<TKey, TElement>(dictionary));
    }
}


internal sealed class Lookup<TKey, TElement>(Dictionary<TKey, List<TElement>> dictionary) : ILookup<TKey, TElement>
    where TKey : notnull
{
    public IEnumerable<TElement> this[TKey key]
    {
        get
        {
            if (dictionary.TryGetValue(key, out var list))
            {
                return list;
            }
            return Enumerable.Empty<TElement>();
        }
    }

    public int Count => dictionary.Count;

    public bool Contains(TKey key)
    {
        return dictionary.ContainsKey(key);
    }

    public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
    {
        foreach (var item in dictionary)
        {
            yield return new Grouping(item);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    sealed class Grouping(KeyValuePair<TKey, List<TElement>> kvp) : IGrouping<TKey, TElement>
    {
        public TKey Key => kvp.Key;

        public IEnumerator<TElement> GetEnumerator()
        {
            return kvp.Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
