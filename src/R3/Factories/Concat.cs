namespace R3;

public static partial class Observable
{
    public static Observable<T> Concat<T>(params Observable<T>[] sources)
    {
        return new Concat<T>(sources);
    }

    public static Observable<T> Concat<T>(IEnumerable<Observable<T>> sources)
    {
        return new Concat<T>(sources);
    }
}

public static partial class ObservableExtensions
{
    public static Observable<T> Concat<T>(this Observable<T> source, Observable<T> second)
    {
        return new Concat<T>(new[] { source, second });
    }
}

internal sealed class Concat<T>(IEnumerable<Observable<T>> sources) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return new _Concat(observer, sources).Run();
    }

    sealed class _Concat : IDisposable
    {
        public Observer<T> observer;
        public IEnumerator<Observable<T>> enumerator;
        public SerialDisposableCore disposable;

        public _Concat(Observer<T> observer, IEnumerable<Observable<T>> sources)
        {
            this.observer = observer;
            this.enumerator = sources.GetEnumerator();
        }

        public IDisposable Run()
        {
            if (!enumerator.MoveNext())
            {
                observer.OnCompleted();
                enumerator.Dispose();
                return Disposable.Empty;
            }
            else
            {
                disposable.Disposable = enumerator.Current.Subscribe(new _ConcatObserver(this));
                return this;
            }
        }

        public void Dispose()
        {
            enumerator.Dispose();
            disposable.Dispose();
        }
    }

    sealed class _ConcatObserver(_Concat parent) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            parent.observer.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            parent.observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            if (result.IsFailure)
            {
                try
                {
                    parent.observer.OnCompleted(result);
                }
                finally
                {
                    Dispose();
                }
            }
            else
            {
                if (parent.enumerator.MoveNext())
                {
                    parent.disposable.Disposable = parent.enumerator.Current.Subscribe(new _ConcatObserver(parent));
                }
                else
                {
                    parent.observer.OnCompleted();
                }
            }
        }
    }
}
