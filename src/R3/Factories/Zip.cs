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
