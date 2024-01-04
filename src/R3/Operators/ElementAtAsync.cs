namespace R3;

public static partial class ObservableExtensions
{
    public static Task<T> ElementAtAsync<T>(this Observable<T> source, int index, CancellationToken cancellationToken = default)
    {
        if (index < 0) throw new ArgumentOutOfRangeException("index");

        var observer = new ElementAtAsync<T>(index, false, default, cancellationToken);
        source.Subscribe(observer);
        return observer.Task;
    }

#if !NETSTANDARD2_0

    public static Task<T> ElementAtAsync<T>(this Observable<T> source, Index index, CancellationToken cancellationToken = default)
    {
        if (index.IsFromEnd)
        {
            if (index.Value <= 0) throw new ArgumentOutOfRangeException("index");
            var observer = new ElementAtFromEndAsync<T>(index.Value, false, default, cancellationToken);
            source.Subscribe(observer);
            return observer.Task;
        }
        else
        {
            return ElementAtAsync(source, index.Value, cancellationToken);
        }
    }

#endif

    public static Task<T> ElementAtOrDefaultAsync<T>(this Observable<T> source, int index, T? defaultValue = default, CancellationToken cancellationToken = default)
    {
        if (index < 0) throw new ArgumentOutOfRangeException("index");
        var observer = new ElementAtAsync<T>(index, true, defaultValue, cancellationToken);
        source.Subscribe(observer);
        return observer.Task;
    }

#if !NETSTANDARD2_0

    public static Task<T> ElementAtOrDefaultAsync<T>(this Observable<T> source, Index index, T? defaultValue = default, CancellationToken cancellationToken = default)
    {
        if (index.IsFromEnd)
        {
            var observer = new ElementAtFromEndAsync<T>(index.Value, true, defaultValue, cancellationToken);
            source.Subscribe(observer);
            return observer.Task;
        }
        else
        {
            return ElementAtOrDefaultAsync(source, index.Value, defaultValue, cancellationToken);
        }
    }

#endif

}

internal sealed class ElementAtAsync<T>(int index, bool useDefaultValue, T? defaultValue, CancellationToken cancellationToken)
    : TaskObserverBase<T, T>(cancellationToken)
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
    : TaskObserverBase<T, T>(cancellationToken)
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
