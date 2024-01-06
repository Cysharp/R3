using System.Collections;
using System.Runtime.InteropServices;

namespace R3
{
    public static partial class ObservableExtensions
    {
        public static LiveList<T> ToLiveList<T>(this Observable<T> source)
        {
            return new LiveList<T>(source);
        }

        public static LiveList<T> ToLiveList<T>(this Observable<T> source, int bufferSize)
        {
            return new LiveList<T>(source, bufferSize);
        }
    }
}

namespace R3.Collections
{
    public sealed class LiveList<T> : IReadOnlyList<T>, IDisposable
    {
        readonly IReadOnlyList<T> list; // RingBuffer<T> or List<T>
        readonly IDisposable sourceSubscription;
        readonly int bufferSize;

        bool isCompleted;
        Result completedValue;

        public bool IsCompleted => isCompleted;

        public Result Result
        {
            get
            {
                lock (list)
                {
                    if (!isCompleted) throw new InvalidOperationException("LiveList is not completed, you should check IsCompleted.");
                    return completedValue;
                }
            }
        }

        public LiveList(Observable<T> source)
        {
            if (bufferSize == 0) bufferSize = 1;
            this.bufferSize = -1;
            this.list = new List<T>();
            this.sourceSubscription = source.Subscribe(new ListObserver(this));
        }

        public LiveList(Observable<T> source, int bufferSize)
        {
            if (bufferSize == 0) bufferSize = 1;
            this.bufferSize = bufferSize; // bufferSize must set before Subscribe(sometimes Subscribe run immediately)
            this.list = new RingBuffer<T>(bufferSize);
            this.sourceSubscription = source.Subscribe(new ListObserver(this));
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

        public void ForEach<TState>(TState state, Action<T, TState> action)
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

        sealed class ListObserver(LiveList<T> parent) : Observer<T>
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
                ObservableSystem.GetUnhandledExceptionHandler().Invoke(error);
            }

            protected override void OnCompletedCore(Result complete)
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
}
