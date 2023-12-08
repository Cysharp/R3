using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace R2.Internal;

[StructLayout(LayoutKind.Auto)]
internal struct FreeListCore<T>
    where T : class
{
    const int InitialArraySize = 4;

    readonly object gate;
    T?[]? values = null;
    int lastIndex;

    public FreeListCore(object gate)
    {
        this.gate = gate;
    }

    public bool IsDisposed => lastIndex == -2;

    public ReadOnlySpan<T?> AsSpan()
    {
        var last = Volatile.Read(ref lastIndex);
        var xs = Volatile.Read(ref values);
        if (xs == null) return ReadOnlySpan<T?>.Empty;
        return xs.AsSpan(0, last + 1);
    }

    public int Add(T item)
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, typeof(FreeListCore<T>));

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

    public void Remove(int index)
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, typeof(FreeListCore<T>));

            if (values == null) return;

            if (index < values.Length)
            {
                ref var v = ref values[index];
                if (v == null) throw new KeyNotFoundException($"key index {index} is not found.");

                v = null;
                if (index == lastIndex)
                {
                    Volatile.Write(ref lastIndex, FindLastNonNullIndex(values, index));
                }
            }
        }
    }

    public void Dispose()
    {
        lock (gate)
        {
            values = null;
            lastIndex = -2; // -2 is disposed.
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
        return index; // return -1 is ok(means empty)
    }
}
