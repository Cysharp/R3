using System.Buffers;
using System.Collections;
using System.Runtime.InteropServices;

namespace R3;

public sealed class CompositeDisposable : ICollection<IDisposable>, IDisposable
{
    List<IDisposable?> list; // when removed, set null
    readonly object gate = new object();
    bool isDisposed;
    int count;

    const int ShrinkThreshold = 64;

    public bool IsDisposed => Volatile.Read(ref isDisposed);

    public CompositeDisposable()
    {
        this.list = new();
    }

    public CompositeDisposable(int capacity)
    {
        if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        this.list = new(capacity);
    }

    public CompositeDisposable(params IDisposable[] disposables)
    {
        this.list = new(disposables);
        this.count = list.Count;
    }

    public CompositeDisposable(IEnumerable<IDisposable> disposables)
    {
        this.list = new(disposables);
        this.count = list.Count;
    }

    public int Count
    {
        get
        {
            lock (gate)
            {
                return count;
            }
        }
    }

    public bool IsReadOnly => false;

    public void Add(IDisposable item)
    {
        lock (gate)
        {
            if (!isDisposed)
            {
                count += 1;
                list.Add(item);
                return;
            }
        }

        // CompositeDisposable is Disposed.
        item.Dispose();
    }

    public bool Remove(IDisposable item)
    {
        lock (gate)
        {
            // CompositeDisposable is Disposed, do nothing.
            if (isDisposed) return false;

            var current = list;

            var index = current.IndexOf(item);
            if (index == -1)
            {
                // not found
                return false;
            }

            // don't do RemoveAt(avoid Array Copy)
            current[index] = null;

            // Do shrink
            if (current.Capacity > ShrinkThreshold && count < current.Capacity / 2)
            {
                var fresh = new List<IDisposable?>(current.Capacity / 2);

                foreach (var d in current)
                {
                    if (d != null)
                    {
                        fresh.Add(d);
                    }
                }

                list = fresh;
            }

            count -= 1;
        }

        // Dispose outside of lock
        item.Dispose();
        return true;
    }

    public void Clear()
    {
        IDisposable?[] targetDisposables;
        int clearCount;
        lock (gate)
        {
            // CompositeDisposable is Disposed, do nothing.
            if (isDisposed) return;
            if (count == 0) return;

            targetDisposables = ArrayPool<IDisposable?>.Shared.Rent(list.Count);
            clearCount = list.Count;

            list.CopyTo(targetDisposables);

            list.Clear();
            count = 0;
        }

        // Dispose outside of lock
        try
        {
            foreach (var item in targetDisposables.AsSpan(0, clearCount))
            {
                item?.Dispose();
            }
        }
        finally
        {
            ArrayPool<IDisposable?>.Shared.Return(targetDisposables, clearArray: true);
        }
    }

    public bool Contains(IDisposable item)
    {
        lock (gate)
        {
            if (isDisposed) return false;
            return list.Contains(item);
        }
    }

    public void CopyTo(IDisposable[] array, int arrayIndex)
    {
        if (arrayIndex < 0 || arrayIndex >= array.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        }

        lock (gate)
        {
            if (isDisposed) return;

            if (arrayIndex + count > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            var i = 0;
            foreach (var item in CollectionsMarshal.AsSpan(list))
            {
                if (item != null)
                {
                    array[arrayIndex + i++] = item;
                }
            }
        }
    }

    public void Dispose()
    {
        List<IDisposable?> disposables;

        lock (gate)
        {
            if (isDisposed) return;

            count = 0;
            isDisposed = true;
            disposables = list;
            list = null!; // dereference.
        }

        foreach (var item in disposables)
        {
            item?.Dispose();
        }
        disposables.Clear();
    }

    public IEnumerator<IDisposable> GetEnumerator()
    {
        lock (gate)
        {
            // make snapshot
            return EnumerateAndClear(list.ToArray()).GetEnumerator();
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        lock (gate)
        {
            // make snapshot
            return EnumerateAndClear(list.ToArray()).GetEnumerator();
        }
    }

    static IEnumerable<IDisposable> EnumerateAndClear(IDisposable?[] disposables)
    {
        try
        {
            foreach (var item in disposables)
            {
                if (item != null)
                {
                    yield return item;
                }
            }
        }
        finally
        {
            disposables.AsSpan().Clear();
        }
    }
}
