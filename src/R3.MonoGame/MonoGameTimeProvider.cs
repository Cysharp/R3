using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using R3.Collections;

namespace R3;

public class MonoGameTimeProvider : TimeProvider, IDisposable
{
    public static readonly MonoGameTimeProvider Update = new();

    public GameTime GameTime { get; private set; } = new();

    FreeListCore<FrameTimer> list;
    readonly object gate = new();

    // frame loop is delayed until first register
    bool running;
    bool disposed;

    public MonoGameTimeProvider()
    {
        list = new FreeListCore<FrameTimer>(gate);
    }

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        return new FrameTimer(callback, state, dueTime, period, this);
    }

    public override long GetTimestamp()
    {
        return GameTime.TotalGameTime.Ticks;
    }

    public void Dispose()
    {
        lock (gate)
        {
            disposed = true;
            list.Dispose();
        }
    }

    public void Tick(GameTime gameTime)
    {
        if (!running) return;

        this.GameTime = gameTime;

        var span = list.AsSpan();
        for (var i = 0; i < span.Length; i++)
        {
            ref readonly var item = ref span[i];
            if (item != null)
            {
                try
                {
                    if (!item.Tick(gameTime))
                    {
                        list.Remove(i);
                    }
                }
                catch (Exception ex)
                {
                    list.Remove(i);
                    try
                    {
                        ObservableSystem.GetUnhandledExceptionHandler().Invoke(ex);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }
    }

    internal void Register(FrameTimer timer)
    {
        ThrowIfDisposed();
        lock (gate)
        {
            running = true;
            list.Add(timer, out _);
        }
    }

    void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(typeof(MonoGameFrameProvider).FullName);
        }
    }
}

internal sealed class FrameTimer : ITimer
{
    enum RunningState
    {
        Stop,
        RunningDueTime,
        RunningPeriod,
        ChangeRequested
    }

    readonly MonoGameTimeProvider timeProvider;
    readonly TimerCallback callback;
    readonly object? state;
    readonly object gate = new();

    TimeSpan dueTime;
    TimeSpan period;
    RunningState runningState;
    double elapsed;
    bool isDisposed;

    public FrameTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period, MonoGameTimeProvider timeProvider)
    {
        this.callback = callback;
        this.state = state;
        this.dueTime = dueTime;
        this.period = period;
        this.timeProvider = timeProvider;
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
                timeProvider.Register(this);
            }

            runningState = RunningState.ChangeRequested;
        }
        return true;
    }

    public bool Tick(GameTime gameTime)
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

        elapsed += gameTime.ElapsedGameTime.TotalSeconds; // ElapsedGameTime is delta time

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
                    return ChangeState(RunningState.RunningPeriod);
                }
                return true;
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

