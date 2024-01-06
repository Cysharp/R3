namespace R3;

public sealed class TimerFrameProvider : FrameProvider, IDisposable
{
    static readonly TimerCallback timerCallback = Run;

    readonly object gate = new object();
    long frameCount;
    bool disposed;
    FreeListCore<IFrameRunnerWorkItem> list;
    ITimer timer;

    public TimerFrameProvider(TimeSpan period)
        : this(period, period, TimeProvider.System)
    {
    }

    public TimerFrameProvider(TimeSpan dueTime, TimeSpan period)
        : this(dueTime, period, TimeProvider.System)
    {
    }

    public TimerFrameProvider(TimeSpan dueTime, TimeSpan period, TimeProvider timeProvider)
    {
        this.list = new FreeListCore<IFrameRunnerWorkItem>(gate);
        this.timer = timeProvider.CreateStoppedTimer(timerCallback, this);

        // start timer
        this.timer.Change(dueTime, period);
    }

    public override long GetFrameCount()
    {
        ThrowHelper.ThrowObjectDisposedIf(disposed, typeof(TimerFrameProvider));
        return frameCount;
    }

    public override void Register(IFrameRunnerWorkItem callback)
    {
        ThrowHelper.ThrowObjectDisposedIf(disposed, typeof(TimerFrameProvider));
        list.Add(callback, out _);
    }

    public void Dispose()
    {
        if (!disposed)
        {
            disposed = true;
            lock (gate)
            {
                timer.Dispose();
                list.Dispose();
            }
        }
    }

    static void Run(object? state)
    {
        var self = (TimerFrameProvider)state!;
        if (self.disposed)
        {
            return;
        }

        lock (self.gate)
        {
            self.frameCount++;

            var span = self.list.AsSpan();
            for (int i = 0; i < span.Length; i++)
            {
                ref readonly var item = ref span[i];
                if (item != null)
                {
                    try
                    {
                        if (!item.MoveNext(self.frameCount))
                        {
                            self.list.Remove(i);
                        }
                    }
                    catch (Exception ex)
                    {
                        self.list.Remove(i);
                        try
                        {
                            ObservableSystem.GetUnhandledExceptionHandler().Invoke(ex);
                        }
                        catch { }
                    }
                }
            }
        }
    }
}
