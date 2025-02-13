﻿using System.Runtime.CompilerServices;

namespace R3;

public sealed class LogicLooperTimerProvider(LogicLooperFrameProvider frameProvider) : TimeProvider
{
    public override long GetTimestamp()
    {
        return frameProvider.timestamp;
    }

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        return new FrameTimer(callback, state, dueTime, period, frameProvider);
    }
}

internal sealed class FrameTimer : ITimer, IFrameRunnerWorkItem
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
    readonly LogicLooperFrameProvider frameProvider;
#if NET9_0_OR_GREATER
    readonly System.Threading.Lock gate = new();
#else
    readonly object gate = new object();
#endif

    TimeSpan dueTime;
    TimeSpan period;
    RunningState runningState;
    double elapsed;
    bool isDisposed;

    public FrameTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period, LogicLooperFrameProvider frameProvider)
    {
        this.callback = callback;
        this.state = state;
        this.dueTime = dueTime;
        this.period = period;
        this.frameProvider = frameProvider;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    double GetDeltaTime()
    {
        return frameProvider.deltaTime.TotalSeconds;
    }

    bool IFrameRunnerWorkItem.MoveNext(long frameCount)
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

        elapsed += GetDeltaTime();

        try
        {
            if (runState == RunningState.RunningDueTime)
            {
                var dt = (float)d.TotalSeconds;
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
                var dt = (float)p.TotalSeconds;
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

    public void Dispose()
    {
        Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        isDisposed = true;
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return default;
    }
}
