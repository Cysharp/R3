using Microsoft.Maui.Dispatching;

namespace R3;

public class MauiDispatcherTimerProvider(IDispatcher dispatcher) : TimeProvider
{
    public IDispatcher Dispatcher => dispatcher;
    
    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        var dispatcherTimer = dispatcher.CreateTimer();
        return new MauiDispatcherTimerProviderTimer(dispatcherTimer, callback, state, dueTime, period);
    }
}

sealed class MauiDispatcherTimerProviderTimer : ITimer
{
    readonly TimerCallback callback;
    readonly object? state;
    readonly EventHandler timerTick;
    IDispatcherTimer? timer;
    TimeSpan? period;

    public MauiDispatcherTimerProviderTimer(IDispatcherTimer timer, TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        this.timer = timer;
        this.timerTick = Tick;
        this.callback = callback;
        this.state = state;
        timer.Tick += timerTick;

        if (dueTime != Timeout.InfiniteTimeSpan)
        {
            Change(dueTime, period);
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

    void Tick(object? sender, EventArgs _)
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
            }
        }
    }
}
