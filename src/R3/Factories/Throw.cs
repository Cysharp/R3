namespace R3;

public static partial class Event
{
    public static Event<TMessage, Result<Unit>> Throw<TMessage>(Exception exception)
    {
        return Throw<TMessage, Unit>(exception);
    }

    public static Event<TMessage, Result<TComplete>> Throw<TMessage, TComplete>(Exception exception)
    {
        return ReturnOnCompleted<TMessage, Result<TComplete>>(Result.Failure<TComplete>(exception));
    }

    public static Event<TMessage, Result<Unit>> Throw<TMessage>(Exception exception, TimeProvider timeProvider)
    {
        return Throw<TMessage, Unit>(exception, timeProvider);
    }

    public static Event<TMessage, Result<TComplete>> Throw<TMessage, TComplete>(Exception exception, TimeProvider timeProvider)
    {
        return ReturnOnCompleted<TMessage, Result<TComplete>>(Result.Failure<TComplete>(exception), timeProvider);
    }

    public static Event<TMessage, Result<Unit>> Throw<TMessage>(Exception exception, TimeSpan dueTime, TimeProvider timeProvider)
    {
        return Throw<TMessage, Unit>(exception, dueTime, timeProvider);
    }

    public static Event<TMessage, Result<TComplete>> Throw<TMessage, TComplete>(Exception exception, TimeSpan dueTime, TimeProvider timeProvider)
    {
        return ReturnOnCompleted<TMessage, Result<TComplete>>(Result.Failure<TComplete>(exception), dueTime, timeProvider);
    }
}
