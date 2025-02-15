using System.Windows.Threading;

namespace R3;

public sealed class WpfDispatcherTimeProvider : TimeProvider
{
    public static readonly TimeProvider Default = new WpfDispatcherTimeProvider();

    readonly DispatcherPriority? priority;
    readonly Dispatcher? dispatcher;

    public WpfDispatcherTimeProvider()
    {
        this.priority = null;
        this.dispatcher = null;
    }

    public WpfDispatcherTimeProvider(DispatcherPriority priority)
    {
        this.priority = priority;
        this.dispatcher = null;
    }

    public WpfDispatcherTimeProvider(DispatcherPriority priority, Dispatcher dispatcher)
    {
        this.priority = priority;
        this.dispatcher = dispatcher;
    }

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        return new WpfDispatcherTimeProviderTimer(priority, dispatcher, callback, state, dueTime, period);
    }
}

internal sealed class WpfDispatcherTimeProviderTimer : ITimer
{
    DispatcherTimer? timer;
    TimerCallback callback;
    object? state;
    EventHandler timerTick;
    TimeSpan? period;
    short timerId;

    public WpfDispatcherTimeProviderTimer(DispatcherPriority? priority, Dispatcher? dispatcher, TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
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
