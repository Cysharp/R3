using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace R2.Internal;

[StructLayout(LayoutKind.Auto)]
internal struct CompactListCore<T>
    where T : class
{
    const double ShrinkRate = 0.8;
    const int InitialArraySize = 4;
    const int MinShrinkStartSize = 16;

    readonly object gate;
    T?[]? values = null;
    int count;

    public CompactListCore(object gate)
    {
        this.gate = gate;
    }

    public int Count => count;
    public bool IsDisposed => count == -1;

    public ReadOnlySpan<T?> AsSpan() => Volatile.Read(ref values); // thread safe in iterate

    public int Add(T item)
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, typeof(CompactListCore<T>));

            if (values == null)
            {
                values = new T[InitialArraySize];
            }

            // try find blank
            var index = FindNullIndex(values);
            if (index == -1)
            {
                // full, resize(x1.5)
                var len = values.Length;
                Array.Resize(ref values, len + (len / 2));
                index = len;
            }

            values[index] = item;
            count++;

            return index; // index is remove key.
        }
    }

    // first, try to find index of item.
    public void Remove(int index, T item)
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, typeof(CompactListCore<T>));

            if (values == null) return;

            if (index < values.Length)
            {
                ref var v = ref values[index];
                if (v != null)
                {
                    if (v == item)
                    {
                        v = null;
                        count--;
                        TryShrink();
                        return;
                    }
                }
            }

            // fallback, when shrinked, index is broken.
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == item)
                {
                    values[i] = null;
                    count--;

                    // when removed, do shrink.
                    TryShrink();
                    return;
                }
            }

        }
    }

    public void Dispose()
    {
        lock (gate)
        {
            values = null;
            count = -1;
        }
    }

    // this method must be called in lock
    void TryShrink()
    {
        if (values == null) return;
        if (values.Length < MinShrinkStartSize) return;

        var rate = (double)(values.Length - count) / values.Length;
        if (rate > ShrinkRate)
        {
            var size = count + (count / 2); // * 1.5
            if (size < 16) size = 16;

            var newArray = new T[size];
            var i = 0;

            // TODO:if can use same index, keep index.
            foreach (var item in values)
            {
                if (item != null)
                {
                    newArray[i++] = item;
                }
            }

            // set new.
            this.values = newArray;
        }
    }

    static int FindNullIndex(T?[] target)
    {
        var span = MemoryMarshal.CreateReadOnlySpan(
            ref Unsafe.As<T?, IntPtr>(ref MemoryMarshal.GetArrayDataReference(target)), target.Length);
        return span.IndexOf(IntPtr.Zero);
    }
}
