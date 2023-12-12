using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace R3
{
    // TODO: ToDictionary
    // TODO: ToLookup

    public static partial class EventExtensions
    {
        public static Task<TMessage[]> ToArrayAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, CancellationToken cancellationToken = default)
        {
            return AggregateAsync(source, new List<TMessage>(), static (list, message) =>
            {
                list.Add(message);
                return list;
            }, (list, _) => list.ToArray(), cancellationToken); // ignore complete
        }

        public static Task<TMessage[]> ToArrayAsync<TMessage, TComplete>(this CompletableEvent<TMessage, Result<TComplete>> source, CancellationToken cancellationToken = default)
        {
            return AggregateAsync(source, new List<TMessage>(), static (list, message) =>
            {
                list.Add(message);
                return list;
            }, (list, _) => list.ToArray(), cancellationToken); // ignore complete
        }

        public static Task<List<TMessage>> ToListAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, CancellationToken cancellationToken = default)
        {
            return AggregateAsync(source, new List<TMessage>(), static (list, message) =>
            {
                list.Add(message);
                return list;
            }, (list, _) => list, cancellationToken); // ignore complete
        }

        public static Task<List<TMessage>> ToListAsync<TMessage, TComplete>(this CompletableEvent<TMessage, Result<TComplete>> source, CancellationToken cancellationToken = default)
        {
            return AggregateAsync(source, new List<TMessage>(), static (list, message) =>
            {
                list.Add(message);
                return list;
            }, (list, _) => list, cancellationToken); // ignore complete
        }

        public static Task<HashSet<TMessage>> ToHashSetAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, CancellationToken cancellationToken = default)
        {
            return ToHashSetAsync(source, null, cancellationToken);
        }

        public static Task<HashSet<TMessage>> ToHashSetAsync<TMessage, TComplete>(this CompletableEvent<TMessage, Result<TComplete>> source, CancellationToken cancellationToken = default)
        {
            return ToHashSetAsync(source, null, cancellationToken);
        }

        public static Task<HashSet<TMessage>> ToHashSetAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, IEqualityComparer<TMessage>? equalityComparer, CancellationToken cancellationToken = default)
        {
            return AggregateAsync(source, new HashSet<TMessage>(equalityComparer), static (value, message) =>
            {
                value.Add(message);
                return value;
            }, (value, _) => value, cancellationToken); // ignore complete
        }

        public static Task<HashSet<TMessage>> ToHashSetAsync<TMessage, TComplete>(this CompletableEvent<TMessage, Result<TComplete>> source, IEqualityComparer<TMessage>? equalityComparer, CancellationToken cancellationToken = default)
        {
            return AggregateAsync(source, new HashSet<TMessage>(equalityComparer), static (value, message) =>
            {
                value.Add(message);
                return value;
            }, (value, _) => value, cancellationToken); // ignore complete
        }

        // CountAsync using AggregateAsync
        public static Task<int> CountAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, CancellationToken cancellationToken = default)
        {
            return AggregateAsync(source, 0, static (count, _) => checked(count + 1), (count, _) => count, cancellationToken); // ignore complete
        }

        // CountAsync Result<TComplete> variation
        public static Task<int> CountAsync<TMessage, TComplete>(this CompletableEvent<TMessage, Result<TComplete>> source, CancellationToken cancellationToken = default)
        {
            return AggregateAsync(source, 0, static (count, _) => checked(count + 1), (count, _) => count, cancellationToken); // ignore complete
        }

        // LongCountAsync using AggregateAsync
        public static Task<long> LongCountAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, CancellationToken cancellationToken = default)
        {
            return AggregateAsync(source, 0L, static (count, _) => checked(count + 1), (count, _) => count, cancellationToken); // ignore complete
        }

        // LongCountAsync using AggregateAsync Result<TComplete> variation
        public static Task<long> LongCountAsync<TMessage, TComplete>(this CompletableEvent<TMessage, Result<TComplete>> source, CancellationToken cancellationToken = default)
        {
            return AggregateAsync(source, 0L, static (count, _) => checked(count + 1), (count, _) => count, cancellationToken); // ignore complete
        }

        public static Task<TMessage> MinAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, CancellationToken cancellationToken = default)
        {
            return AggregateAsync(source, (default(TMessage)!, hasValue: false),
                static (min, message) =>
                {
                    if (!min.hasValue) return (message, true); // first
                    return Comparer<TMessage>.Default.Compare(min.Item1, message) < 0 ? (min.Item1, true) : (message, true);
                },
                static (min, _) =>
                {
                    if (!min.hasValue) throw new InvalidOperationException("Sequence contains no elements");
                    return min.Item1;
                }, cancellationToken);
        }

        public static Task<TMessage> MinAsync<TMessage, TComplete>(this CompletableEvent<TMessage, Result<TComplete>> source, CancellationToken cancellationToken = default)
        {
            return AggregateAsync(source, (default(TMessage)!, hasValue: false),
                static (min, message) =>
                {
                    if (!min.hasValue) return (message, true); // first
                    return Comparer<TMessage>.Default.Compare(min.Item1, message) < 0 ? (min.Item1, true) : (message, true);
                },
                static (min, _) =>
                {
                    if (!min.hasValue) throw new InvalidOperationException("Sequence contains no elements");
                    return min.Item1;
                }, cancellationToken);
        }

        public static Task<TMessage> MaxAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, CancellationToken cancellationToken = default)
        {
            return AggregateAsync(source, (default(TMessage)!, hasValue: false),
                static (max, message) =>
                {
                    if (!max.hasValue) return (message, true); // first
                    return Comparer<TMessage>.Default.Compare(max.Item1, message) > 0 ? (max.Item1, true) : (message, true);
                },
                static (max, _) =>
                {
                    if (!max.hasValue) throw new InvalidOperationException("Sequence contains no elements");
                    return max.Item1;
                }, cancellationToken);
        }

        public static Task<TMessage> MaxAsync<TMessage, TComplete>(this CompletableEvent<TMessage, Result<TComplete>> source, CancellationToken cancellationToken = default)
        {
            return AggregateAsync(source, (default(TMessage)!, hasValue: false),
                static (max, message) =>
                {
                    if (!max.hasValue) return (message, true); // first
                    return Comparer<TMessage>.Default.Compare(max.Item1, message) > 0 ? (max.Item1, true) : (message, true);
                },
                static (max, _) =>
                {
                    if (!max.hasValue) throw new InvalidOperationException("Sequence contains no elements");
                    return max.Item1;
                }, cancellationToken);
        }

        public static Task<(TMessage Min, TMessage Max)> MinMaxAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, CancellationToken cancellationToken = default)
        {
            return AggregateAsync(source,
                (min: default(TMessage)!, max: default(TMessage)!, hasValue: false),
                static (minmax, message) =>
                {
                    if (!minmax.hasValue) return (message, message, true); // first
                    var min = Comparer<TMessage>.Default.Compare(minmax.min, message) < 0 ? minmax.min : message;
                    var max = Comparer<TMessage>.Default.Compare(minmax.max, message) > 0 ? minmax.max : message;
                    return (min, max, true);
                },
                static (minmax, _) =>
                {
                    if (!minmax.hasValue) throw new InvalidOperationException("Sequence contains no elements");
                    return (minmax.min, minmax.max);
                }, cancellationToken);
        }


        public static Task<(TMessage Min, TMessage Max)> MinMaxAsync<TMessage, TComplete>(this CompletableEvent<TMessage, Result<TComplete>> source, CancellationToken cancellationToken = default)
        {
            return AggregateAsync(source,
                (min: default(TMessage)!, max: default(TMessage)!, hasValue: false),
                static (minmax, message) =>
                {
                    if (!minmax.hasValue) return (message, message, true); // first
                    var min = Comparer<TMessage>.Default.Compare(minmax.min, message) < 0 ? minmax.min : message;
                    var max = Comparer<TMessage>.Default.Compare(minmax.max, message) > 0 ? minmax.max : message;
                    return (min, max, true);
                },
                static (minmax, _) =>
                {
                    if (!minmax.hasValue) throw new InvalidOperationException("Sequence contains no elements");
                    return (minmax.min, minmax.max);
                }, cancellationToken);
        }

        public static Task<TMessage> SumAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, CancellationToken cancellationToken = default)
            where TMessage : IAdditionOperators<TMessage, TMessage, TMessage>
        {
            return AggregateAsync(source, default(TMessage)!, static (sum, message) => checked(sum + message), (sum, _) => sum, cancellationToken); // ignore complete
        }

        public static Task<TMessage> SumAsync<TMessage, TComplete>(this CompletableEvent<TMessage, Result<TComplete>> source, CancellationToken cancellationToken = default)
            where TMessage : IAdditionOperators<TMessage, TMessage, TMessage>
        {
            return AggregateAsync(source, default(TMessage)!, static (sum, message) => checked(sum + message), (sum, _) => sum, cancellationToken); // ignore complete
        }

        public static Task<double> AverageAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, CancellationToken cancellationToken = default)
            where TMessage : INumberBase<TMessage>
        {
            return AggregateAsync(source,
                (sum: default(TMessage)!, count: 0, hasValue: false),
                static (avg, message) =>
                {
                    return (checked(avg.sum + message), checked(avg.count + 1), true); // sum, count, hasValue
                },
                static (avg, _) =>
                {
                    if (!avg.hasValue) throw new InvalidOperationException("Sequence contains no elements");
                    return double.CreateChecked(avg.sum) / double.CreateChecked(avg.count);
                },
                cancellationToken);
        }

        public static Task<double> AverageAsync<TMessage, TComplete>(this CompletableEvent<TMessage, Result<TComplete>> source, CancellationToken cancellationToken = default)
            where TMessage : INumberBase<TMessage>
        {
            return AggregateAsync(source,
                (default(TMessage)!, 0),
                static ((TMessage Sum, long Count) avg, TMessage message) => (avg.Sum + message, checked(avg.Count + 1)),
                (avg, _) => double.CreateChecked(avg.Item1) / double.CreateChecked(avg.Item2), // ignore complete
                cancellationToken);
        }

        public static Task WaitAsync<TMessage>(this CompletableEvent<TMessage, Unit> source, CancellationToken cancellationToken = default)
        {
            // get only complete value.
            return AggregateAsync(source, 0, static (_, _) => 0, static (_, result) => result, cancellationToken);
        }

        public static Task<TComplete> WaitAsync<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, CancellationToken cancellationToken = default)
        {
            // get only complete value.
            return AggregateAsync(source, 0, static (_, _) => 0, static (_, result) => result, cancellationToken);
        }
    }
}
