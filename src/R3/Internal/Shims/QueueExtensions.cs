#if NETSTANDARD2_0

namespace R3.Internal;

internal static class QueueExtensions
{
    internal static bool TryDequeue<T>(this Queue<T> queue, out T value)
    {
        if (queue.Count == 0)
        {
            value = default!;
            return false;
        }
        else
        {
            value = queue.Dequeue();
            return true;
        }
    }

    internal static bool TryPeek<T>(this Queue<T> queue, out T value)
    {
        if (queue.Count == 0)
        {
            value = default!;
            return false;
        }
        else
        {
            value = queue.Peek();
            return true;
        }
    }
}

#endif
