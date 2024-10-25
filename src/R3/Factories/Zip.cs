namespace R3;

public static partial class Observable
{
    /// <summary>
    ///   <para>
    ///     Combine the emissions of multiple <see cref="Observable"/>s together via a specified function and emit single items for each combination based on the results of this function.
    ///   </para>
    ///   <para>
    ///     ReactiveX docs link: <see href="https://reactivex.io/documentation/operators/zip.html" />
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
    /// Results:
    ///   1: --
    ///   2: --
    ///   3: --
    ///   4: --
    ///   5: [20,01,00]
    ///   6: --
    ///   7: [40,02,00]
    ///   8: --
    ///   9: [60,03,00]
    ///  10: --
    ///     </code>
    ///   </example>
    ///   <example>
    ///     <para>Example 2:</para>
    ///     <code>
    /// Number:       1  2  3  4  5  6  7  8  9 10 11
    /// Sequence 1:  -- -- -- 20 -- 40 -- 60 |- -- -->
    /// Sequence 2:  -- 01 -- 02 03 -- |- -- -- -- -->
    /// Sequence 3:  -- -- 00 -- -- -- -- -- 00 00 -->
    ///
    /// Results:
    ///   1: --
    ///   2: --
    ///   3: --
    ///   4: [20,01,00]
    ///   5: --
    ///   6: --
    ///   7: --
    ///   8: --
    ///   9: [40,02,00]
    ///  10: [60,03,00]
    ///  11: --
    ///     </code>
    ///   </example>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sources"></param>
    /// <returns></returns>
    public static Observable<T[]> Zip<T>(params Observable<T>[] sources)
    {
        return new Zip<T>(sources);
    }

    public static Observable<T[]> Zip<T>(IEnumerable<Observable<T>> sources)
    {
        return new Zip<T>(sources);
    }
}

internal sealed class Zip<T>(IEnumerable<Observable<T>> sources) : Observable<T[]>
{
    protected override IDisposable SubscribeCore(Observer<T[]> observer)
    {
        return new _Zip(observer, sources).Run();
    }

    sealed class _Zip : IDisposable
    {
        readonly Observer<T[]> observer;
        readonly Observable<T>[] sources;
        readonly ZipObserver[] observers; // as lock gate

        public _Zip(Observer<T[]> observer, IEnumerable<Observable<T>> sources)
        {
            this.observer = observer;
            if (sources is Observable<T>[] array)
            {
                this.sources = array;
            }
            else
            {
                this.sources = sources.ToArray();
            }

            var observers = new ZipObserver[this.sources.Length];
            for (int i = 0; i < observers.Length; i++)
            {
                observers[i] = new ZipObserver(this);
            }
            this.observers = observers;
        }

        public IDisposable Run()
        {
            try
            {
                for (int i = 0; i < sources.Length; i++)
                {
                    sources[i].Subscribe(observers[i]);
                }
            }
            catch
            {
                Dispose();
                throw;
            }
            return this;
        }

        public void TryPublishOnNext()
        {
            bool requireCallOnCompleted = false;
            foreach (var item in observers)
            {
                if (!item.HasValue(out var shouldComplete)) return;
                if (shouldComplete)
                {
                    requireCallOnCompleted = true;
                }
            }

            var values = new T[observers.Length];
            for (int i = 0; i < observers.Length; i++)
            {
                values[i] = observers[i].Values.Dequeue();
            }
            observer.OnNext(values);

            if (requireCallOnCompleted)
            {
                observer.OnCompleted();
                Dispose();
            }
        }

        public void TryPublishOnCompleted(Result result, bool empty)
        {
            if (result.IsFailure)
            {
                observer.OnCompleted(result);
                Dispose();
            }
            else
            {
                if (empty || AllObserverIsCompleted())
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        bool AllObserverIsCompleted()
        {
            foreach (var item in observers)
            {
                if (!item.IsCompleted) return false;
            }
            return true;
        }

        public void Dispose()
        {
            foreach (var observer in observers)
            {
                observer.Dispose();
            }
        }

        sealed class ZipObserver(_Zip parent) : Observer<T>
        {
            public Queue<T> Values { get; } = new Queue<T>();
            public bool IsCompleted { get; private set; }

            public bool HasValue(out bool shouldComplete)
            {
                var count = Values.Count;
                shouldComplete = IsCompleted && count == 1;
                return count != 0;
            }

            protected override void OnNextCore(T value)
            {
                lock (parent.observers)
                {
                    this.Values.Enqueue(value);
                    parent.TryPublishOnNext();
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                parent.observer.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                lock (parent.observers)
                {
                    IsCompleted = true;
                    parent.TryPublishOnCompleted(result, Values.Count == 0);
                }
            }
        }
    }
}
