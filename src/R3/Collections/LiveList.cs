using System.Collections;

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
        readonly RingBuffer<T> list; // lock object
        readonly IDisposable sourceSubscription;
        readonly int bufferSize; // meaningful only when isUnbounded is false
        readonly bool isUnbounded;

        bool isCompleted;
        Result completedValue;

        public bool IsCompleted => isCompleted;

        public Result Result
        {
            get
            {
                lock (list)
                {
                    if (!isCompleted)
                    {
                        throw new InvalidOperationException("LiveList is not completed, you should check IsCompleted.");
                    }

                    return completedValue;
                }
            }
        }

        public LiveList(Observable<T> source)
        {
            this.isUnbounded = true; // isUnbounded must set before Subscribe(sometimes Subscribe run immediately)
            this.list = new RingBuffer<T>();
            this.sourceSubscription = source.Subscribe(new ListObserver(this));
        }

        public LiveList(Observable<T> source, int bufferSize)
        {
            if (bufferSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize), bufferSize, "bufferSize must be non-negative. Use the constructor without bufferSize to create a LiveList that has no size limit.");
            }

            if (bufferSize == 0)
            {
                bufferSize = 1;
            }

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
                    // RingBuffer<T> indexer does not validate the index, it masks the index with the internal
                    // buffer capacity. An out-of-range index would silently return default or a stale value.
                    if ((uint)index >= (uint)list.Count)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index), index, $"index must be non-negative and less than Count ({list.Count}). Count changes while the source is being observed.");
                    }

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
                    var ring = parent.list;

                    // when the size is limited, drop the oldest value to make room.
                    // otherwise RingBuffer<T> grows automatically.
                    if (!parent.isUnbounded && ring.Count == parent.bufferSize)
                    {
                        ring.RemoveFirst();
                    }

                    ring.AddLast(message);
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
}
