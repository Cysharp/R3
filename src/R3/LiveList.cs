using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace R3;

public static partial class EventExtensions
{
    public static LiveList<TMessage> ToLiveList<TMessage>(this Event<TMessage> source)
    {
        return new LiveList<TMessage>(source);
    }

    public static LiveList<TMessage> ToLiveList<TMessage>(this Event<TMessage> source, int bufferSize)
    {
        return new LiveList<TMessage>(source, bufferSize);
    }

    public static LiveList<TMessage, TComplete> ToLiveList<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source)
    {
        return new LiveList<TMessage, TComplete>(source);
    }

    public static LiveList<TMessage, TComplete> ToLiveList<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, int bufferSize)
    {
        return new LiveList<TMessage, TComplete>(source, bufferSize);
    }
}

public sealed class LiveList<T> : IReadOnlyList<T>, IDisposable
{
    readonly RingBuffer<T> list = new RingBuffer<T>();
    readonly IDisposable sourceSubscription;
    readonly int bufferSize;

    public LiveList(Event<T> source)
        : this(source, -1)
    {
    }

    public LiveList(Event<T> source, int bufferSize)
    {
        if (bufferSize == 0) bufferSize = 1;
        this.bufferSize = bufferSize; // bufferSize must set before Subscribe(sometimes Subscribe run immediately)
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

    sealed class ListSubscriber(LiveList<T> parent) : Subscriber<T>
    {
        protected override void OnNextCore(T message)
        {
            lock (parent.list)
            {
                if (parent.list.Count == parent.bufferSize)
                {
                    parent.list.RemoveFirst();
                }
                parent.list.AddLast(message);
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            EventSystem.GetUnhandledExceptionHandler().Invoke(error);
        }
    }
}

public sealed class LiveList<T, TComplete> : IReadOnlyList<T>, IDisposable
{
    readonly RingBuffer<T> list = new RingBuffer<T>();
    readonly IDisposable sourceSubscription;
    readonly int bufferSize;

    bool isCompleted;
    TComplete? completedValue;

    [MemberNotNullWhen(true, nameof(CompletedValue))]
    public bool IsCompleted => isCompleted;

    public TComplete? CompletedValue => completedValue;

    public LiveList(CompletableEvent<T, TComplete> source)
        : this(source, -1)
    {
    }

    public LiveList(CompletableEvent<T, TComplete> source, int bufferSize)
    {
        if (bufferSize == 0) bufferSize = 1;
        this.bufferSize = bufferSize; // bufferSize must set before Subscribe(sometimes Subscribe run immediately)
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
                if (parent.list.Count == parent.bufferSize)
                {
                    parent.list.RemoveFirst();

                }
                parent.list.AddLast(message);
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
