namespace R3;

public static class ObserverExtensions
{
    public static void OnCompleted<T>(this Observer<T> observer)
    {
        observer.OnCompleted(Result.Success);
    }

    public static void OnCompleted<T>(this Observer<T> observer, Exception exception)
    {
        observer.OnCompleted(Result.Failure(exception));
    }

    public static Observer<T> Wrap<T>(this Observer<T> observer)
    {
        return new WrappedObserver<T>(observer);
    }


    public static Observer<T> ToObserver<T>(this IObserver<T> observer)
    {
        return new IObserverToObserver<T>(observer);
    }
}

internal sealed class IObserverToObserver<T>(IObserver<T> observer) : Observer<T>
{
    protected override void OnNextCore(T value)
    {
        observer.OnNext(value);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        observer.OnError(error);
    }

    protected override void OnCompletedCore(Result result)
    {
        if (result.IsFailure)
        {
            observer.OnError(result.Exception);
        }
        else
        {
            observer.OnCompleted();
        }
    }
}

internal sealed class WrappedObserver<T>(Observer<T> observer) : Observer<T>
{
    protected override void OnNextCore(T value)
    {
        observer.OnNext(value);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        observer.OnErrorResume(error);
    }

    protected override void OnCompletedCore(Result result)
    {
        observer.OnCompleted(result);
    }

    protected override void DisposeCore()
    {
        observer.Dispose();
    }
}
