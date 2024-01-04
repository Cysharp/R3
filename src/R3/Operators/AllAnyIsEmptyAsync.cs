namespace R3;

public static partial class ObservableExtensions
{
    public static Task<bool> AllAsync<T>(this Observable<T> source, Func<T, bool> predicate, CancellationToken cancellationToken = default)
    {
        var observer = new AllAsync<T>(predicate, cancellationToken);
        source.Subscribe(observer);
        return observer.Task;
    }

    public static Task<bool> AnyAsync<T>(this Observable<T> source, CancellationToken cancellationToken = default)
    {
        return AnyAsync<T>(source, static x => true, cancellationToken);
    }

    public static Task<bool> AnyAsync<T>(this Observable<T> source, Func<T, bool> predicate, CancellationToken cancellationToken = default)
    {
        var observer = new AnyAsync<T>(predicate, cancellationToken);
        source.Subscribe(observer);
        return observer.Task;
    }

    public static Task<bool> IsEmptyAsync<T>(this Observable<T> source, CancellationToken cancellationToken = default)
    {
        var observer = new IsEmptyAsync<T>(cancellationToken);
        source.Subscribe(observer);
        return observer.Task;
    }
}

internal sealed class AllAsync<T>(Func<T, bool> predicate, CancellationToken cancellationToken)
: TaskObserverBase<T, bool>(cancellationToken)
{
    protected override void OnNextCore(T value)
    {
        if (!predicate(value))
        {
            TrySetResult(false);
        }
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
            return;
        }
        TrySetResult(true);
    }
}

// AnyAsync
internal sealed class AnyAsync<T>(Func<T, bool> predicate, CancellationToken cancellationToken)
    : TaskObserverBase<T, bool>(cancellationToken)
{
    protected override void OnNextCore(T value)
    {
        if (predicate(value))
        {
            TrySetResult(true);
        }
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
            return;
        }
        TrySetResult(false);
    }
}

// IsEmptyAsync
internal sealed class IsEmptyAsync<T>(CancellationToken cancellationToken)
    : TaskObserverBase<T, bool>(cancellationToken)
{
    protected override void OnNextCore(T value)
    {
        TrySetResult(false);
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
            return;
        }
        TrySetResult(true);
    }
}
