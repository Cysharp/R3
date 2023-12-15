namespace R3;

public static class SubjectExtensions
{
    public static void OnCompleted<T>(this ISubject<T> subject)
    {
        subject.OnCompleted(default);
    }

    public static void OnCompleted<T>(this ISubject<T> subject, Exception exception)
    {
        subject.OnCompleted(Result.Failure(exception));
    }
}
