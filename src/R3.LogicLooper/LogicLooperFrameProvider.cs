using System;
using System.ComponentModel.DataAnnotations;
using Cysharp.Threading;
using R3.Collections;

namespace R3;

public sealed class LogicLooperFrameProvider : FrameProvider, IDisposable
{
    bool disposed;
    long frameCount;
    FreeListCore<IFrameRunnerWorkItem> list;
    Task loop;
    internal long timestamp;
    internal TimeSpan deltaTime;
    readonly object gate = new object();

    public LogicLooperFrameProvider(ILogicLooper looper)
    {
        this.list = new FreeListCore<IFrameRunnerWorkItem>(gate);

        this.frameCount = looper.CurrentFrame;
        this.loop = looper.RegisterActionAsync(Run);
    }

    public override long GetFrameCount()
    {
        ThrowObjectDisposedIf(disposed, typeof(LogicLooperFrameProvider));
        return frameCount;
    }

    public override void Register(IFrameRunnerWorkItem callback)
    {
        ThrowObjectDisposedIf(disposed, typeof(LogicLooperFrameProvider));
        list.Add(callback, out _);
    }

    public void Dispose()
    {
        disposed = true;
        list.Dispose();
    }

    bool Run(in LogicLooperActionContext context)
    {
        if (disposed) return false;

        frameCount = context.CurrentFrame;
        deltaTime = context.ElapsedTimeFromPreviousFrame;
        timestamp += context.ElapsedTimeFromPreviousFrame.Ticks;

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

        return true;
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
