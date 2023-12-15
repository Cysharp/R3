namespace R3;

public static partial class Observable
{
    public static Observable<Unit> EveryUpdate()
    {
        return new EveryUpdate(ObservableSystem.DefaultFrameProvider, CancellationToken.None);
    }

    public static Observable<Unit> EveryUpdate(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return Empty<Unit>();
        return new EveryUpdate(ObservableSystem.DefaultFrameProvider, cancellationToken);
    }

    public static Observable<Unit> EveryUpdate(FrameProvider frameProvider)
    {
        return new EveryUpdate(frameProvider, CancellationToken.None);
    }

    public static Observable<Unit> EveryUpdate(FrameProvider frameProvider, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return Empty<Unit>();

        return new EveryUpdate(frameProvider, cancellationToken);
    }
}


internal sealed class EveryUpdate(FrameProvider frameProvider, CancellationToken cancellationToken) : Observable<Unit>
{
    protected override IDisposable SubscribeCore(Observer<Unit> observer)
    {
        var runner = new EveryUpdateRunnerWorkItem(observer, cancellationToken);
        frameProvider.Register(runner);
        return runner;
    }

    class EveryUpdateRunnerWorkItem : IFrameRunnerWorkItem, IDisposable
    {
        Observer<Unit> observer;
        CancellationToken cancellationToken;
        CancellationTokenRegistration cancellationTokenRegistration;
        bool isDisposed;

        public EveryUpdateRunnerWorkItem(Observer<Unit> observer, CancellationToken cancellationToken)
        {
            this.observer = observer;
            this.cancellationToken = cancellationToken;

            if (cancellationToken.CanBeCanceled)
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
