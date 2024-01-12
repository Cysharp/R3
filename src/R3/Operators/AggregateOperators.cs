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

    public static Task<double> AverageAsync<T>(this Observable<T> source, CancellationToken cancellationToken = default)
        where T : INumberBase<T>
    {
        return AggregateAsync(source,
            (sum: default(T)!, count: 0, hasValue: false),
            static (avg, message) =>
            {
                return (checked(avg.sum + message), checked(avg.count + 1), true); // sum, count, hasValue
            },
            static (avg) =>
            {
                if (!avg.hasValue) throw new InvalidOperationException("Sequence contains no elements");
                return double.CreateChecked(avg.sum) / double.CreateChecked(avg.count);
            },
            cancellationToken);
    }

#endif

    public static Task WaitAsync<T>(this Observable<T> source, CancellationToken cancellationToken = default)
    {
        return AggregateAsync(source, 0, static (_, _) => 0, Stubs<int>.ReturnSelf, cancellationToken);
    }
}


