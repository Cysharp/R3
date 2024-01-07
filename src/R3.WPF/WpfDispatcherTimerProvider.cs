using System.Windows.Threading;

namespace R3;

public sealed class WpfDispatcherTimerProvider : TimeProvider
{
    readonly DispatcherPriority? priority;
    readonly Dispatcher? dispatcher;

    public WpfDispatcherTimerProvider()
    {
        this.priority = null;
        this.dispatcher = null;
    }

    public WpfDispatcherTimerProvider(DispatcherPriority priority)
    {
        this.priority = priority;
        this.dispatcher = null;
    }

    public WpfDispatcherTimerProvider(DispatcherPriority priority, Dispatcher dispatcher)
    {
        this.priority = priority;
        this.dispatcher = dispatcher;
    }

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        return new WpfDispatcherTimerProviderTimer(priority, dispatcher, callback, state, dueTime, period);
    }
}

internal sealed class WpfDispatcherTimerProviderTimer : ITimer
{
    DispatcherTimer? timer;
    TimerCallback callback;
    object? state;
    EventHandler timerTick;
    TimeSpan? period;

    public WpfDispatcherTimerProviderTimer(DispatcherPriority? priority, Dispatcher? dispatcher, TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        this.timerTick = Timer_Tick;
        this.callback = callback;
        this.state = state;
        if (priority == null && dispatcher == null)
        {
            this.timer = new DispatcherTimer();
        }
        else if (dispatcher == null) // priority is not null
        {
            this.timer = new DispatcherTimer(priority!.Value);
        }
        else
        {
            this.timer = new DispatcherTimer(priority!.Value, dispatcher);
        }

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

    void Timer_Tick(object? sender, EventArgs e)
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
