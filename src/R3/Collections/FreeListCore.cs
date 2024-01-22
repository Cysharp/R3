using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace R3.Collections;

[StructLayout(LayoutKind.Auto)]
public struct FreeListCore<T>
    where T : class
{
    readonly object gate;
    T?[]? values = null;
    int lastIndex;

    public FreeListCore(object gate)
    {
        // don't create values at initialize
        this.gate = gate;
        this.lastIndex = -1;
    }

    public bool IsDisposed => lastIndex == -2;

    public ReadOnlySpan<T?> AsSpan()
    {
        var last = Volatile.Read(ref lastIndex);
        var xs = Volatile.Read(ref values);
        if (xs == null) return ReadOnlySpan<T?>.Empty;
        return xs.AsSpan(0, last + 1);
    }

    public void Add(T item, out int removeKey)
    {
        lock (gate)
        {
            ThrowHelper.ThrowObjectDisposedIf(IsDisposed, typeof(FreeListCore<T>));

            if (values == null)
            {
                values = new T[1]; // initial size is 1.
            }

            // try find blank
            var index = FindNullIndex(values);
            if (index == -1)
            {
                // full, 1, 4, 6,...resize(x1.5)
                var len = values.Length;
                var newValues = len == 1 ? new T[4] : new T[len + len / 2];
                Array.Copy(values, newValues, len);
                Volatile.Write(ref values, newValues);
                index = len;
            }

            values[index] = item;
            if (lastIndex < index)
            {
                Volatile.Write(ref lastIndex, index);
            }

            removeKey = index; // index is remove key.
        }
    }

    public void Remove(int index)
    {
        lock (gate)
        {
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

    public bool RemoveSlow(T value)
    {
        lock (gate)
        {
            if (values == null) return false;
            if (lastIndex < 0) return false;

            var index = -1;
            var span = values.AsSpan(0, lastIndex + 1);
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] == value)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                Remove(index);
                return true;
            }
        }
        return false;
    }

    public void Clear(bool removeArray)
    {
        lock (gate)
        {
            if (lastIndex >= 0)
            {
                values.AsSpan(0, lastIndex + 1).Clear();
            }
            if (removeArray)
            {
                values = null;
            }
            if (lastIndex != -2)
            {
                lastIndex = -1;
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

#if NET6_0_OR_GREATER

    static int FindNullIndex(T?[] target)
    {
        var span = MemoryMarshal.CreateReadOnlySpan(
            ref Unsafe.As<T?, IntPtr>(ref MemoryMarshal.GetArrayDataReference(target)), target.Length);
        return span.IndexOf(IntPtr.Zero);
    }

#else

    static unsafe int FindNullIndex(T?[] target)
    {
        ref var head = ref Unsafe.As<T?, IntPtr>(ref MemoryMarshal.GetReference(target.AsSpan()));
        fixed (void* p = &head)
        {
            var span = new ReadOnlySpan<IntPtr>(p, target.Length);

#if NETSTANDARD2_1
            return span.IndexOf(IntPtr.Zero);
#else
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] == IntPtr.Zero) return i;
            }
            return -1;
#endif
        }
    }

#endif

#if NET8_0_OR_GREATER

    static int FindLastNonNullIndex(T?[] target, int lastIndex)
    {
        var span = MemoryMarshal.CreateReadOnlySpan(
            ref Unsafe.As<T?, IntPtr>(ref MemoryMarshal.GetArrayDataReference(target)), lastIndex); // without lastIndexed value.
        var index = span.LastIndexOfAnyExcept(IntPtr.Zero);
        return index; // return -1 is ok(means empty)
    }

#else

    static unsafe int FindLastNonNullIndex(T?[] target, int lastIndex)
    {
        ref var head = ref Unsafe.As<T?, IntPtr>(ref MemoryMarshal.GetReference(target.AsSpan()));
        fixed (void* p = &head)
        {
            var span = new ReadOnlySpan<IntPtr>(p, lastIndex); // without lastIndexed value.

            for (var i = span.Length - 1; i >= 0; i--)
            {
                if (span[i] != IntPtr.Zero) return i;
            }

            return -1;
        }
    }

#endif
}
