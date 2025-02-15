using Avalonia.Threading;

namespace R3;

public sealed class AvaloniaDispatcherTimeProvider : TimeProvider
{
    public static readonly TimeProvider Default = new AvaloniaDispatcherTimeProvider();

    readonly DispatcherPriority? priority;

    public AvaloniaDispatcherTimeProvider()
    {
        this.priority = null;
    }

    public AvaloniaDispatcherTimeProvider(DispatcherPriority priority)
    {
        this.priority = priority;
    }

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        return new AvaloniaDispatcherTimeProviderTimer(priority, callback, state, dueTime, period);
    }
}

internal sealed class AvaloniaDispatcherTimeProviderTimer : ITimer
{
    DispatcherTimer? timer;
    TimerCallback callback;
    object? state;
    EventHandler timerTick;
    TimeSpan? period;
    short timerId;

    public AvaloniaDispatcherTimeProviderTimer(DispatcherPriority? priority, TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        this.timerTick = Timer_Tick;
        this.callback = callback;
        this.state = state;
        if (priority == null)
        {
            this.timer = new DispatcherTimer();
        }
        else
        {
            this.timer = new DispatcherTimer(priority!.Value);
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

            // when start, change timerId.
            unchecked { timerId++; }
            timer.Start();
            return true;
        }
        return false;
    }

    void Timer_Tick(object? sender, EventArgs e)
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
