namespace R3;

public static partial class EventExtensions
{
    public static Task<TResult> AggregateAsync<T, TAccumulate, TResult>
        (this Observable<T> source,
        TAccumulate seed,
        Func<TAccumulate, T, TAccumulate> func,
        Func<TAccumulate, TResult> resultSelector,
        CancellationToken cancellationToken = default)
    {
        var subscriber = new AggregateAsync<T, TAccumulate, TResult>(seed, func, resultSelector, cancellationToken);
        source.Subscribe(subscriber);
        return subscriber.Task;
    }
}

internal sealed class AggregateAsync<T, TAccumulate, TResult>(
    TAccumulate seed,
    Func<TAccumulate, T, TAccumulate> func,
    Func<TAccumulate, TResult> resultSelector,
    CancellationToken cancellationToken)
    : TaskSubscriberBase<T, TResult>(cancellationToken)
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
