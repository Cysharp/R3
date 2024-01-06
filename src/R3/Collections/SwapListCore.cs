namespace R3.Collections;

public struct SwapListCore<T>
{
    const int InitialArraySize = 4;

    T[]? arrayA;
    int lengthA;

    T[]? arrayB;
    int lengthB;

    bool useA;

    public bool HasValue
    {
        get
        {
            return lengthA > 0 || lengthB > 0;
        }
    }

    public void Add(T value)
    {
        if (useA)
        {
            if (arrayA == null)
            {
                arrayA = new T[InitialArraySize];
            }
            else if (lengthA == arrayA.Length)
            {
                var tmp = new T[arrayA.Length * 2];
                Array.Copy(arrayA, tmp, arrayA.Length);
                arrayA = tmp;
            }
            arrayA[lengthA++] = value;
        }
        else
        {
            if (arrayB == null)
            {
                arrayB = new T[InitialArraySize];
            }
            else if (lengthB == arrayB.Length)
            {
                var tmp = new T[arrayB.Length * 2];
                Array.Copy(arrayB, tmp, arrayB.Length);
                arrayB = tmp;
            }
            arrayB[lengthB++] = value;
        }
    }

    public ReadOnlySpan<T> Swap(out bool token)
    {
        if (useA)
        {
            useA = false;
            if (arrayA == null)
            {
                token = true;
                return ReadOnlySpan<T>.Empty;
            }
            else
            {
                token = true;
                return arrayA.AsSpan(0, lengthA);
            }
        }
        else
        {
            useA = true;
            if (arrayB == null)
            {
                token = false;
                return ReadOnlySpan<T>.Empty;
            }
            else
            {
                token = false;
                return arrayB.AsSpan(0, lengthB);
            }
        }
    }

    public void Clear(bool token)
    {
        if (token) // token means useA
        {
            if (arrayA != null)
            {
                Array.Clear(arrayA, 0, lengthA);
                lengthA = 0;
            }
        }
        else
        {
            if (arrayB != null)
            {
                Array.Clear(arrayB, 0, lengthB);
                lengthB = 0;
            }
        }

        if (lengthB == 0)
        {
            useA = true;
        }
    }

    public void Dispose()
    {
        if (arrayA != null)
        {
            Array.Clear(arrayA, 0, lengthA);
            arrayA = null;
            lengthA = 0;
        }

        if (arrayB != null)
        {
            Array.Clear(arrayB, 0, lengthB);
            arrayB = null;
            lengthB = 0;
        }
    }
}
