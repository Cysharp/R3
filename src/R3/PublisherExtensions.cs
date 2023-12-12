namespace R3;

public static class PublisherExtensions
{
    public static void PublishOnCompleted<T>(this CompletablePublisher<T, Unit> publisher)
    {
        publisher.PublishOnCompleted(default);
    }
}
