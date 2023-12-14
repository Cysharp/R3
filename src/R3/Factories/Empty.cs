namespace R3;

public static partial class Event
{
    public static Event<T> Empty<T>()
    {
        return R3.Empty<T>.Instance;
    }

    public static Event<T> Empty<T>(TimeProvider timeProvider)
    {
        return ReturnOnCompleted<T>(Result.Success, timeProvider);
    }

    public static Event<T> Empty<T>(TimeSpan dueTime, TimeProvider timeProvider)
    {
        return ReturnOnCompleted<T>(Result.Success, dueTime, timeProvider);
    }
}

internal sealed class Empty<T> : Event<T>
{
    // singleton
    public static readonly Empty<T> Instance = new Empty<T>();

    protected override IDisposable SubscribeCore(Subscriber<T> subscriber)
    {
        subscriber.OnCompleted();
        return Disposable.Empty;
    }

    Empty()
    {

    }
}
