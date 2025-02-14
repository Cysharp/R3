using System.ComponentModel;

namespace R3;

public static partial class Observable
{
    public static Observable<T> Race<T>(params Observable<T>[] sources)
    {
        return new Race<T>(sources);
    }

    public static Observable<T> Race<T>(IEnumerable<Observable<T>> sources)
    {
        return new Race<T>(sources);
    }

    [Obsolete("Amb is renamed to Race.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Observable<T> Amb<T>(params Observable<T>[] sources)
    {
        return Race(sources);
    }

    [Obsolete("Amb is renamed to Race.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Observable<T> Amb<T>(IEnumerable<Observable<T>> sources)
    {
        return Race(sources);
    }
}

public static partial class ObservableExtensions
{
    public static Observable<T> Race<T>(this Observable<T> source, Observable<T> second)
    {
        return Observable.Race(source, second);
    }

    [Obsolete("Amb is renamed to Race.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Observable<T> Amb<T>(this Observable<T> source, Observable<T> second)
    {
        return Race(source, second);
    }
}

internal sealed class Race<T>(IEnumerable<Observable<T>> sources) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        if (!sources.TryGetNonEnumeratedCount(out var count))
        {
            count = 4;
        }

        var race = new _Race(observer, count);
        var index = 0;
        foreach (var item in sources)
        {
            var d = item.Subscribe(new _RaceObserver(race, index++));
            race.disposables.Add(d);
        }
        return race;
    }

    sealed class _Race : IDisposable
    {
        public Observer<T> observer;
        public ListDisposableCore disposables;

        public _RaceObserver? winner;

        public _Race(Observer<T> observer, int initialCount)
        {
            this.observer = observer;
            this.disposables = new ListDisposableCore(initialCount, this);
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }

    sealed class _RaceObserver(_Race parent, int index) : Observer<T>
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
