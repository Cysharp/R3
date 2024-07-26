using System.Diagnostics.CodeAnalysis;

namespace R3;

public static partial class Observable
{
    public static Observable<T[]> CombineLatest<T>(params Observable<T>[] sources)
    {
        return new CombineLatest<T>(sources);
    }

    public static Observable<T[]> CombineLatest<T>(IEnumerable<Observable<T>> sources)
    {
        return new CombineLatest<T>(sources);
    }
}

internal sealed class CombineLatest<T>(IEnumerable<Observable<T>> sources) : Observable<T[]>
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
        bool hasValueAll;
        int completedCount;

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
            if (!hasValueAll)
            {
                foreach (var item in observers)
                {
                    if (!item.HasValue) return;
                }
                hasValueAll = true;
            }

            var values = new T[observers.Length];
            for (int i = 0; i < observers.Length; i++)
            {
                values[i] = observers[i].Value!;
            }
            observer.OnNext(values);
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
                completedCount += 1;
                if (empty || completedCount == sources.Length)
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

        sealed class CombineLatestObserver(_CombineLatest parent) : Observer<T>
        {
            public T? Value { get; private set; }

            [MemberNotNullWhen(true, nameof(Value))]
            public bool HasValue { get; private set; }

            protected override void OnNextCore(T value)
            {
                lock (parent.observers)
                {
                    this.Value = value;
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
                lock (parent.observers)
                {
                    parent.TryPublishOnCompleted(result, !HasValue);
                }
            }
        }
    }
}
