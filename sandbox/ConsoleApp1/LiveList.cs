using R3;
using System.Collections;
using System.Runtime.InteropServices;

public sealed class LiveList<T> : IReadOnlyList<T>, IDisposable
{
    readonly List<T> list = new List<T>();
    readonly IDisposable sourceSubscription;

    public LiveList(Event<T> source)
    {
        sourceSubscription = source.Subscribe(new ListSubscriber(list));
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
            var span = CollectionsMarshal.AsSpan(list);
            foreach (ref var item in span)
            {
                action(item);
            }
        }
    }

    public void ForEach<TState>(Action<T, TState> action, TState state)
    {
        lock (list)
        {
            var span = CollectionsMarshal.AsSpan(list);
            foreach (ref var item in span)
            {
                action(item, state);
            }
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        lock (list)
        {
            // snapshot
            return CollectionsMarshal.AsSpan(list).ToArray().AsEnumerable().GetEnumerator();
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        lock (list)
        {
            // snapshot
            return CollectionsMarshal.AsSpan(list).ToArray().AsEnumerable().GetEnumerator();
        }
    }

    sealed class ListSubscriber(List<T> list) : Subscriber<T>
    {
        protected override void OnNextCore(T message)
        {
            lock (list)
            {
                list.Add(message);
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            EventSystem.GetUnhandledExceptionHandler().Invoke(error);
        }
    }
}
