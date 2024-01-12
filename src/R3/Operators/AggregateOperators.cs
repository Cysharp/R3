using System.Numerics;

namespace R3;

// TODO: Selector APIs

public static partial class ObservableExtensions
{
    public static Task<T[]> ToArrayAsync<T>(this Observable<T> source, CancellationToken cancellationToken = default)
    {
        return AggregateAsync(source, new List<T>(), static (list, message) =>
        {
            list.Add(message);
            return list;
        }, (list) => list.ToArray(), cancellationToken); // ignore complete
    }

    public static Task<List<T>> ToListAsync<T>(this Observable<T> source, CancellationToken cancellationToken = default)
    {
        return AggregateAsync(source, new List<T>(), static (list, message) =>
        {
            list.Add(message);
            return list;
        }, (list) => list, cancellationToken); // ignore complete
    }

    public static Task<HashSet<T>> ToHashSetAsync<T>(this Observable<T> source, CancellationToken cancellationToken = default)
    {
        return ToHashSetAsync(source, null, cancellationToken);
    }


    public static Task<HashSet<T>> ToHashSetAsync<T>(this Observable<T> source, IEqualityComparer<T>? equalityComparer, CancellationToken cancellationToken = default)
    {
        return AggregateAsync(source, new HashSet<T>(equalityComparer), static (value, message) =>
        {
            value.Add(message);
            return value;
        }, (value) => value, cancellationToken); // ignore complete
    }

    // CountAsync using AggregateAsync
    public static Task<int> CountAsync<T>(this Observable<T> source, CancellationToken cancellationToken = default)
    {
        return AggregateAsync(source, 0, static (count, _) => checked(count + 1), Stubs<int>.ReturnSelf, cancellationToken); // ignore complete
    }

    // LongCountAsync using AggregateAsync
    public static Task<long> LongCountAsync<T>(this Observable<T> source, CancellationToken cancellationToken = default)
    {
        return AggregateAsync(source, 0L, static (count, _) => checked(count + 1), Stubs<long>.ReturnSelf, cancellationToken); // ignore complete
    }

    public static Task<T> MinAsync<T>(this Observable<T> source, CancellationToken cancellationToken = default)
    {
        return AggregateAsync(source, (default(T)!, hasValue: false),
            static (min, message) =>
            {
                if (!min.hasValue) return (message, true); // first
                return Comparer<T>.Default.Compare(min.Item1, message) < 0 ? (min.Item1, true) : (message, true);
            },
            static (min) =>
            {
                if (!min.hasValue) throw new InvalidOperationException("Sequence contains no elements");
                return min.Item1;
            }, cancellationToken);
    }


    public static Task<T> MaxAsync<T>(this Observable<T> source, CancellationToken cancellationToken = default)
    {
        return AggregateAsync(source, (default(T)!, hasValue: false),
            static (max, message) =>
            {
                if (!max.hasValue) return (message, true); // first
                return Comparer<T>.Default.Compare(max.Item1, message) > 0 ? (max.Item1, true) : (message, true);
            },
            static (max) =>
            {
                if (!max.hasValue) throw new InvalidOperationException("Sequence contains no elements");
                return max.Item1;
            }, cancellationToken);
    }


    public static Task<(T Min, T Max)> MinMaxAsync<T>(this Observable<T> source, CancellationToken cancellationToken = default)
    {
        return AggregateAsync(source,
            (min: default(T)!, max: default(T)!, hasValue: false),
            static (minmax, message) =>
            {
                if (!minmax.hasValue) return (message, message, true); // first
                var min = Comparer<T>.Default.Compare(minmax.min, message) < 0 ? minmax.min : message;
                var max = Comparer<T>.Default.Compare(minmax.max, message) > 0 ? minmax.max : message;
                return (min, max, true);
            },
            static (minmax) =>
            {
                if (!minmax.hasValue) throw new InvalidOperationException("Sequence contains no elements");
                return (minmax.min, minmax.max);
            }, cancellationToken);
    }

#if NET8_0_OR_GREATER

    public static Task<T> SumAsync<T>(this Observable<T> source, CancellationToken cancellationToken = default)
        where T : IAdditionOperators<T, T, T>
    {
        return AggregateAsync(source, default(T)!, static (sum, message) => checked(sum + message), Stubs<T>.ReturnSelf, cancellationToken); // ignore complete
    }

#endif

    public static Task WaitAsync<T>(this Observable<T> source, CancellationToken cancellationToken = default)
    {
        return AggregateAsync(source, 0, static (_, _) => 0, Stubs<int>.ReturnSelf, cancellationToken);
    }
}
