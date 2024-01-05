using System.Windows.Threading;

namespace R3.WPF;

public sealed class DispatcherTimerProvider : TimeProvider
{
    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        return base.CreateTimer(callback, state, dueTime, period);
    }
}

internal sealed class DispatcherTimerProviderTimer : ITimer
{
    DispatcherTimer? timer;
    TimerCallback callback;
    object? state;
    EventHandler timerTick;
    TimeSpan? period;

    public DispatcherTimerProviderTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        this.timerTick = Timer_Tick;
        this.callback = callback;
        this.state = state;
        this.timer = new DispatcherTimer();
        timer.Tick += timerTick;

        Change(dueTime, period);
    }

    public bool Change(TimeSpan dueTime, TimeSpan period)
    {
        if (timer != null)
        {
            timer.Stop();

            this.period = period;
            timer.Interval = dueTime;

            timer.Start();
            return true;
        }
        return false;
    }

    void Timer_Tick(object? sender, EventArgs e)
    {
        callback(state);

        if (timer != null && period != null)
        {
            timer.Stop();

            timer.Interval = period.Value;
            period = null;

            timer.Start();
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
