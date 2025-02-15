using Microsoft.Maui.Dispatching;

namespace R3;

public class MauiDispatcherTimeProvider(IDispatcher dispatcher) : TimeProvider
{
    public IDispatcher Dispatcher => dispatcher;

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        var dispatcherTimer = dispatcher.CreateTimer();
        return new MauiDispatcherTimeProviderTimer(dispatcherTimer, callback, state, dueTime, period);
    }
}

sealed class MauiDispatcherTimeProviderTimer : ITimer
{
    readonly TimerCallback callback;
    readonly object? state;
    readonly EventHandler timerTick;
    IDispatcherTimer? timer;
    TimeSpan? period;
    short timerId;

    public MauiDispatcherTimeProviderTimer(IDispatcherTimer timer, TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
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
            // when start, change timerId.
            unchecked { timerId++; }
            timer.Start();
            return true;
        }
        return false;
    }

    void Tick(object? sender, EventArgs _)
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
            }
        }
    }
}
