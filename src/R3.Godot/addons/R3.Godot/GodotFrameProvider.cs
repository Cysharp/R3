#nullable enable

using Godot;
using R3.Collections;
using System;
using System.Runtime.CompilerServices;

namespace R3;

internal enum PlayerLoopTiming
{
    Process,
    PhysicsProcess
}

public class GodotFrameProvider : FrameProvider
{
    public static readonly GodotFrameProvider Process = new GodotFrameProvider(PlayerLoopTiming.Process);
    public static readonly GodotFrameProvider PhysicsProcess = new GodotFrameProvider(PlayerLoopTiming.PhysicsProcess);

    FreeListCore<IFrameRunnerWorkItem> list;
    readonly object gate = new object();

    PlayerLoopTiming PlayerLoopTiming { get; }

    internal StrongBox<double> Delta = default!; // set from Node before running process.

    internal GodotFrameProvider(PlayerLoopTiming playerLoopTiming)
    {
        this.PlayerLoopTiming = playerLoopTiming;
        this.list = new FreeListCore<IFrameRunnerWorkItem>(gate);
    }

    public override long GetFrameCount()
    {
        if (PlayerLoopTiming == PlayerLoopTiming.Process)
        {
            return (long)Engine.GetProcessFrames();
        }
        else
        {
            return (long)Engine.GetPhysicsFrames();
        }
    }

    public override void Register(IFrameRunnerWorkItem callback)
    {
        list.Add(callback, out _);
    }

    internal void Run(double _)
    {
        long frameCount = GetFrameCount();

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
}
