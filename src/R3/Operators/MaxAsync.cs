namespace R3;

public static partial class ObservableExtensions
{
    public static Task<T> MaxAsync<T>(this Observable<T> source, CancellationToken cancellationToken = default)
    {
        var method = new MaxAsync<T>(Comparer<T>.Default, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<T> MaxAsync<T>(this Observable<T> source, IComparer<T> comparer, CancellationToken cancellationToken = default)
    {
        var method = new MaxAsync<T>(comparer, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<TResult> MaxAsync<TSource, TResult>(this Observable<TSource> source, Func<TSource, TResult> selector, CancellationToken cancellationToken = default)
    {
        var method = new MaxAsync<TSource, TResult>(selector, Comparer<TResult>.Default, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<TResult> MaxAsync<TSource, TResult>(this Observable<TSource> source, Func<TSource, TResult> selector, IComparer<TResult> comparer, CancellationToken cancellationToken = default)
    {
        var method = new MaxAsync<TSource, TResult>(selector, comparer, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }
}

internal sealed class MaxAsync<T>(IComparer<T> comparer, CancellationToken cancellation) : TaskObserverBase<T, T>(cancellation)
{
    T current = default!;
    bool hasValue;

    protected override void OnNextCore(T value)
    {
        if (!hasValue)
        {
            hasValue = true;
            current = value;
            return;
        }

        if (comparer.Compare(value, current) > 0)
        {
            current = value;
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

        if (hasValue)
        {
            TrySetResult(current);
        }
        else
        {
            TrySetException(new InvalidOperationException("Sequence contains no elements"));
        }
    }
}

internal sealed class MaxAsync<TSource, TResult>(Func<TSource, TResult> selector, IComparer<TResult> comparer, CancellationToken cancellation) : TaskObserverBase<TSource, TResult>(cancellation)
{
    TResult current = default!;
    bool hasValue;

    protected override void OnNextCore(TSource value)
    {
        var nextValue = selector(value);
        if (!hasValue)
        {
            hasValue = true;
            current = nextValue;
            return;
        }

        if (comparer.Compare(nextValue, current) > 0)
        {
            current = nextValue;
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

        if (hasValue)
        {
            TrySetResult(current);
        }
        else
        {
            TrySetException(new InvalidOperationException("Sequence contains no elements"));
        }
    }
}
