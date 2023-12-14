namespace R3;

public static class PublisherExtensions
{
    public static void PublishOnCompleted<T>(this Publisher<T> publisher)
    {
        publisher.PublishOnCompleted(default);
    }
}
