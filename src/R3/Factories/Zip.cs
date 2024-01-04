namespace R3;

public static partial class Observable
{
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
        readonly ZipObserver[] observers;
        int completedCount;

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
            foreach (var item in observers)
            {
                if (!item.HasValue) return;
            }

            var values = new T[observers.Length];
            for (int i = 0; i < observers.Length; i++)
            {
                values[i] = observers[i].Values.Dequeue();
            }
            observer.OnNext(values);
        }

        public void TryPublishOnCompleted(Result result)
        {
            if (result.IsFailure)
            {
                observer.OnCompleted(result);
                Dispose();
            }
            else
            {
                if (Interlocked.Increment(ref completedCount) == sources.Length)
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
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
            public Queue<T> Values { get; private set; } = new Queue<T>();
            public bool HasValue => Values.Count != 0;

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
                parent.TryPublishOnCompleted(result);
            }
        }
    }
}
