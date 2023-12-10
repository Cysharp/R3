namespace R3;

public static class SubscriberExtensions
{
    public static void OnCompleted<T>(this Subscriber<T, Unit> subscriber)
    {
        subscriber.OnCompleted(default);
    }
}
