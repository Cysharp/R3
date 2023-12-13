using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace R3;

public static partial class EventExtensions
{
    public static LiveList<TMessage, TComplete> ToLiveList<TMessage, TComplete>(this Event<TMessage, TComplete> source)
    {
        return new LiveList<TMessage, TComplete>(source);
    }

    public static LiveList<TMessage, TComplete> ToLiveList<TMessage, TComplete>(this Event<TMessage, TComplete> source, int bufferSize)
    {
        return new LiveList<TMessage, TComplete>(source, bufferSize);
    }
}

public sealed class LiveList<T, TComplete> : IReadOnlyList<T>, IDisposable
{
    readonly IReadOnlyList<T> list; // RingBuffer<T> or List<T>
    readonly IDisposable sourceSubscription;
    readonly int bufferSize;

    bool isCompleted;
    TComplete? completedValue;

    [MemberNotNullWhen(true, nameof(CompletedValue))]
    public bool IsCompleted => isCompleted;

    public TComplete? CompletedValue => completedValue;

    public LiveList(Event<T, TComplete> source)
    {
        if (bufferSize == 0) bufferSize = 1;
        this.bufferSize = -1;
        this.list = new List<T>();
        this.sourceSubscription = source.Subscribe(new ListSubscriber(this));
    }

    public LiveList(Event<T, TComplete> source, int bufferSize)
    {
        if (bufferSize == 0) bufferSize = 1;
        this.bufferSize = bufferSize; // bufferSize must set before Subscribe(sometimes Subscribe run immediately)
        this.list = new RingBuffer<T>(bufferSize);
        this.sourceSubscription = source.Subscribe(new ListSubscriber(this));
    }

    public T this[int index]
    {
        get
        {
            lock (list)
            {
                return list[index];
            }
        }
    }

    public int Count
    {
        get
        {
            lock (list)
            {
                return list.Count;
            }
        }
    }

    public void Clear()
    {
        lock (list)
        {
            list.Clear();
        }
    }

    public void Dispose()
    {
        sourceSubscription.Dispose();
    }

    public void ForEach(Action<T> action)
    {
        lock (list)
        {
            var span = list.GetSpan();
            foreach (ref readonly var item in span)
            {
                action(item);
            }
        }
    }

    public void ForEach<TState>(Action<T, TState> action, TState state)
    {
        lock (list)
        {
            var span = list.GetSpan();
            foreach (ref readonly var item in span)
            {
                action(item, state);
            }
        }
    }

    public T[] ToArray()
    {
        lock (list)
        {
            return list.ToArray();
        }
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        lock (list)
        {
            // snapshot
            return ToArray().AsEnumerable().GetEnumerator();
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        lock (list)
        {
            // snapshot
            return ToArray().AsEnumerable().GetEnumerator();
        }
    }

    sealed class ListSubscriber(LiveList<T, TComplete> parent) : Subscriber<T, TComplete>
    {
        protected override void OnNextCore(T message)
        {
            lock (parent.list)
            {
                if (parent.bufferSize == -1)
                {
                    ((List<T>)parent.list).Add(message);
                }
                else
                {
                    var ring = (RingBuffer<T>)parent.list;

                    if (ring.Count == parent.bufferSize)
                    {
                        ring.RemoveFirst();
                    }
                    ring.AddLast(message);
                }
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            EventSystem.GetUnhandledExceptionHandler().Invoke(error);
        }

        protected override void OnCompletedCore(TComplete complete)
        {
            lock (parent.list)
            {
                parent.completedValue = complete;
                parent.isCompleted = true;
            }
        }
    }
}

file static class RingBufferOrListExtensions
{
    public static RingBufferSpan<T> GetSpan<T>(this IReadOnlyList<T> list)
    {
        if (list is RingBuffer<T> r)
        {
            return r.GetSpan();
        }
        else if (list is List<T> l)
        {
            var span1 = CollectionsMarshal.AsSpan(l);
            return new RingBufferSpan<T>(span1, default, span1.Length);
        }
        else
        {
            throw new NotSupportedException();
        }
    }

    public static void Clear<T>(this IReadOnlyList<T> list)
    {
        if (list is RingBuffer<T> r)
        {
            r.Clear();
        }
        else if (list is List<T> l)
        {
            l.Clear();
        }
        else
        {
            throw new NotSupportedException();
        }
    }

    public static T[] ToArray<T>(this IReadOnlyList<T> list)
    {
        if (list is RingBuffer<T> r)
        {
            return r.ToArray();
        }
        else if (list is List<T> l)
        {
            return CollectionsMarshal.AsSpan(l).ToArray();
        }
        else
        {
            throw new NotSupportedException();
        }
    }
}
