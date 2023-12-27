using System.Collections.Concurrent;

namespace R3.Internal;

// TODO: remove this(maybe no use).
internal sealed class PooledThreadPoolWorkItem<T> : IThreadPoolWorkItem
{
    static ConcurrentQueue<PooledThreadPoolWorkItem<T>> pool = new();

    T state = default!;
    Action<T> action = default!;

    PooledThreadPoolWorkItem()
    {
    }

    public static IThreadPoolWorkItem Create(Action<T> action, T state)
    {
        if (!pool.TryDequeue(out var item))
        {
            item = new PooledThreadPoolWorkItem<T>();
        }

        item.state = state;
        item.action = action;
        return item;
    }

    public void Execute()
    {
        try
        {
            action(state);
        }
        finally
        {
            state = default!;
            action = default!;
            pool.Enqueue(this);
        }
    }
}
