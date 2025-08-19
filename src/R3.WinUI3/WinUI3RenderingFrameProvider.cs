﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using R3.Collections;

namespace R3;

public sealed class WinUI3RenderingFrameProvider : FrameProvider, IDisposable
{
    public static readonly FrameProvider Default = new WinUI3RenderingFrameProvider(true);

    readonly bool isDefaultProvider;
    bool disposed;
    long frameCount;
    FreeListCore<IFrameRunnerWorkItem> list;
    readonly Lock gate = new();

    EventHandler<object> messageLoop;

    WinUI3RenderingFrameProvider(bool _) : this()
    {
        isDefaultProvider = true;
    }

    public WinUI3RenderingFrameProvider()
    {
        this.messageLoop = Run;
        this.list = new FreeListCore<IFrameRunnerWorkItem>(gate);
        Microsoft.UI.Xaml.Media.CompositionTarget.Rendering += messageLoop;
    }

    public override long GetFrameCount()
    {
        ThrowObjectDisposedIf(disposed, typeof(WinUI3RenderingFrameProvider));
        return frameCount;
    }

    public override void Register(IFrameRunnerWorkItem callback)
    {
        ThrowObjectDisposedIf(disposed, typeof(WinUI3RenderingFrameProvider));
        list.Add(callback, out _);
    }

    public void Dispose()
    {
        if (isDefaultProvider) return;

        disposed = true;
        Microsoft.UI.Xaml.Media.CompositionTarget.Rendering -= messageLoop;
        list.Dispose();
    }

    void Run(object? sender, object e)
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
