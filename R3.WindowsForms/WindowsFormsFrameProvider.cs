using System;
using System.Windows.Forms;

using R3.Collections;

namespace R3.WindowsForms;

public delegate bool MessageFilter(in Message message);

public sealed class WindowsFormsFrameProvider :
    FrameProvider,
    IDisposable
{
    private bool disposed;
    private long frameCount;
    private FreeListCore<IFrameRunnerWorkItem> list;
    private readonly object gate = new object();
    private readonly MessageHook filter;
    private readonly MessageFilter? predicate;

    public WindowsFormsFrameProvider()
        : this(null)
    {
    }

    public WindowsFormsFrameProvider(
        MessageFilter? predicate)
    {
        this.list = new FreeListCore<IFrameRunnerWorkItem>(gate);
        this.filter = new MessageHook(this);
        this.predicate = predicate;

        Application.AddMessageFilter(this.filter);
    }

    public override long GetFrameCount()
    {
        ThrowObjectDisposedIf(disposed, typeof(WindowsFormsFrameProvider));
        return frameCount;
    }

    public override void Register(IFrameRunnerWorkItem callback)
    {
        ThrowObjectDisposedIf(disposed, typeof(WindowsFormsFrameProvider));
        list.Add(callback, out _);
    }

    public void Dispose()
    {
        disposed = true;
        Application.RemoveMessageFilter(this.filter);
        list.Dispose();
    }

    private void Run(in Message message)
    {
        if (this.predicate is {} p && !p(message))
        {
            return;
        }

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
                    catch
                    {
                    }
                }
            }
        }
    }

    private static void ThrowObjectDisposedIf(/*[DoesNotReturnIf(true)]*/ bool condition, Type type)
    {
        if (condition)
        {
            ThrowObjectDisposedException(type);
        }
    }

    // [DoesNotReturn]
    private static void ThrowObjectDisposedException(Type? type) =>
        throw new ObjectDisposedException(type?.FullName);

    private sealed class MessageHook(WindowsFormsFrameProvider parent) : IMessageFilter
    {
        public bool PreFilterMessage(ref Message m)
        {
            parent.Run(in m);
            return false;
        }
    }
}
