namespace R3;

public static partial class Observable
{
    public static Observable<Unit> EveryUpdate()
    {
        return new EveryUpdate(ObservableSystem.DefaultFrameProvider, CancellationToken.None, cancelImmediately: false);
    }

    public static Observable<Unit> EveryUpdate(CancellationToken cancellationToken)
    {
        return new EveryUpdate(ObservableSystem.DefaultFrameProvider, cancellationToken, cancelImmediately: false);
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
    protected override IDisposable SubscribeCore(Observer<Unit> observer)
    {
        var runner = new EveryUpdateRunnerWorkItem(observer, cancellationToken, cancelImmediately);
        frameProvider.Register(runner);
        return runner;
    }

    class EveryUpdateRunnerWorkItem : IFrameRunnerWorkItem, IDisposable
    {
        Observer<Unit> observer;
        CancellationToken cancellationToken;
        CancellationTokenRegistration cancellationTokenRegistration;
        bool isDisposed;

        public EveryUpdateRunnerWorkItem(Observer<Unit> observer, CancellationToken cancellationToken, bool cancelImmediately)
        {
            this.observer = observer;
            this.cancellationToken = cancellationToken;

            if (cancelImmediately && cancellationToken.CanBeCanceled)
            {
                cancellationTokenRegistration = cancellationToken.UnsafeRegister(static state =>
                {
                    var s = (EveryUpdateRunnerWorkItem)state!;
                    s.observer.OnCompleted();
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
                observer.OnCompleted();
                Dispose();
                return false;
            }

            observer.OnNext(default);
            return true;
        }

        public void Dispose()
        {
            isDisposed = true;
            cancellationTokenRegistration.Dispose();
        }
    }
}
