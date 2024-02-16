using Microsoft.UI.Xaml;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace R3;

public sealed class WinUI3DispatcherTimerProvider : TimeProvider
{
    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        return new WinUI3DispatcherTimerProviderTimer(callback, state, dueTime, period);
    }
}

internal sealed class WinUI3DispatcherTimerProviderTimer : ITimer
{
    DispatcherTimer? timer;
    TimerCallback callback;
    object? state;
    EventHandler<object> timerTick;
    TimeSpan? period;

    public WinUI3DispatcherTimerProviderTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
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

            timer.Start();
            return true;
        }
        return false;
    }

    void Timer_Tick(object? sender, object e)
    {
        callback(state);

        if (timer != null && period != null)
        {
            if (period.Value == Timeout.InfiniteTimeSpan)
            {
                period = null;
                timer.Stop();
            }
            else
            {
                timer.Interval = period.Value;
                period = null;
                timer.Start();
            }
        }
    }

    public void Dispose()
    {
        if (timer != null)
        {
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
