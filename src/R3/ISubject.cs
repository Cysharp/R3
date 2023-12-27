namespace R3;

// NOTE: implement type must inherit from Observable<T>
public interface ISubject<T>
{
    // Observable
    IDisposable Subscribe(Observer<T> observer);

    // Observer
    void OnNext(T value);
    void OnErrorResume(Exception error);
    void OnCompleted(Result complete);

    // Conversion
    public Observer<T> AsObserver()
    {
        return new SubjectToObserver<T>(this);
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
