namespace R3;

public static partial class Observable
{
    public static Observable<T[]> ZipLatest<T>(params Observable<T>[] sources)
    {
        return new ZipLatest<T>(sources);
    }

    public static Observable<T[]> ZipLatest<T>(IEnumerable<Observable<T>> sources)
    {
        return new ZipLatest<T>(sources);
    }
}

internal sealed class ZipLatest<T>(IEnumerable<Observable<T>> sources) : Observable<T[]>
{
    protected override IDisposable SubscribeCore(Observer<T[]> observer)
    {
        return new _CombineLatest(observer, sources).Run();
    }

    sealed class _CombineLatest : IDisposable
    {
        readonly Observer<T[]> observer;
        readonly Observable<T>[] sources;
        readonly CombineLatestObserver[] observers;

        public _CombineLatest(Observer<T[]> observer, IEnumerable<Observable<T>> sources)
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

            var observers = new CombineLatestObserver[this.sources.Length];
            for (int i = 0; i < observers.Length; i++)
            {
                observers[i] = new CombineLatestObserver(this);
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
            var hasCompletedObserver = false;
            foreach (var item in observers)
            {
                if (!item.HasValue) return;
                if (item.IsCompleted)
                {
                    hasCompletedObserver = true;
                }
            }

            var values = new T[observers.Length];
            for (int i = 0; i < observers.Length; i++)
            {
                values[i] = observers[i].GetValue();
            }
            observer.OnNext(values);

            if (hasCompletedObserver)
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

        sealed class CombineLatestObserver(_CombineLatest parent) : Observer<T>
        {
            T? value;
            public bool HasValue { get; private set; }
            public bool IsCompleted { get; private set; }

            public T GetValue()
            {
                var v = this.value;
                this.value = default;
                this.HasValue = false;
                return v!;
            }

            protected override void OnNextCore(T value)
            {
                lock (parent.observers)
                {
                    this.value = value;
                    this.HasValue = true;
                    parent.TryPublishOnNext();
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                parent.observer.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                lock (parent.observer)
                {
                    IsCompleted = true;
                    parent.TryPublishOnCompleted(result, !HasValue);
                }
            }
        }
    }
}
