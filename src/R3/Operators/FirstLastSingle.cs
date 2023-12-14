namespace R3;

using static FirstLastSingleOperation;

public static partial class EventExtensions
{
    public static Task<T> FirstAsync<T>(this Event<T> source, CancellationToken cancellationToken = default) => FirstAsync(source, static _ => true, cancellationToken);
    public static Task<T> FirstOrDefaultAsync<T>(this Event<T> source, T? defaultValue = default, CancellationToken cancellationToken = default) => FirstOrDefaultAsync(source, static _ => true, defaultValue, cancellationToken);
    public static Task<T> LastAsync<T>(this Event<T> source, CancellationToken cancellationToken = default) => LastAsync(source, static _ => true, cancellationToken);
    public static Task<T> LastOrDefaultAsync<T>(this Event<T> source, T? defaultValue = default, CancellationToken cancellationToken = default) => LastOrDefaultAsync(source, static _ => true, defaultValue, cancellationToken);
    public static Task<T> SingleAsync<T>(this Event<T> source, CancellationToken cancellationToken = default) => SingleAsync(source, static _ => true, cancellationToken);
    public static Task<T> SingleOrDefaultAsync<T>(this Event<T> source, T? defaultValue = default, CancellationToken cancellationToken = default) => SingleOrDefaultAsync(source, static _ => true, defaultValue, cancellationToken);

    // with predicate
    public static Task<T> FirstAsync<T>(this Event<T> source, Func<T, bool> predicate, CancellationToken cancellationToken = default) => FirstLastSingleAsync(source, First, false, default, predicate, cancellationToken);
    public static Task<T> FirstOrDefaultAsync<T>(this Event<T> source, Func<T, bool> predicate, T? defaultValue = default, CancellationToken cancellationToken = default) => FirstLastSingleAsync(source, First, true, defaultValue, predicate, cancellationToken);
    public static Task<T> LastAsync<T>(this Event<T> source, Func<T, bool> predicate, CancellationToken cancellationToken = default) => FirstLastSingleAsync(source, Last, false, default, predicate, cancellationToken);
    public static Task<T> LastOrDefaultAsync<T>(this Event<T> source, Func<T, bool> predicate, T? defaultValue = default, CancellationToken cancellationToken = default) => FirstLastSingleAsync(source, Last, true, defaultValue, predicate, cancellationToken);
    public static Task<T> SingleAsync<T>(this Event<T> source, Func<T, bool> predicate, CancellationToken cancellationToken = default) => FirstLastSingleAsync(source, Single, false, default, predicate, cancellationToken);
    public static Task<T> SingleOrDefaultAsync<T>(this Event<T> source, Func<T, bool> predicate, T? defaultValue = default, CancellationToken cancellationToken = default) => FirstLastSingleAsync(source, Single, true, defaultValue, predicate, cancellationToken);

    static Task<T> FirstLastSingleAsync<T>(this Event<T> source, FirstLastSingleOperation operation, bool useDefaultIfEmpty, T? defaultValue, Func<T, bool> predicate, CancellationToken cancellationToken)
    {
        var subscriber = new FirstLastSingle<T>(operation, useDefaultIfEmpty, defaultValue, predicate, cancellationToken);
        source.Subscribe(subscriber);
        return subscriber.Task;
    }
}

internal sealed class FirstLastSingle<T>(FirstLastSingleOperation operation, bool useDefaultIfEmpty, T? defaultValue, Func<T, bool> predicate, CancellationToken cancellationToken)
    : TaskSubscriberBase<T, T>(cancellationToken)
{
    bool hasValue;
    T? latestValue = defaultValue;

    protected override void OnNextCore(T value)
    {
        if (!predicate(value)) return;

        if (operation == FirstLastSingleOperation.Single && hasValue)
        {
            TrySetException(new InvalidOperationException("Sequence contains more than one element."));
            return;
        }

        hasValue = true;
        if (operation == FirstLastSingleOperation.First)
        {
            TrySetResult(value); // First / FirstOrDefault
        }
        else
        {
            latestValue = value;
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

        if (hasValue || useDefaultIfEmpty)
        {
            TrySetResult(latestValue!);
            return;
        }

        TrySetException(new InvalidOperationException("Sequence contains no elements."));
    }
}

internal enum FirstLastSingleOperation
{
    First,
    Last,
    Single
}
