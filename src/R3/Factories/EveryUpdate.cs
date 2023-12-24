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

    sealed class EveryUpdateRunnerWorkItem(Observer<Unit> observer, CancellationToken cancellationToken)
        : CancellableFrameRunnerWorkItemBase<Unit>(observer, cancellationToken)
    {
        protected override bool MoveNextCore(long _)
        {
            PublishOnNext(default);
            return true;
        }
    }
}
