namespace R3;

public static partial class Observable
{
    public static Observable<T> ReturnFrame<T>(T value, CancellationToken cancellationToken = default)
    {
        return ReturnFrame(value, ObservableSystem.DefaultFrameProvider, cancellationToken);
    }

    public static Observable<T> ReturnFrame<T>(T value, FrameProvider frameProvider, CancellationToken cancellationToken = default)
    {
        return new ReturnFrame<T>(value, frameProvider, cancellationToken);
    }

    public static Observable<T> ReturnFrame<T>(T value, int dueTimeFrame, CancellationToken cancellationToken = default)
    {
        return ReturnFrame(value, dueTimeFrame, ObservableSystem.DefaultFrameProvider, cancellationToken);
    }

    public static Observable<T> ReturnFrame<T>(T value, int dueTimeFrame, FrameProvider frameProvider, CancellationToken cancellationToken = default)
    {
        return new ReturnFrameTime<T>(value, dueTimeFrame, frameProvider, cancellationToken);
    }

    public static Observable<Unit> NextFrame(CancellationToken cancellationToken = default)
    {
        return NextFrame(ObservableSystem.DefaultFrameProvider, cancellationToken);
    }

    public static Observable<Unit> NextFrame(FrameProvider frameProvider, CancellationToken cancellationToken = default)
    {
        return new NextFrame(frameProvider, cancellationToken);
    }

    // util

    public static Observable<Unit> YieldFrame(CancellationToken cancellationToken = default)
    {
        return ReturnFrame(Unit.Default, ObservableSystem.DefaultFrameProvider, cancellationToken);
    }

    public static Observable<Unit> YieldFrame(FrameProvider frameProvider, CancellationToken cancellationToken = default)
    {
        return ReturnFrame(Unit.Default, frameProvider, cancellationToken);
    }
}

internal sealed class ReturnFrame<T>(T value, FrameProvider frameProvider, CancellationToken cancellationToken) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var runner = new ReturnFrameRunnerWorkItem(value, observer, cancellationToken);
        frameProvider.Register(runner);
        return runner;
    }

    sealed class ReturnFrameRunnerWorkItem(T value, Observer<T> observer, CancellationToken cancellationToken)
        : CancellableFrameRunnerWorkItemBase<T>(observer, cancellationToken)
    {
        protected override bool MoveNextCore(long frameCount)
        {
            PublishOnNext(value);
            PublishOnCompleted();
            return false;
        }
    }
}

internal sealed class ReturnFrameTime<T>(T value, int dueTimeFrame, FrameProvider frameProvider, CancellationToken cancellationToken) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var runner = new ReturnFrameTimeRunnerWorkItem(value, dueTimeFrame.NormalizeFrame(), observer, cancellationToken);
        frameProvider.Register(runner);
        return runner;
    }

    sealed class ReturnFrameTimeRunnerWorkItem(T value, int dueTimeFrame, Observer<T> observer, CancellationToken cancellationToken)
        : CancellableFrameRunnerWorkItemBase<T>(observer, cancellationToken)
    {
        int currentFrame;

        protected override bool MoveNextCore(long frameCount)
        {
            if (++currentFrame == dueTimeFrame)
            {
                PublishOnNext(value);
                PublishOnCompleted();
                return false;
            }

            return true;
        }
    }
}

internal sealed class NextFrame(FrameProvider frameProvider, CancellationToken cancellationToken) : Observable<Unit>
{
    protected override IDisposable SubscribeCore(Observer<Unit> observer)
    {
        var runner = new NextFrameRunnerWorkItem(observer, frameProvider.GetFrameCount(), cancellationToken);
        frameProvider.Register(runner);
        return runner;
    }

    sealed class NextFrameRunnerWorkItem(Observer<Unit> observer, long startFrameCount, CancellationToken cancellationToken)
        : CancellableFrameRunnerWorkItemBase<Unit>(observer, cancellationToken)
    {
        protected override bool MoveNextCore(long frameCount)
        {
            // same frame, skip
            if (startFrameCount == frameCount)
            {
                return true;
            }

            PublishOnNext(default);
            PublishOnCompleted();
            return false;
        }
    }
}
