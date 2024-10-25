﻿namespace R3;

public static partial class Observable
{
    /// <summary>
    ///   <para>
    ///     Given two or more source <see cref="Observable"/>s, emit all of the items from only the first of these <see cref="Observable"/> to emit an item or notification.
    ///   </para>
    ///   <para>
    ///     ReactiveX docs link: <see href="https://reactivex.io/documentation/operators/amb.html" />
    ///   </para>
    ///   <para>
    ///     In the following examples each column represent single time tick.
    ///     "--" means no emission on this tick.
    ///   </para>
    ///   <example>
    ///     <para>Example 1:</para>
    ///     <code>
    /// Number:      1  2  3  4  5  6  7  8  9 10
    /// Sequence 1: -- -- -- 20 -- 40 -- 60 -- -->
    /// Sequence 2: -- 01 -- 02 -- 03 -- -- -- -->
    /// Sequence 3: -- -- -- -- 00 -- 00 -- 00 -->
    /// 
    /// Results:    -- 01 -- 02 -- 03 -- -- -- -->
    ///     </code>
    ///   </example>
    ///   <example>
    ///     <para>Example 2:</para>
    ///     <code>
    ///  Number:      1  2  3  4  5  6  7  8  9
    ///  Sequence 1: -- -- -- 20 -- 40 -- 60 -->
    ///  Sequence 2: -- -- -- 01 -- 02 -- 03 -->
    ///  Sequence 3: -- 00 -- 00 -- 00 -- -- -->
    ///  
    ///  Results:    -- 00 -- 00 -- 00 -- -- -->
    ///     </code>
    ///   </example>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sources"></param>
    /// <returns></returns>
    public static Observable<T> Amb<T>(params Observable<T>[] sources)
    {
        return new Amb<T>(sources);
    }

    public static Observable<T> Amb<T>(IEnumerable<Observable<T>> sources)
    {
        return new Amb<T>(sources);
    }
}

public static partial class ObservableExtensions
{
    public static Observable<T> Amb<T>(this Observable<T> source, Observable<T> second)
    {
        return Observable.Amb(source, second);
    }
}

internal sealed class Amb<T>(IEnumerable<Observable<T>> sources) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        if (!sources.TryGetNonEnumeratedCount(out var count))
        {
            count = 4;
        }

        var amb = new _Amb(observer, count);
        var index = 0;
        foreach (var item in sources)
        {
            var d = item.Subscribe(new _AmbObserver(amb, index++));
            amb.disposables.Add(d);
        }
        return amb;
    }

    sealed class _Amb : IDisposable
    {
        public Observer<T> observer;
        public ListDisposableCore disposables;

        public _AmbObserver? winner;

        public _Amb(Observer<T> observer, int initialCount)
        {
            this.observer = observer;
            this.disposables = new ListDisposableCore(initialCount, this);
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }

    sealed class _AmbObserver(_Amb parent, int index) : Observer<T>
    {
        bool won;

        protected override void OnNextCore(T value)
        {
            if (won)
            {
                parent.observer.OnNext(value);
                return;
            }

            var field = Interlocked.CompareExchange(ref parent.winner, this, null);
            if (field == null)
            {
                // first, dispose others.
                won = true;
                parent.disposables.RemoveAllExceptAt(index);
                parent.observer.OnNext(value);
            }
            else if (field == this)
            {
                parent.observer.OnNext(value);
            }
            else
            {
                // dispose self.
                Dispose();
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            if (won)
            {
                parent.observer.OnErrorResume(error);
                return;
            }

            var field = Interlocked.CompareExchange(ref parent.winner, this, null);
            if (field == null)
            {
                // first, dispose others.
                won = true;
                parent.disposables.RemoveAllExceptAt(index);
                parent.observer.OnErrorResume(error);
            }
            else if (field == this)
            {
                parent.observer.OnErrorResume(error);
            }
            else
            {
                // dispose self.
                Dispose();
            }
        }

        protected override void OnCompletedCore(Result result)
        {
            if (won)
            {
                parent.observer.OnCompleted(result);
                return;
            }

            var field = Interlocked.CompareExchange(ref parent.winner, this, null);
            if (field == null)
            {
                // first, dispose others.
                won = true;
                parent.disposables.RemoveAllExceptAt(index);
                parent.observer.OnCompleted(result);
            }
            else if (field == this)
            {
                parent.observer.OnCompleted(result);
            }
            else
            {
                // dispose self.
                Dispose();
            }
        }

        protected override void DisposeCore()
        {
            parent.disposables.RemoveAt(index);
        }
    }
}
