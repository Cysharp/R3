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

    }


}


