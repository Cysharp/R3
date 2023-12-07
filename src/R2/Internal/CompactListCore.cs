using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace R2.Internal;

[StructLayout(LayoutKind.Auto)]
internal struct CompactListCore<T>
    where T : class
{
    const int InitialArraySize = 4;

    readonly object gate;
    T?[]? values = null;
    int lastIndex;

    public CompactListCore(object gate)
    {
        this.gate = gate;
    }

    public bool IsDisposed => lastIndex == -1;

    public ReadOnlySpan<T?> AsSpan()
    {
        var last = Volatile.Read(ref lastIndex);
        var xs = Volatile.Read(ref values);
        return xs.AsSpan(0, last + 1);
    }

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
                var newValues = new T[len + (len / 2)];
                Array.Copy(values, newValues, len);
                Volatile.Write(ref values, newValues);
                index = len;
            }

            values[index] = item;
            if (lastIndex < index)
            {
                Volatile.Write(ref lastIndex, index);
            }

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

                        if (index == lastIndex)
                        {
                            Volatile.Write(ref lastIndex, FindLastNonNullIndex(values, index));
                        }
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

                    if (i == lastIndex)
                    {
                        Volatile.Write(ref lastIndex, FindLastNonNullIndex(values, i));
                    }
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
            lastIndex = -1;
        }
    }

    static int FindNullIndex(T?[] target)
    {
        var span = MemoryMarshal.CreateReadOnlySpan(
            ref Unsafe.As<T?, IntPtr>(ref MemoryMarshal.GetArrayDataReference(target)), target.Length);
        return span.IndexOf(IntPtr.Zero);
    }

    static int FindLastNonNullIndex(T?[] target, int lastIndex)
    {
        var span = MemoryMarshal.CreateReadOnlySpan(
            ref Unsafe.As<T?, IntPtr>(ref MemoryMarshal.GetArrayDataReference(target)), lastIndex); // without lastIndexed value.
        var index = span.LastIndexOfAnyExcept(IntPtr.Zero);
        return (index == -1) ? 0 : index;
    }
}
