namespace R3;

public static partial class EventExtensions
{
    // TODO: this is working space, will remove this file after complete.


    // Time based
    // Frame based
    // OnErrorStop

    // Rx Merging:
    //CombineLatest, Merge, Zip, WithLatestFrom, ZipLatest, Switch, MostRecent

    // Standard Query:
    // Concat, Append, Prepend, Distinct, DistinctUntilChanged, Scan, Select, SelectMany

    // SkipTake:
    // Skip, SkipLast, SkipUntil, SkipWhile, Take, TakeLast, TakeLastBuffer, TakeUntil, TakeWhile
    // TakeUntilDestroy, TakeUntilDisable

    // return tasks:
    // All, Any, Contains, SequenceEqual, ElementAt, ElementAtOrDefault, IsEmpty, MaxBy, MinBy, ToDictionary, ToLookup, ForEachAsync
}

// TODO: now working



internal sealed class ElementAtAsync<T>(int index, bool useDefaultValue, T? defaultValue, CancellationToken cancellationToken)
    : TaskSubscriberBase<T, T>(cancellationToken)
{
    int count = 0;
    bool hasValue;

    protected override void OnNextCore(T value)
    {
        hasValue = true;
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
            // TODO:...
        }
        else
        {
            // TODO:...
        }
    }
}

// Index.IsFromEnd
internal sealed class ElementAtFromEndAsync<T>(int fromEndIndex, bool useDefaultValue, T? defaultValue, CancellationToken cancellationToken)
    : TaskSubscriberBase<T, T>(cancellationToken)
{
    bool hasValue;

    Queue<T> queue = new Queue<T>(fromEndIndex);

    protected override void OnNextCore(T value)
    {
        hasValue = true;
        if (queue.Count == fromEndIndex)
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

        if (!hasValue)
        {
            TrySetException(new InvalidOperationException("Sequence contains no elements"));
        }
        else
        {
            TrySetException(new ArgumentOutOfRangeException("index"));
        }
    }
}
