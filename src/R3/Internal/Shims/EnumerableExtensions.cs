#if NETSTANDARD2_0 || NETSTANDARD2_1

namespace R3.Internal;

internal static class EnumerableExtensions
{
    internal static bool TryGetNonEnumeratedCount<T>(this IEnumerable<T> source, out int count)
    {
        if (source is ICollection<T> collection)
        {
            count = collection.Count;
            return true;
        }
        if (source is IReadOnlyCollection<T> readOnlyCollection)
        {
            count = readOnlyCollection.Count;
            return true;
        }
        count = 0;
        return false;
    }
}

#endif
