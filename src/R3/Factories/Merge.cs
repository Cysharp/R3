namespace R3;

public static partial class Observable
{
    /// <summary>
    ///   <para>
    ///     Combine multiple <see cref="Observable"/>s into one by merging their emissions.
    ///   </para>
    ///   <para>
    ///     ReactiveX docs link: <see href="https://reactivex.io/documentation/operators/merge.html" />
    ///   </para>
    ///   <para>
    ///     In the following examples each column represent single time tick.
    ///     "--" means no emission on this tick.
    ///     "|-" means completion.
    ///   </para>
    ///   <example>
    ///     <para>Example 1:</para>
    ///     <code>
    /// Number:       1  2  3  4  5  6  7  8  9 10
    /// Sequence 1:  -- -- -- 20 -- 40 -- 60 -- -->
    /// Sequence 2:  -- 01 -- 02 -- 03 -- -- -- -->
    /// Sequence 3:  -- -- -- -- 00 -- 00 -- 00 -->
    ///
    /// Results:     -- 01 -- 20 00 40 00 60 00 -->
    ///                       02    03            >
    ///     </code>
    ///   </example>
    ///   <example>
    ///     <para>Example 2:</para>
    ///     <code>
    /// Number:       1  2  3  4  5  6  7  8  9 10 11
    /// Sequence 1:  -- -- -- -- -- 20 -- |- -- -- -->
    /// Sequence 2:  -- 01 -- 02 -- |- -- -- -- -- -->
    /// Sequence 3:  -- -- 00 -- -- -- 00 -- -- 00 -->
    ///
    /// Results:     -- 01 00 02 -- 20 00 -- -- 00 -->
    ///     </code>
    ///   </example>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sources"></param>
    /// <returns>Value of type <see cref="Observable{T}"/></returns>
    public static Observable<T> Merge<T>(params Observable<T>[] sources)
    {
        return new Merge<T>(sources);
    }

    public static Observable<T> Merge<T>(this IEnumerable<Observable<T>> sources)
    {
        return new Merge<T>(sources);
    }
}

public static partial class ObservableExtensions
{
    public static Observable<T> Merge<T>(this Observable<T> source, Observable<T> second)
    {
        return new Merge<T>(new[] { source, second });
    }
}

internal sealed class Merge<T>(IEnumerable<Observable<T>> sources) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var merge = new _Merge(observer);
        var builder = Disposable.CreateBuilder();

        var count = 0;
        foreach (var item in sources)
        {
            item.Subscribe(new _MergeObserver(merge)).AddTo(ref builder);
            count++;
        }

        merge.disposable.Disposable = builder.Build();

        merge.SetSourceCount(count);

        return merge;
    }

    sealed class _Merge(Observer<T> observer) : IDisposable
    {
        public Observer<T> observer = observer;
        public SingleAssignmentDisposableCore disposable;
        public readonly object gate = new object();

        int sourceCount = -1; // not set yet.
        int completeCount;

        public void SetSourceCount(int count)
        {
            lock (gate)
            {
                sourceCount = count;
                if (sourceCount == completeCount)
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        // when all sources are completed, then this observer is completed
        public void TryPublishCompleted()
        {
            lock (gate)
            {
                completeCount++;
                if (completeCount == sourceCount)
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void Dispose()
        {
            disposable.Dispose();
        }
    }

    sealed class _MergeObserver(_Merge parent) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            lock (parent.gate)
            {
                parent.observer.OnNext(value);
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            lock (parent.gate)
            {
                parent.observer.OnErrorResume(error);
            }
        }

        protected override void OnCompletedCore(Result result)
        {
            if (result.IsFailure)
            {
                // when error, publish OnCompleted immediately
                lock (parent.gate)
                {
                    parent.observer.OnCompleted(result);
                }
            }
            else
            {
                parent.TryPublishCompleted();
            }
        }
    }
}
