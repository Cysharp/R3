namespace R3;

public static partial class Event
{
    // Never
    public static Event<T> Never<T>()
    {
        return R3.Never<T>.Instance;
    }
}

internal sealed class Never<T> : Event<T>
{
    // singleton
    public static readonly Never<T> Instance = new Never<T>();

    Never()
    {

    }

    protected override IDisposable SubscribeCore(Subscriber<T> subscriber)
    {
        return Disposable.Empty;
    }
}
