namespace R3.Internal;

internal struct ListDisposableCore : IDisposable
{
    IDisposable?[] disposables;
    int count;
    object gate;

    public ListDisposableCore(int initialCount, object gate)
    {
        this.disposables = new IDisposable?[initialCount];
        this.gate = gate;
    }

    public void Add(IDisposable disposable)
    {
        lock (gate)
        {
            if (disposables.Length == count)
            {
                Array.Resize(ref disposables, count * 2);
            }

            disposables[count++] = disposable;
        }
    }

    public void RemoveAt(int index)
    {
        lock (gate)
        {
            if (index < 0 || index >= count)
            {
                return;
            }

            ref var d = ref disposables[index];
            if (d != null)
            {
                d.Dispose();
            }
            d = null;
        }
    }

    public void RemoveAllExceptAt(int index)
    {
        lock (gate)
        {
            if (index < 0 || index >= count)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                if (i == index) continue;

                ref var d = ref disposables[i];
                if (d != null)
                {
                    d.Dispose();
                }
                d = null;
            }
        }
    }

    public void Dispose()
    {
        lock (gate)
        {
            for (int i = 0; i < count; i++)
            {
                disposables[i]?.Dispose();
                disposables[i] = null;
            }
            count = 0;
        }
    }
}
