using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace R3
{
    public enum TimeKind
    {
        /// <summary>use Time.time, Time.deltaTime or Time.fixedTime, Time.fixedDeltaTime.</summary>
        Time,
        /// <summary>Ignore timescale, use Time.unscaledTime, Time.unscaledDeltaTime or Time.fixedUnscaledTime, Time.fixedUnscaledDeltaTime.</summary>
        UnscaledTime,
        /// <summary>use Time.realtimeSinceStartup, TimeProvider.System.GetTimestamp()<summary>
        Realtime
    }

    public class UnityTimeProvider : TimeProvider
    {
        public static readonly TimeProvider Initialization = new UnityTimeProvider(UnityFrameProvider.Initialization, TimeKind.Time);
        public static readonly TimeProvider EarlyUpdate = new UnityTimeProvider(UnityFrameProvider.EarlyUpdate, TimeKind.Time);
        public static readonly TimeProvider FixedUpdate = new UnityTimeProvider(UnityFrameProvider.FixedUpdate, TimeKind.Time);
        public static readonly TimeProvider PreUpdate = new UnityTimeProvider(UnityFrameProvider.PreUpdate, TimeKind.Time);
        public static readonly TimeProvider Update = new UnityTimeProvider(UnityFrameProvider.Update, TimeKind.Time);
        public static readonly TimeProvider PreLateUpdate = new UnityTimeProvider(UnityFrameProvider.PreLateUpdate, TimeKind.Time);
        public static readonly TimeProvider PostLateUpdate = new UnityTimeProvider(UnityFrameProvider.PostLateUpdate, TimeKind.Time);
        public static readonly TimeProvider TimeUpdate = new UnityTimeProvider(UnityFrameProvider.TimeUpdate, TimeKind.Time);

        public static readonly TimeProvider InitializationIgnoreTimeScale = new UnityTimeProvider(UnityFrameProvider.Initialization, TimeKind.UnscaledTime);
        public static readonly TimeProvider EarlyUpdateIgnoreTimeScale = new UnityTimeProvider(UnityFrameProvider.EarlyUpdate, TimeKind.UnscaledTime);
        public static readonly TimeProvider FixedUpdateIgnoreTimeScale = new UnityTimeProvider(UnityFrameProvider.FixedUpdate, TimeKind.UnscaledTime);
        public static readonly TimeProvider PreUpdateIgnoreTimeScale = new UnityTimeProvider(UnityFrameProvider.PreUpdate, TimeKind.UnscaledTime);
        public static readonly TimeProvider UpdateIgnoreTimeScale = new UnityTimeProvider(UnityFrameProvider.Update, TimeKind.UnscaledTime);
        public static readonly TimeProvider PreLateUpdateIgnoreTimeScale = new UnityTimeProvider(UnityFrameProvider.PreLateUpdate, TimeKind.UnscaledTime);
        public static readonly TimeProvider PostLateUpdateIgnoreTimeScale = new UnityTimeProvider(UnityFrameProvider.PostLateUpdate, TimeKind.UnscaledTime);
        public static readonly TimeProvider TimeUpdateIgnoreTimeScale = new UnityTimeProvider(UnityFrameProvider.TimeUpdate, TimeKind.UnscaledTime);

        public static readonly TimeProvider InitializationRealtime = new UnityTimeProvider(UnityFrameProvider.Initialization, TimeKind.Realtime);
        public static readonly TimeProvider EarlyUpdateRealtime = new UnityTimeProvider(UnityFrameProvider.EarlyUpdate, TimeKind.Realtime);
        public static readonly TimeProvider FixedUpdateRealtime = new UnityTimeProvider(UnityFrameProvider.FixedUpdate, TimeKind.Realtime);
        public static readonly TimeProvider PreUpdateRealtime = new UnityTimeProvider(UnityFrameProvider.PreUpdate, TimeKind.Realtime);
        public static readonly TimeProvider UpdateRealtime = new UnityTimeProvider(UnityFrameProvider.Update, TimeKind.Realtime);
        public static readonly TimeProvider PreLateUpdateRealtime = new UnityTimeProvider(UnityFrameProvider.PreLateUpdate, TimeKind.Realtime);
        public static readonly TimeProvider PostLateUpdateRealtime = new UnityTimeProvider(UnityFrameProvider.PostLateUpdate, TimeKind.Realtime);
        public static readonly TimeProvider TimeUpdateRealtime = new UnityTimeProvider(UnityFrameProvider.TimeUpdate, TimeKind.Realtime);

        readonly UnityFrameProvider frameProvider;
        readonly TimeKind timeKind;

        UnityTimeProvider(FrameProvider frameProvider, TimeKind timeKind)
        {
            this.frameProvider = (UnityFrameProvider)frameProvider;
            this.timeKind = timeKind;
        }

        public override long GetTimestamp()
        {
            // need to convert ticks(used TimeSpan ctor)
            if (frameProvider.PlayerLoopTiming == PlayerLoopTiming.FixedUpdate)
            {
                switch (timeKind)
                {
                    case TimeKind.Time:
                        return TimeSpan.FromSeconds(Time.fixedTimeAsDouble).Ticks;
                    case TimeKind.UnscaledTime:
                        return TimeSpan.FromSeconds(Time.fixedUnscaledTimeAsDouble).Ticks;
                    default:
                        break;
                }
            }
            else
            {
                switch (timeKind)
                {
                    case TimeKind.Time:
                        return TimeSpan.FromSeconds(Time.timeAsDouble).Ticks;
                    case TimeKind.UnscaledTime:
                        return TimeSpan.FromSeconds(Time.unscaledTimeAsDouble).Ticks;
                    default:
                        break;
                }
            }

            return TimeSpan.FromSeconds(Time.realtimeSinceStartupAsDouble).Ticks;
        }

        public override ITimer CreateTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            return new FrameTimer(callback, state, dueTime, period, frameProvider, timeKind);
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
        readonly object state;
        readonly UnityFrameProvider frameProvider;
        readonly TimeKind timeKind;
        readonly object gate = new object();

        TimeSpan dueTime;
        TimeSpan period;
        RunningState runningState;
        float elapsed;
        bool isDisposed;

        // for DeltaType.Realtime
        long lastTimestamp;

        public FrameTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period, UnityFrameProvider frameProvider, TimeKind timeKind)
        {
            this.callback = callback;
            this.state = state;
            this.dueTime = dueTime;
            this.period = period;
            this.frameProvider = frameProvider;
            this.timeKind = timeKind;
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
        float GetDeltaTime()
        {
            if (frameProvider.PlayerLoopTiming == PlayerLoopTiming.FixedUpdate)
            {
                switch (timeKind)
                {
                    case TimeKind.Time:
                        return Time.fixedDeltaTime;
                    case TimeKind.UnscaledTime:
                        return Time.fixedUnscaledDeltaTime;
                    default:
                        break;
                }
            }
            else
            {
                switch (timeKind)
                {
                    case TimeKind.Time:
                        return Time.deltaTime;
                    case TimeKind.UnscaledTime:
                        return Time.unscaledDeltaTime;
                    default:
                        break;
                }
            }

            // DelayType.Realtime
            var current = TimeProvider.System.GetTimestamp();
            var elapsed = TimeProvider.System.GetElapsedTime(lastTimestamp, current);
            lastTimestamp = current;
            return (float)elapsed.TotalSeconds;
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
}
