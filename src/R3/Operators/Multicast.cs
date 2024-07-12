namespace R3;

public static partial class ObservableExtensions
{
    // Multicast, Publish, Replay, ReplayFrame, Share

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
        return source.Multicast(new BehaviorSubject<T>(initialValue));
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

    public static ConnectableObservable<T> ReplayFrame<T>(this Observable<T> source, int window)
    {
        return source.Multicast(new ReplayFrameSubject<T>(window));
    }

    public static ConnectableObservable<T> ReplayFrame<T>(this Observable<T> source, int window, FrameProvider frameProvider)
    {
        return source.Multicast(new ReplayFrameSubject<T>(window, frameProvider));
    }

    public static ConnectableObservable<T> ReplayFrame<T>(this Observable<T> source, int bufferSize, int window)
    {
        return source.Multicast(new ReplayFrameSubject<T>(bufferSize, window));
    }

    public static ConnectableObservable<T> ReplayFrame<T>(this Observable<T> source, int bufferSize, int window, FrameProvider frameProvider)
    {
        return source.Multicast(new ReplayFrameSubject<T>(bufferSize, window, frameProvider));
    }

    public static Observable<T> Share<T>(this Observable<T> source)
    {
        return source.Publish().RefCount();
    }
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
        return subject.Subscribe(observer.Wrap());
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
