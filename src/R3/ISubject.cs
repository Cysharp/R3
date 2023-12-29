namespace R3;

// NOTE: implement type must inherit from Observable<T>
// Subject<T>, ReactiveProperty<T>(as BehaviorSubject<T>), ReplaySubject<T>, ReplayFrameSubject<T>.
// All subjects, when disposed, call OnCompleted().
public interface ISubject<T>
{
    // Observable
    IDisposable Subscribe(Observer<T> observer);

    // Observer
    void OnNext(T value);
    void OnErrorResume(Exception error);
    void OnCompleted(Result complete);
}

public static class SubjectExtensions
{
    public static Observer<T> AsObserver<T>(this ISubject<T> subject)
    {
        return new SubjectToObserver<T>(subject);
    }

    public static void OnCompleted<T>(this ISubject<T> subject)
    {
        subject.OnCompleted(default);
    }

    public static void OnCompleted<T>(this ISubject<T> subject, Exception exception)
    {
        subject.OnCompleted(Result.Failure(exception));
    }
}

internal sealed class SubjectToObserver<T>(ISubject<T> subject) : Observer<T>
{
    protected override void OnNextCore(T value)
    {
        subject.OnNext(value);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        subject.OnErrorResume(error);
    }

    protected override void OnCompletedCore(Result result)
    {
        subject.OnCompleted(result);
    }
}
