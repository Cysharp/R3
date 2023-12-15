namespace R3;

public static partial class Observable
{
    public static Observable<Unit> EveryUpdate()
    {
        return new EveryUpdate(EventSystem.DefaultFrameProvider, CancellationToken.None, cancelImmediately: false);
    }

    public static Observable<Unit> EveryUpdate(CancellationToken cancellationToken)
    {
        return new EveryUpdate(EventSystem.DefaultFrameProvider, cancellationToken, cancelImmediately: false);
    }

    public static Observable<Unit> EveryUpdate(FrameProvider frameProvider)
    {
        return new EveryUpdate(frameProvider, CancellationToken.None, cancelImmediately: false);
    }

    public static Observable<Unit> EveryUpdate(FrameProvider frameProvider, CancellationToken cancellationToken)
    {
        return new EveryUpdate(frameProvider, cancellationToken, cancelImmediately: false);
    }

    public static Observable<Unit> EveryUpdate(FrameProvider frameProvider, CancellationToken cancellationToken, bool cancelImmediately)
    {
        return new EveryUpdate(frameProvider, cancellationToken, cancelImmediately: cancelImmediately);
    }
}


internal sealed class EveryUpdate(FrameProvider frameProvider, CancellationToken cancellationToken, bool cancelImmediately) : Observable<Unit>
{
    protected override IDisposable SubscribeCore(Observer<Unit> subscriber)
    {
        var runner = new EveryUpdateRunnerWorkItem(subscriber, cancellationToken, cancelImmediately);
        frameProvider.Register(runner);
        return runner;
    }

    class EveryUpdateRunnerWorkItem : IFrameRunnerWorkItem, IDisposable
    {
        Observer<Unit> subscriber;
        CancellationToken cancellationToken;
        CancellationTokenRegistration cancellationTokenRegistration;
        bool isDisposed;

        public EveryUpdateRunnerWorkItem(Observer<Unit> subscriber, CancellationToken cancellationToken, bool cancelImmediately)
        {
            this.subscriber = subscriber;
            this.cancellationToken = cancellationToken;

            if (cancelImmediately && cancellationToken.CanBeCanceled)
            {
                cancellationTokenRegistration = cancellationToken.UnsafeRegister(static state =>
                {
                    var s = (EveryUpdateRunnerWorkItem)state!;
                    s.subscriber.OnCompleted();
                    s.Dispose();
                }, this);
            }
        }

        public bool MoveNext(long frameCount)
        {
            if (isDisposed)
            {
                return false;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                subscriber.OnCompleted();
                Dispose();
                return false;
            }

            subscriber.OnNext(default);
            return true;
        }

        public void Dispose()
        {
            isDisposed = true;
            cancellationTokenRegistration.Dispose();
        }
    }
}
