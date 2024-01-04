using System.Runtime.CompilerServices;

#if !NET6_0_OR_GREATER
namespace System.Runtime.InteropServices;

internal static class CollectionsMarshal
{
#pragma warning disable CS8618
#pragma warning disable CS0169
#pragma warning disable CS0649

    class ListDummy<T>
    {
        public T[] Items;
        int size;
        int version;
    }

    internal static Span<T> AsSpan<T>(List<T> list)
    {
        return Unsafe.As<ListDummy<T>>(list).Items.AsSpan(0, list.Count);
    }
}
#endif
