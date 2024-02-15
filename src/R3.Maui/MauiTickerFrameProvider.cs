using Microsoft.Maui.Animations;
using R3.Collections;

namespace R3;

public class MauiTickerFrameProvider : FrameProvider, IDisposable
{
    readonly ITicker ticker;
    readonly object gate = new();
    readonly Action timerTick;

    FreeListCore<IFrameRunnerWorkItem> runners;
    long frameCount;
    bool disposed;

    // frame loop is delayed until first register
    bool running;

    public MauiTickerFrameProvider(ITicker ticker)
    {
        this.ticker = ticker;
        timerTick = Tick;
        runners = new FreeListCore<IFrameRunnerWorkItem>(gate);
    }

    public override long GetFrameCount()
    {
        ThrowIfDisposed();
        return frameCount;
    }

    public override void Register(IFrameRunnerWorkItem callback)
    {
        ThrowIfDisposed();
        lock (gate)
        {
            if (running == false)
            {
                if (!ticker.IsRunning)
                {
                    ticker.Start();
                }
                ticker.Fire += timerTick;
                running = true;
            }
            runners.Add(callback, out _);
        }
    }

    public void Dispose()
    {
        lock (gate)
        {
            disposed = true;
            ticker.Fire -= timerTick;
            runners.Dispose();
        }
    }

    void Tick()
    {
        frameCount++;

        var span = runners.AsSpan();
        for (int i = 0; i < span.Length; i++)
        {
            ref readonly var item = ref span[i];
            if (item != null)
            {
                try
                {
                    if (!item.MoveNext(frameCount))
                    {
                        runners.Remove(i);
                    }
                }
                catch (Exception ex)
                {
                    runners.Remove(i);
                    try
                    {
                        ObservableSystem.GetUnhandledExceptionHandler().Invoke(ex);
                    }
                    catch { }
                }
            }
        }
    }

    void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(typeof(MauiTickerFrameProvider).FullName);
        }
    }
}
