using Microsoft.UI.Xaml;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace R3;

public sealed class WinUI3DispatcherTimeProvider : TimeProvider
{
    public static readonly TimeProvider Default = new WinUI3DispatcherTimeProvider();

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        return new WinUI3DispatcherTimeProviderTimer(callback, state, dueTime, period);
    }
}

internal sealed class WinUI3DispatcherTimeProviderTimer : ITimer
{
    DispatcherTimer? timer;
    TimerCallback callback;
    object? state;
    EventHandler<object> timerTick;
    TimeSpan? period;
    short timerId;

    public WinUI3DispatcherTimeProviderTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        this.timerTick = Timer_Tick;
        this.callback = callback;
        this.state = state;
        this.timer = new DispatcherTimer();

        timer.Tick += timerTick;

        if (dueTime != Timeout.InfiniteTimeSpan)
        {
            Change(dueTime, period);
        }
    }

    public bool Change(TimeSpan dueTime, TimeSpan period)
    {
        if (timer != null)
        {
            this.period = period;
            timer.Interval = dueTime;

            // when start, change timerId.
            unchecked { timerId++; }
            timer.Start();
            return true;
        }
        return false;
    }

    void Timer_Tick(object? sender, object e)
    {
        var id = timerId;
        callback(state);
        if (id != timerId)
        {
            // called new timer status, do nothing.
            return;
        }

        if (timer != null && period != null)
        {
            if (period.Value == Timeout.InfiniteTimeSpan)
            {
                period = null;
                unchecked { timerId++; }
                timer.Stop();
            }
            else
            {
                timer.Interval = period.Value;
                period = null;
                unchecked { timerId++; }
                timer.Start();
            }
        }
    }

    public void Dispose()
    {
        if (timer != null)
        {
            unchecked { timerId++; }
            timer.Stop();
            timer.Tick -= timerTick;
            timer = null;
        }
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return default;
    }
}
