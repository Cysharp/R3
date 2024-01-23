using Stride.Games;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace R3;

public sealed class StrideTimeProvider: TimeProvider
{
    readonly StrideFrameProvider frameProvider;

    internal StrideTimeProvider(StrideFrameProvider frameProvider)
    {
        this.frameProvider = frameProvider;
    }

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        return new StrideFrameTimer(callback, state, dueTime, period, frameProvider);
    }
}
internal sealed class StrideFrameTimer : ITimer, IFrameRunnerWorkItem
{
    enum RunningState
    {
        Stop,
        RunningDueTime,
        RunningPeriod,
        ChangeRequested
    }
    readonly TimerCallback callback;
    readonly object? state;
    TimeSpan dueTime;
    TimeSpan period;
    bool isDisposed;
    readonly StrideFrameProvider frameProvider;
    readonly object gate = new object();
    RunningState runningState;
    double elapsed;


    public StrideFrameTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period, StrideFrameProvider frameProvider)
    {
        this.callback = callback;
        this.state = state;
        this.dueTime = dueTime;
        this.period = period;
        runningState = RunningState.Stop;
        isDisposed = false;
        this.frameProvider = frameProvider;
        elapsed = 0;
        Change(dueTime, period);
    }

    public bool Change(TimeSpan dueTime, TimeSpan period)
    {
        if (isDisposed) return false;

        lock (gate)
        {
            this.dueTime = dueTime;
            this.period = period;

            if (dueTime == Timeout.InfiniteTimeSpan)
            {
                if (runningState == RunningState.Stop)
                {
                    return true;
                }
            }

            if (runningState == RunningState.Stop)
            {
                frameProvider.Register(this);
            }

            runningState = RunningState.ChangeRequested;
        }
        return true;
    }

    public void Dispose()
    {
        Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        isDisposed = true;
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    public bool MoveNext(long frameCount)
    {
        if (isDisposed) return false;

        RunningState runState;
        TimeSpan p; // period
        TimeSpan d; // dueTime
        lock (gate)
        {
            runState = runningState;

            if (runState == RunningState.ChangeRequested)
            {
                elapsed = 0;
                if (dueTime == Timeout.InfiniteTimeSpan)
                {
                    runningState = RunningState.Stop;
                    return false;
                }

                runState = runningState = RunningState.RunningDueTime;
            }
            p = period;
            d = dueTime;
        }

        elapsed += frameProvider.Delta.Value;

        try
        {
            if (runState == RunningState.RunningDueTime)
            {
                var dt = (double)d.TotalSeconds;
                if (elapsed >= dt)
                {
                    callback(state);

                    elapsed = 0;
                    if (period == Timeout.InfiniteTimeSpan)
                    {
                        return ChangeState(RunningState.Stop);
                    }
                    else
                    {
                        return ChangeState(RunningState.RunningPeriod);
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                var dt = (double)p.TotalSeconds;
                if (elapsed >= dt)
                {
                    callback(state);
                    elapsed = 0;
                }

                return ChangeState(RunningState.RunningPeriod);
            }
        }
        catch (Exception ex)
        {
            ObservableSystem.GetUnhandledExceptionHandler().Invoke(ex);
            return ChangeState(RunningState.Stop);
        }
    }
    bool ChangeState(RunningState state)
    {
        lock (gate)
        {
            // change requested is high priority
            if (runningState == RunningState.ChangeRequested)
            {
                return true;
            }

            switch (state)
            {
                case RunningState.RunningPeriod:
                    runningState = state;
                    return true;
                default: // otherwise(Stop)
                    runningState = state;
                    return false;
            }
        }
    }
}
