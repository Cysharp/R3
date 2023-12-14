namespace R3;

public static partial class EventExtensions
{
    public static Task<T> ElementAtAsync<T>(this Event<T> source, int index, CancellationToken cancellationToken = default)
    {
        if (index < 0) throw new ArgumentOutOfRangeException("index");

        var subscriber = new ElementAtAsync<T>(index, false, default, cancellationToken);
        source.Subscribe(subscriber);
        return subscriber.Task;
    }

    public static Task<T> ElementAtAsync<T>(this Event<T> source, Index index, CancellationToken cancellationToken = default)
    {
        if (index.IsFromEnd)
        {
            if (index.Value <= 0) throw new ArgumentOutOfRangeException("index");
            var subscriber = new ElementAtFromEndAsync<T>(index.Value, false, default, cancellationToken);
            source.Subscribe(subscriber);
            return subscriber.Task;
        }
        else
        {
            return ElementAtAsync(source, index.Value, cancellationToken);
        }
    }

    public static Task<T> ElementAtOrDefaultAsync<T>(this Event<T> source, int index, T? defaultValue = default, CancellationToken cancellationToken = default)
    {
        if (index < 0) throw new ArgumentOutOfRangeException("index");
        var subscriber = new ElementAtAsync<T>(index, true, defaultValue, cancellationToken);
        source.Subscribe(subscriber);
        return subscriber.Task;
    }

    public static Task<T> ElementAtOrDefaultAsync<T>(this Event<T> source, Index index, T? defaultValue = default, CancellationToken cancellationToken = default)
    {
        if (index.IsFromEnd)
        {
            var subscriber = new ElementAtFromEndAsync<T>(index.Value, true, defaultValue, cancellationToken);
            source.Subscribe(subscriber);
            return subscriber.Task;
        }
        else
        {
            return ElementAtOrDefaultAsync(source, index.Value, defaultValue, cancellationToken);
        }
    }
}

internal sealed class ElementAtAsync<T>(int index, bool useDefaultValue, T? defaultValue, CancellationToken cancellationToken)
    : TaskSubscriberBase<T, T>(cancellationToken)
{
    int count = 0;

    protected override void OnNextCore(T value)
    {
        if (count++ == index)
        {
            TrySetResult(value);
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
        }
        else
        {
            if (useDefaultValue)
            {
                TrySetResult(defaultValue!);
            }
            else
            {
                TrySetException(new ArgumentOutOfRangeException("index"));
            }
        }
    }
}

// Index.IsFromEnd
internal sealed class ElementAtFromEndAsync<T>(int fromEndIndex, bool useDefaultValue, T? defaultValue, CancellationToken cancellationToken)
    : TaskSubscriberBase<T, T>(cancellationToken)
{
    Queue<T> queue = new Queue<T>(fromEndIndex);

    protected override void OnNextCore(T value)
    {
        if (queue.Count == fromEndIndex && queue.Count != 0)
        {
            queue.Dequeue();
        }

        queue.Enqueue(value);
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

        if (queue.Count == fromEndIndex)
        {
            var value = queue.Dequeue();
            TrySetResult(value);
            return;
        }

        if (useDefaultValue)
        {
            TrySetResult(defaultValue!);
            return;
        }

        TrySetException(new ArgumentOutOfRangeException("index"));
    }
}
