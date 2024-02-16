namespace R3;

public static partial class ObservableExtensions
{
    public static Task<T> AggregateAsync<T>(
        this Observable<T> source,
        Func<T, T, T> func,
        CancellationToken cancellationToken = default)
    {
        var observer = new AggregateAsync<T>(func, cancellationToken);
        source.Subscribe(observer);
        return observer.Task;
    }

    public static Task<TResult> AggregateAsync<T, TResult>(
        this Observable<T> source,
        TResult seed,
        Func<TResult, T, TResult> func,
        CancellationToken cancellationToken = default)
    {
        var observer = new AggregateAsync<T, TResult>(seed, func, cancellationToken);
        source.Subscribe(observer);
        return observer.Task;
    }

    public static Task<TResult> AggregateAsync<T, TAccumulate, TResult>
    (this Observable<T> source,
        TAccumulate seed,
        Func<TAccumulate, T, TAccumulate> func,
        Func<TAccumulate, TResult> resultSelector,
        CancellationToken cancellationToken = default)
    {
        var observer = new AggregateAsync<T, TAccumulate, TResult>(seed, func, resultSelector, cancellationToken);
        source.Subscribe(observer);
        return observer.Task;
    }
}

internal sealed class AggregateAsync<T>(
    Func<T, T, T> func,
    CancellationToken cancellationToken)
    : TaskObserverBase<T, T>(cancellationToken)
{
    T currentResult = default!;
    bool hasValue;

    protected override void OnNextCore(T value)
    {
        if (hasValue)
        {
            currentResult = func(currentResult, value); // OnNext error is route to OnErrorResumeCore
        }
        else
        {
            currentResult = value;
            hasValue = true;
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
            TrySetResult(currentResult);
        }
        else
        {
            TrySetException(new InvalidOperationException("Sequence contains no elements"));
        }
    }
}

internal sealed class AggregateAsync<T, TResult>(
    TResult seed,
    Func<TResult, T, TResult> func,
    CancellationToken cancellationToken)
    : TaskObserverBase<T, TResult>(cancellationToken)
{
    TResult currentValue = seed;

    protected override void OnNextCore(T value)
    {
        currentValue = func(currentValue, value); // OnNext error is route to OnErrorResumeCore
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
        TrySetResult(currentValue);
    }
}

internal sealed class AggregateAsync<T, TAccumulate, TResult>(
    TAccumulate seed,
    Func<TAccumulate, T, TAccumulate> func,
    Func<TAccumulate, TResult> resultSelector,
    CancellationToken cancellationToken)
    : TaskObserverBase<T, TResult>(cancellationToken)
{
    TAccumulate value = seed;

    protected override void OnNextCore(T value)
    {
        this.value = func(this.value, value); // OnNext error is route to OnErrorResumeCore
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

        try
        {
            var v = resultSelector(value); // trap this resultSelector exception
            TrySetResult(v);
        }
        catch (Exception ex)
        {
            TrySetException(ex);
        }
    }
}
