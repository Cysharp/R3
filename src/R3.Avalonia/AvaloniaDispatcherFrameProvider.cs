using Avalonia.Threading;
using R3.Collections;
using System.Diagnostics.CodeAnalysis;

namespace R3;

// like the Avalonia.Rendering.UiThreadRenderTimer

// NOTE: idially, not polling, use like the WPF's CompositionTarget.Rendering

public sealed class AvaloniaDispatcherFrameProvider : FrameProvider, IDisposable
{
    bool disposed;
    long frameCount;
    FreeListCore<IFrameRunnerWorkItem> list;
    readonly object gate = new object();
    readonly DispatcherTimer timer;
    EventHandler timerTick;

    // frame loop is delayed until first register
    bool running;

    public AvaloniaDispatcherFrameProvider()
        : this(60, null)
    {
    }

    public AvaloniaDispatcherFrameProvider(int framesPerSecond)
        : this(framesPerSecond, null)
    {
    }

    public AvaloniaDispatcherFrameProvider(int framesPerSecond, DispatcherPriority? dispatcherPriority)
    {
        this.timerTick = Run;
        this.list = new FreeListCore<IFrameRunnerWorkItem>(gate);
        this.timer = dispatcherPriority == null
            ? new DispatcherTimer()
            : new DispatcherTimer(dispatcherPriority.Value);
        this.timer.Interval = TimeSpan.FromSeconds(1.0 / framesPerSecond);
        this.timer.Tick += timerTick;
    }

    public override long GetFrameCount()
    {
        ThrowObjectDisposedIf(disposed, typeof(NewThreadSleepFrameProvider));
        return frameCount;
    }

    public override void Register(IFrameRunnerWorkItem callback)
    {
        ThrowObjectDisposedIf(disposed, typeof(NewThreadSleepFrameProvider));
        lock (gate)
        {
            if (running == false)
            {
                running = true;
                timer.Start();
            }
            list.Add(callback, out _);
        }
    }

    public void Dispose()
    {
        lock (gate)
        {
            disposed = true;
            this.timer.Tick -= timerTick;
            this.timer.Stop();
            list.Dispose();
        }
    }

    void Run(object? sender, EventArgs e)
    {
        frameCount++;

        var span = list.AsSpan();
        for (int i = 0; i < span.Length; i++)
        {
            ref readonly var item = ref span[i];
            if (item != null)
            {
                try
                {
                    if (!item.MoveNext(frameCount))
                    {
                        list.Remove(i);
                    }
                }
                catch (Exception ex)
                {
                    list.Remove(i);
                    try
                    {
                        ObservableSystem.GetUnhandledExceptionHandler().Invoke(ex);
                    }
                    catch { }
                }
            }
        }
    }

    static void ThrowObjectDisposedIf(/*[DoesNotReturnIf(true)]*/ bool condition, Type type)
    {
        if (condition)
        {
            ThrowObjectDisposedException(type);
        }
    }

    // [DoesNotReturn]
    internal static void ThrowObjectDisposedException(Type? type) => throw new ObjectDisposedException(type?.FullName);
}
