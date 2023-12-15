namespace R3;

public static partial class Observable
{
    public static Observable<T> Throw<T>(Exception exception)
    {
        return ReturnOnCompleted<T>(Result.Failure(exception));
    }

    public static Observable<T> Throw<T>(Exception exception, TimeProvider timeProvider)
    {
        return ReturnOnCompleted<T>(Result.Failure(exception), timeProvider);
    }

    public static Observable<T> Throw<T>(Exception exception, TimeSpan dueTime, TimeProvider timeProvider)
    {
        return ReturnOnCompleted<T>(Result.Failure(exception), dueTime, timeProvider);
    }
}


