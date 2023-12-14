namespace R3;

public static class PublisherExtensions
{
    public static void PublishOnCompleted<T>(this Publisher<T> publisher)
    {
        publisher.PublishOnCompleted(default);
    }

    public static void PublishOnCompleted<T>(this Publisher<T> publisher, Exception exception)
    {
        publisher.PublishOnCompleted(Result.Failure(exception));
    }
}
