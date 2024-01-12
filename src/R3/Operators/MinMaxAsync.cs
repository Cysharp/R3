namespace R3;

public static partial class ObservableExtensions
{
    public static Task<(T Min, T Max)> MinMaxAsync<T>(this Observable<T> source, CancellationToken cancellationToken = default)
    {
        return source.MinMaxAsync(Comparer<T>.Default, cancellationToken);
    }

    public static Task<(T Min, T Max)> MinMaxAsync<T>(this Observable<T> source, IComparer<T> comparer, CancellationToken cancellationToken = default)
    {
        var method = new MinMaxAsync<T>(comparer, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<(TResult Min, TResult Max)> MinMaxAsync<TSource, TResult>(this Observable<TSource> source, Func<TSource, TResult> selector, CancellationToken cancellationToken = default)
    {
        return source.MinMaxAsync(selector, Comparer<TResult>.Default, cancellationToken);
    }

    public static Task<(TResult Min, TResult Max)> MinMaxAsync<TSource, TResult>(this Observable<TSource> source, Func<TSource, TResult> selector, IComparer<TResult> comparer, CancellationToken cancellationToken = default)
    {
        var method = new MinMaxAsync<TSource, TResult>(selector, comparer, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }
}

internal sealed class MinMaxAsync<T>(IComparer<T> comparer, CancellationToken cancellationToken) : TaskObserverBase<T, (T, T)>(cancellationToken)
{
    T min = default!;
    T max = default!;
    bool hasValue;

    protected override void OnNextCore(T value)
    {
        if (!hasValue)
        {
            min = value;
            max = value;
            hasValue = true;
            return;
        }

        if (comparer.Compare(value, min) < 0)
        {
            min = value;
        }
        if (comparer.Compare(value, max) > 0)
        {
            max = value;
        }
        hasValue = true;
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
            TrySetResult((min, max));
        }
        else
        {
            TrySetException(new InvalidOperationException("Sequence contains no elements"));
        }
    }
}

internal sealed class MinMaxAsync<TSource, TResult>(Func<TSource, TResult> selector, IComparer<TResult> comparer, CancellationToken cancellationToken) : TaskObserverBase<TSource, (TResult, TResult)>(cancellationToken)
{
    TResult min = default!;
    TResult max = default!;
    bool hasValue;

    protected override void OnNextCore(TSource value)
    {
        var current = selector(value);
        if (!hasValue)
        {
            min = current;
            max = current;
            hasValue = true;
            return;
        }

        if (comparer.Compare(current, min) < 0)
        {
            min = current;
        }
        if (comparer.Compare(current, max) > 0)
        {
            max = current;
        }
        hasValue = true;
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
            TrySetResult((min, max));
        }
        else
        {
            TrySetException(new InvalidOperationException("Sequence contains no elements"));
        }
    }
}
