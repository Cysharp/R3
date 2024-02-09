using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Stride.Engine;
using Stride.Engine.Events;

namespace R3;
public static class R3StrideEventExtension
{
    public static Observable<T> AsObservable<T>(this EventKey<T> eventKey, CancellationToken token = default)
    {
        return Observable.Create<T, (EventKey<T>, CancellationToken)>((eventKey, token), static (observer, state) =>
        {
            if(R3.Stride.StrideInitializer.DefaultFrameProvider == null)
            {
                throw new NullReferenceException("initialize default frameprovider first");
            }
            var (evk, token) = state;
            var receiver = new EventReceiver<T>(evk);
            var runnerItem = new EventReceiverRunnerItem<T>(observer, receiver, R3.Stride.StrideInitializer.DefaultFrameProvider, token);
            Stride.StrideInitializer.DefaultFrameProvider.Register(runnerItem);
            return runnerItem;
        });
    }
    public static Observable<Unit> AsObservable(this EventKey eventKey, CancellationToken token = default)
    {
        return Observable.Create<Unit, (EventKey, CancellationToken)>((eventKey, token), static (observer, state) =>
        {
            if (R3.Stride.StrideInitializer.DefaultFrameProvider == null)
            {
                throw new NullReferenceException("initialize default frameprovider first");
            }
            var (evk, token) = state;
            var receiver = new EventReceiver(evk);
            var runnerItem = new EventReceiverRunnerItem(observer, receiver, R3.Stride.StrideInitializer.DefaultFrameProvider, token);
            Stride.StrideInitializer.DefaultFrameProvider.Register(runnerItem);
            return runnerItem;
        });
    }
    sealed class EventReceiverRunnerItem : IFrameRunnerWorkItem, IDisposable
    {
        Observer<Unit> observer;
        CancellationToken token;
        IDisposable cancellationTokenSubscription;
        EventReceiver _receiver;
        public EventReceiverRunnerItem(Observer<Unit> observer, EventReceiver receiver, FrameProvider frameProvider, CancellationToken token = default)
        {
            this.observer = observer;
            this.token = token;
            this._receiver = receiver;
            if (token.CanBeCanceled)
            {
                this.cancellationTokenSubscription = token.UnsafeRegister(static (state) =>
                {
                    var item = state as EventReceiverRunnerItem;
                    if (item != null)
                    {
                        item.observer.OnCompleted();
                        item.Dispose();
                    }
                }, this);
            }
            else
            {
                this.cancellationTokenSubscription = Disposable.Empty;
            }
        }
        bool isDisposed = false;
        public bool MoveNext(long frameCount)
        {
            if (token.IsCancellationRequested || isDisposed)
            {
                return false;
            }
            if (observer.IsDisposed)
            {
                Dispose();
                return false;
            }
            if (_receiver.TryReceive())
            {
                observer.OnNext(Unit.Default);
            }
            return true;
        }
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;
                cancellationTokenSubscription.Dispose();
                try
                {
                    _receiver.Dispose();
                } catch { }
            }
        }
    }
    // I did trying to merge EventReceiverRunnerItem and EventReceiverRunnerItem<T>, but could not.
    sealed class EventReceiverRunnerItem<T> : IFrameRunnerWorkItem, IDisposable
    {
        Observer<T> observer;
        CancellationToken token;
        IDisposable cancellationTokenSubscription;
        EventReceiver<T> _receiver;
        public EventReceiverRunnerItem(Observer<T> observer, EventReceiver<T> receiver, FrameProvider frameProvider, CancellationToken token = default)
        {
            this.observer = observer;
            this.token = token;
            this._receiver = receiver;
            if(token.CanBeCanceled)
            {
                this.cancellationTokenSubscription = token.UnsafeRegister(static (state) =>
                {
                    var item = state as EventReceiverRunnerItem<T>;
                    if(item != null)
                    {
                        item.observer.OnCompleted();
                        item.Dispose();
                    }
                }, this);
            }
            else
            {
                this.cancellationTokenSubscription = Disposable.Empty;
            }
        }
        bool isDisposed = false;
        public bool MoveNext(long frameCount)
        {
            if(token.IsCancellationRequested || isDisposed)
            {
                return false;
            }
            if(observer.IsDisposed)
            {
                Dispose();
                return false;
            }
            if(_receiver.TryReceive(out var item))
            {
                observer.OnNext(item);
            }
            return true;
        }
        public void Dispose()
        {
            if(!isDisposed)
            {
                isDisposed = true;
                cancellationTokenSubscription.Dispose();
                try
                {
                    _receiver.Dispose();
                } catch { }
            }
        }
    }
}
