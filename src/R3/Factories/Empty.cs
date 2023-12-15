namespace R3;

public static partial class Observable
{
    public static Observable<T> Empty<T>()
    {
        return R3.Empty<T>.Instance;
    }

    public static Observable<T> Empty<T>(TimeProvider timeProvider)
    {
        return ReturnOnCompleted<T>(Result.Success, timeProvider);
    }

    public static Observable<T> Empty<T>(TimeSpan dueTime, TimeProvider timeProvider)
    {
        return ReturnOnCompleted<T>(Result.Success, dueTime, timeProvider);
    }
}

internal sealed class Empty<T> : Observable<T>
{
    // singleton
    public static readonly Empty<T> Instance = new Empty<T>();

    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        observer.OnCompleted();
        return Disposable.Empty;
    }

    Empty()
    {

    }
}
