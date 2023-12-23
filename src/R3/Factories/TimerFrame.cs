namespace R3;

public static partial class Observable
{
    public static Observable<Unit> IntervalFrame(int periodFrame, CancellationToken cancellationToken = default)
    {
        return TimerFrame(periodFrame, periodFrame, cancellationToken);
    }

    public static Observable<Unit> IntervalFrame(int periodFrame, FrameProvider frameProvider, CancellationToken cancellationToken = default)
    {
        return TimerFrame(periodFrame, periodFrame, frameProvider, cancellationToken);
    }

    public static Observable<Unit> TimerFrame(int dueTimeFrame, CancellationToken cancellationToken = default)
    {
        return new TimerFrame(dueTimeFrame, null, ObservableSystem.DefaultFrameProvider, cancellationToken);
    }

    public static Observable<Unit> TimerFrame(int dueTimeFrame, int periodFrame, CancellationToken cancellationToken = default)
    {
        return new TimerFrame(dueTimeFrame, periodFrame, ObservableSystem.DefaultFrameProvider, cancellationToken);
    }

    public static Observable<Unit> TimerFrame(int dueTimeFrame, FrameProvider frameProvider, CancellationToken cancellationToken = default)
    {
        return new TimerFrame(dueTimeFrame, null, frameProvider, cancellationToken);
    }

    public static Observable<Unit> TimerFrame(int dueTimeFrame, int periodFrame, FrameProvider frameProvider, CancellationToken cancellationToken = default)
    {
        return new TimerFrame(dueTimeFrame, periodFrame, frameProvider, cancellationToken);
    }
}

internal sealed class TimerFrame(int dueTimeFrame, int? periodFrame, FrameProvider frameProvider, CancellationToken cancellationToken) : Observable<Unit>
{
    protected override IDisposable SubscribeCore(Observer<Unit> observer)
    {
        dueTimeFrame = dueTimeFrame.NormalizeFrame();
        periodFrame = periodFrame?.NormalizeFrame();

        CancellableFrameRunnerWorkItemBase<Unit> runner = (periodFrame == null)
            ? new SingleTimerFrameRunnerWorkItem(dueTimeFrame, observer, cancellationToken)
            : new MultiTimerFrameRunnerWorkItem(dueTimeFrame, periodFrame.Value, observer, cancellationToken);
        frameProvider.Register(runner);
        return runner;
    }

    sealed class SingleTimerFrameRunnerWorkItem(int dueTimeFrame, Observer<Unit> observer, CancellationToken cancellationToken)
        : CancellableFrameRunnerWorkItemBase<Unit>(observer, cancellationToken)
    {
        int currentFrame;

        protected override bool MoveNextCore(long _)
        {
            if (++currentFrame == dueTimeFrame)
            {
                PublishOnNext(default);
                PublishOnCompleted();
                return false;
            }

            return true;
        }
    }

    sealed class MultiTimerFrameRunnerWorkItem(int dueTimeFrame, int periodFrame, Observer<Unit> observer, CancellationToken cancellationToken)
        : CancellableFrameRunnerWorkItemBase<Unit>(observer, cancellationToken)
    {
        int currentFrame;
        bool isPeriodPhase;

        protected override bool MoveNextCore(long _)
        {
            // initial phase
            if (!isPeriodPhase)
            {
                if (++currentFrame == dueTimeFrame)
                {
                    PublishOnNext(default);
                    isPeriodPhase = true;
                    currentFrame = 0;
                }
                return true;
            }

            // period phase
            if (++currentFrame == periodFrame)
            {
                PublishOnNext(default);
                currentFrame = 0;
            }
            return true;
        }
    }
}
