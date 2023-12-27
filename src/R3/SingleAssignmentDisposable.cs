namespace R3;

public sealed class SingleAssignmentDisposable : IDisposable
{
    SingleAssignmentDisposableCore core;

    public bool IsDisposed => core.IsDisposed;

    public IDisposable? Disposable
    {
        get => core.Disposable;
        set => core.Disposable = value;
    }

    public void Dispose()
    {
        core.Dispose();
    }
}

// struct, be carefult to use
public struct SingleAssignmentDisposableCore
{
    IDisposable? current;

    public bool IsDisposed => Volatile.Read(ref current) == DisposedSentinel.Instance;

    public IDisposable? Disposable
    {
        get
        {
            var field = Volatile.Read(ref current);
            if (field == DisposedSentinel.Instance)
            {
                return R3.Disposable.Empty; // don't expose sentinel
            }
            return field;
        }
        set
        {
            var field = Interlocked.CompareExchange(ref current, value, null);
            if (field == null)
            {
                // ok to set.
                return;
            }

            if (field == DisposedSentinel.Instance)
            {
                // We've already been disposed, so dispose the value we've just been given.
                value?.Dispose();
                return;
            }

            // otherwise, invalid assignment
            ThrowAlreadyAssignment();
        }
    }

    public void Dispose()
    {
        var field = Interlocked.Exchange(ref current, DisposedSentinel.Instance);
        if (field != DisposedSentinel.Instance)
        {
            field?.Dispose();
        }
    }

    static void ThrowAlreadyAssignment()
    {
        throw new InvalidOperationException("Disposable is already assigned.");
    }

    sealed class DisposedSentinel : IDisposable
    {
        public static readonly DisposedSentinel Instance = new();

        DisposedSentinel()
        {
        }

        public void Dispose()
        {
        }
    }
}
