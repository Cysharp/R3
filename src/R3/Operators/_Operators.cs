
using System.Diagnostics.CodeAnalysis;

namespace R3
{
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
}

namespace R3.Operators
{
    // TODO: now working

    internal sealed class ElementAtAsync<TMessage>(Event<TMessage> source, int index, CancellationToken cancellationToken)
        : TaskSubscriberBase<TMessage, TMessage>(cancellationToken)
    {
        int count = 0;

        protected override void OnNextCore(TMessage message)
        {
            if (count++ == index)
            {
                TrySetResult(message);
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            TrySetException(error);
        }
    }

    internal sealed class ElementAtAsync<TMessage, TComplete>(CompletableEvent<TMessage, TComplete> source, int index, bool useDefaultValue, TMessage? defaultValue, CancellationToken cancellationToken)
        : TaskSubscriberBase<TMessage, TComplete, TMessage>(cancellationToken)
    {
        int count = 0;
        bool hasValue;

        protected override void OnNextCore(TMessage message)
        {
            hasValue = true;
            if (count++ == index)
            {
                TrySetResult(message);
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            TrySetException(error);
        }

        protected override void OnCompletedCore(TComplete complete)
        {
            throw new NotImplementedException();
        }
    }

    // Index.IsFromEnd
    internal sealed class ElementAtFromEndAsync<TMessage, TComplete>(CompletableEvent<TMessage, TComplete> source, int fromEndIndex, bool useDefaultValue, TMessage? defaultValue, CancellationToken cancellationToken)
        : TaskSubscriberBase<TMessage, TComplete, TMessage>(cancellationToken)
    {
        int count = 0;
        bool hasValue;

        Queue<TMessage> queue = new Queue<TMessage>(fromEndIndex);

        protected override void OnNextCore(TMessage message)
        {
            hasValue = true;
            if (queue.Count == fromEndIndex)
            {
                queue.Dequeue();
            }

            queue.Enqueue(message);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            TrySetException(error);
        }

        protected override void OnCompletedCore(TComplete complete)
        {
            if (queue.Count == fromEndIndex)
            {
                var result = queue.Dequeue();
                TrySetResult(result);
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
}
