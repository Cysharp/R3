namespace R3.Internal;

internal struct ArrayBuffer<T>
{
    public readonly T[] Array;
    int index;

    public int Count => index;

    public Span<T> Span => Array.AsSpan(0, index);

    public ArrayBuffer(int count)
    {
        Array = new T[count];
    }

    public void Add(T value)
    {
        Array[index++] = value;
    }
}
