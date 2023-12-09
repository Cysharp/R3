namespace R2;

public static class SubscriberExtensions
{
    public static void OnCompleted<T>(this Subscriber<T, Unit> subscriber)
    {
        subscriber.OnCompleted(Unit.Default);
    }
}
