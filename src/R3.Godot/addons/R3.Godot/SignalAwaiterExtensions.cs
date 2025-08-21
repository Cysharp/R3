
using System;
using Godot;

namespace R3;


public static class SignalAwaiterExtensions
{
    public static Observable<Unit> ToObservable(this SignalAwaiter signalAwaiter)
    {
        return new SignalToObservable(signalAwaiter);
    }
}

public sealed class SignalToObservable : Observable<Unit>
{
    private readonly SignalAwaiter _source;

    public SignalToObservable(SignalAwaiter source)
    {
        _source = source;
    }

    protected override IDisposable SubscribeCore(Observer<Unit> observer)
    {
        SubscribeTask(observer);
        return Disposable.Empty; // no need to return subscription
    }

    async void SubscribeTask(Observer<Unit> observer)
    {
        try
        {
            await _source;
        }
        catch (Exception ex)
        {
            if (!observer.IsDisposed)
            {
                observer.OnCompleted(ex);
            }
            return;
        }

        if (!observer.IsDisposed)
        {
            observer.OnNext(Unit.Default);
            observer.OnCompleted();
        }
    }
}