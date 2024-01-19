using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using ThreadingTimer = System.Threading.Timer;

namespace R3.WindowsForms;

public sealed class WindowsFormsTimerProvider(ISynchronizeInvoke? marshalingControl) :
    TimeProvider
{
    public WindowsFormsTimerProvider()
        : this(null)
    {
    }

    public override ITimer CreateTimer(
        TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        return new WindowsFormsTimerProviderTimer(
            marshalingControl,
            SynchronizationContext.Current as WindowsFormsSynchronizationContext,
            callback,
            state,
            dueTime,
            period);
    }
}

internal sealed class WindowsFormsTimerProviderTimer :
    ITimer
{
    public WindowsFormsTimerProviderTimer(
        ISynchronizeInvoke? control,
        WindowsFormsSynchronizationContext? synchronizationContext,
        TimerCallback callback,
        object? state,
        TimeSpan dueTime,
        TimeSpan period)
    {
        if (control is null && synchronizationContext is null)
        {
            throw new ArgumentNullException(nameof(control));
        }

        var context = new TimerContext(
            control,
            synchronizationContext,
            callback,
            state);

        this._timer = new ThreadingTimer(InvokeCallback, context, dueTime, period);

        static void InvokeCallback(object? context)
        {
            var (control, syncCtx, callback, state) = (TimerContext)context!;

            if (control is not null)
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(callback, [state]);
                }
                else
                {
                    callback(state);
                }
            }
            else if(syncCtx is not null)
            {
                syncCtx.Send(new SendOrPostCallback(callback), state);
            }
        }
    }

    private record TimerContext(
        ISynchronizeInvoke? Control,
        WindowsFormsSynchronizationContext? SynchronizationContext,
        TimerCallback Callback,
        object? State);

    public bool Change(TimeSpan dueTime, TimeSpan period)
    {
        return this._timer.Change(dueTime, period);
    }

    public void Dispose()
    {
        this._timer.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        this.Dispose();
        return default;
    }

    private readonly ThreadingTimer _timer;
}
