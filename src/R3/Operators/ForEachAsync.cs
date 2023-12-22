namespace R3;

public static partial class ObservableExtensions
{
    public static Task ForEachAsync<T>(this Observable<T> source, Action<T> action, CancellationToken cancellationToken = default)
    {
        var observer = new ForEachAsync<T>(action, cancellationToken);
        source.Subscribe(observer);
        return observer.Task;
    }

    public static Task ForEachAsync<T>(this Observable<T> source, Action<T, int> action, CancellationToken cancellationToken = default)
    {
        var observer = new ForEachAsyncWithIndex<T>(action, cancellationToken);
        source.Subscribe(observer);
        return observer.Task;
    }
}

internal sealed class ForEachAsync<T>(Action<T> action, CancellationToken cancellationToken)
    : TaskObserverBase<T, Unit>(cancellationToken)
{
    protected override void OnNextCore(T value)
    {
        action(value);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        TrySetException(error);
    }

    protected override void OnCompletedCore(Result result)
    {
        if (result.IsFailure)
        {
            TrySetException(result.Exception);
        }
        else
        {
            TrySetResult(default);
        }
    }
}

internal sealed class ForEachAsyncWithIndex<T>(Action<T, int> action, CancellationToken cancellationToken)
    : TaskObserverBase<T, Unit>(cancellationToken)
{
    int index;

    protected override void OnNextCore(T value)
    {
        action(value, index++);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        TrySetException(error);
    }

    protected override void OnCompletedCore(Result result)
    {
        if (result.IsFailure)
        {
            TrySetException(result.Exception);
        }
        else
        {
            TrySetResult(default);
        }
    }
}
