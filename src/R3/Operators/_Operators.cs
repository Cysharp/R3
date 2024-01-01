namespace R3;

public static partial class ObservableExtensions
{
    // TODO: this is working space, will remove this file after complete.


    // AsUnitObservable

    // Time based
    // Debounce, Throttle, ThrottleFirst, Sample, Delay, DelaySubscription, Timeout
    // + frame variation

    // TImeInterval <-> FrameInterval

    // Buffer + BUfferFrame => Chunk, ChunkFrame

    // SubscribeOn, Synchronize

    // Rx Merging:
    // CombineLatest, Zip, WithLatestFrom, ZipLatest, Switch

    // Standard Query:
    // Distinct, DistinctBy, DistinctUntilChanged, Scan, DefaultIfEmpty

    // SkipTake:
    // Skip, SkipLast, SkipUntil, SkipWhile

    // return tasks:
    // All, Any, Contains, SequenceEqual, IsEmpty, MaxBy, MinBy, ToDictionary, ToLookup,

    // Multicast
    // Multicast, Publish, Replay, RefCount, Share(Publish().RefCount())

    public static ConnectableObservable<T> Multicast<T>(this Observable<T> source, ISubject<T> subject)
    {
        return new Multicast<T>(source, subject);
    }

    public static ConnectableObservable<T> Publish<T>(this Observable<T> source)
    {
        return source.Multicast(new Subject<T>());
    }

    public static ConnectableObservable<T> Publish<T>(this Observable<T> source, T initialValue)
    {
        return source.Multicast(new ReactiveProperty<T>(initialValue, equalityComparer: null));
    }

    public static ConnectableObservable<T> Replay<T>(this Observable<T> source)
    {
        return source.Multicast(new ReplaySubject<T>());
    }

    public static ConnectableObservable<T> Replay<T>(this Observable<T> source, int bufferSize)
    {
        return source.Multicast(new ReplaySubject<T>(bufferSize));
    }

    public static ConnectableObservable<T> Replay<T>(this Observable<T> source, TimeSpan window)
    {
        return source.Multicast(new ReplaySubject<T>(window));
    }

    public static ConnectableObservable<T> Replay<T>(this Observable<T> source, TimeSpan window, TimeProvider timeProvider)
    {
        return source.Multicast(new ReplaySubject<T>(window, timeProvider));
    }

    public static ConnectableObservable<T> Replay<T>(this Observable<T> source, int bufferSize, TimeSpan window)
    {
        return source.Multicast(new ReplaySubject<T>(bufferSize, window));
    }

    public static ConnectableObservable<T> Replay<T>(this Observable<T> source, int bufferSize, TimeSpan window, TimeProvider timeProvider)
    {
        return source.Multicast(new ReplaySubject<T>(bufferSize, window, timeProvider));
    }

    public static Observable<T> RefCount<T>(this ConnectableObservable<T> source)
    {
        return new RefCount<T>(source);
    }

    public static Observable<T> Share<T>(this Observable<T> source)
    {
        return source.Publish().RefCount();
    }
}

public abstract class ConnectableObservable<T> : Observable<T>
{
    public abstract IDisposable Connect();
}

internal sealed class Multicast<T>(Observable<T> source, ISubject<T> subject) : ConnectableObservable<T>
{
    readonly object gate = new object();
    Connection? connection;

    public override IDisposable Connect()
    {
        lock (gate)
        {
            if (connection == null)
            {
                var subscription = source.Subscribe(subject.AsObserver());
                connection = new Connection(this, subscription);
            }

            return connection;
        }
    }

    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return subject.Subscribe(observer);
    }

    sealed class Connection(Multicast<T> parent, IDisposable? subscription) : IDisposable
    {
        public void Dispose()
        {
            lock (parent.gate)
            {
                if (subscription != null)
                {
                    subscription.Dispose();
                    subscription = null;
                    parent.connection = null;
                }
            }
        }
    }
}

internal sealed class RefCount<T>(ConnectableObservable<T> source) : Observable<T>
{
    readonly object gate = new object();
    int refCount = 0;
    IDisposable? connection;

    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        lock (gate)
        {
            var subcription = source.Subscribe(new _RefCount(this, observer));
            if (++refCount == 1)
            {
                connection = source.Connect();
            }
            return subcription;
        }
    }

    sealed class _RefCount(RefCount<T> parent, Observer<T> observer) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            observer.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            observer.OnCompleted(result);
        }

        protected override void DisposeCore()
        {
            lock (parent.gate)
            {
                if (--parent.refCount == 0)
                {
                    parent.connection?.Dispose();
                    parent.connection = null;
                }
            }
        }
    }
}
