namespace R3;

public static partial class ObservableExtensions
{
    public static Task<int> CountAsync<T>(this Observable<T> source, CancellationToken cancellationToken = default)
    {
        var method = new CountAsync<T>(cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<int> CountAsync<T>(this Observable<T> source, Func<T, bool> predicate, CancellationToken cancellationToken = default)
    {
        var method = new CountFilterAsync<T>(predicate, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<long> LongCountAsync<T>(this Observable<T> source, CancellationToken cancellationToken = default)
    {
        var method = new LongCountAsync<T>(cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<long> LongCountAsync<T>(this Observable<T> source, Func<T, bool> predicate, CancellationToken cancellationToken = default)
    {
        var method = new LongCountFilterAsync<T>(predicate, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }
}

internal sealed class CountAsync<T>(CancellationToken cancellationToken) : TaskObserverBase<T, int>(cancellationToken)
{
    int count;

    protected override void OnNextCore(T _)
    {
        count = checked(count + 1);
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
        TrySetResult(count);
    }
}

internal sealed class CountFilterAsync<T>(Func<T, bool> predicate, CancellationToken cancellationToken) : TaskObserverBase<T, int>(cancellationToken)
{
    int count;

    protected override void OnNextCore(T value)
    {
        if (predicate(value))
        {
            count = checked(count + 1);
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
        TrySetResult(count);
    }
}

internal sealed class LongCountAsync<T>(CancellationToken cancellationToken) : TaskObserverBase<T, long>(cancellationToken)
{
    long count;

    protected override void OnNextCore(T _)
    {
        count = checked(count + 1);
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
        TrySetResult(count);
    }
}

internal sealed class LongCountFilterAsync<T>(Func<T, bool> predicate, CancellationToken cancellationToken) : TaskObserverBase<T, long>(cancellationToken)
{
    long count;

    protected override void OnNextCore(T value)
    {
        if (predicate(value))
        {
            count = checked(count + 1);
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
        TrySetResult(count);
    }
}
