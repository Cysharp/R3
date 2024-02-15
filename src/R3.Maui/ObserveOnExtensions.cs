using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using R3.Collections;

namespace R3; // using R3

public static class ObserveOnExtensions
{
    static IDispatcher GetDefaultDispatcher() =>
        (ObservableSystem.DefaultTimeProvider as MauiDispatcherTimerProvider)?.Dispatcher
        ?? Application.Current!.Dispatcher;

    public static Observable<T> ObserveOnDispatcher<T>(this Observable<T> source)
    {
        return ObserveOnDispatcher(source, GetDefaultDispatcher());
    }

    public static Observable<T> ObserveOnDispatcher<T>(this Observable<T> source, IDispatcher dispatcher)
    {
        return new ObserveOnDispatcher<T>(source, dispatcher);
    }

    public static Observable<T> SubscribeOnUIThreadDispatcher<T>(this Observable<T> source)
    {
        return SubscribeOnDispatcher(source, GetDefaultDispatcher());
    }

    public static Observable<T> SubscribeOnDispatcher<T>(this Observable<T> source, IDispatcher dispatcher)
    {
        return new SubscribeOnDispatcher<T>(source, dispatcher);
    }
}

internal sealed class ObserveOnDispatcher<T>(Observable<T> source, IDispatcher dispatcher) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _ObserveOnDispatcher(observer, dispatcher));
    }

    sealed class _ObserveOnDispatcher : Observer<T>
    {
        readonly Action postCallback;
        readonly Observer<T> observer;
        readonly IDispatcher dispatcher;
        readonly object gate = new();
        SwapListCore<Notification<T>> list;
        bool running;

        protected override bool AutoDisposeOnCompleted => false;

        public _ObserveOnDispatcher(Observer<T> observer, IDispatcher dispatcher)
        {
            this.observer = observer;
            this.dispatcher = dispatcher;
            this.postCallback = DrainMessages;
        }

        protected override void OnNextCore(T value)
        {
            EnqueueValue(new Notification<T>(value));
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            EnqueueValue(new Notification<T>(error));
        }

        protected override void OnCompletedCore(Result result)
        {
            EnqueueValue(new Notification<T>(result));
        }

        void EnqueueValue(Notification<T> value)
        {
            lock (gate)
            {
                if (IsDisposed) return;
                list.Add(value);

                if (!running)
                {
                    running = true;
                    dispatcher.Dispatch(postCallback);
                }
            }
        }

        protected override void DisposeCore()
        {
            lock (gate)
            {
                list.Dispose();
            }
        }

        void DrainMessages()
        {
            var self = this;

            ReadOnlySpan<Notification<T>> values;
            bool token;
            lock (self.gate)
            {
                values = self.list.Swap(out token);
                if (values.Length == 0)
                {
                    goto FINALIZE;
                }
            }

            foreach (var value in values)
            {
                try
                {
                    switch (value.Kind)
                    {
                        case NotificationKind.OnNext:
                            self.observer.OnNext(value.Value!);
                            break;
                        case NotificationKind.OnErrorResume:
                            self.observer.OnErrorResume(value.Error!);
                            break;
                        case NotificationKind.OnCompleted:
                            try
                            {
                                self.observer.OnCompleted(value.Result!);
                            }
                            finally
                            {
                                self.Dispose();
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        ObservableSystem.GetUnhandledExceptionHandler().Invoke(ex);
                    }
                    catch { }
                }
            }

            FINALIZE:
            lock (self.gate)
            {
                self.list.Clear(token);

                if (self.IsDisposed)
                {
                    self.running = false;
                    return;
                }

                if (self.list.HasValue)
                {
                    // post again
                    dispatcher.Dispatch(postCallback);
                }
                else
                {
                    self.running = false;
                }
            }
        }
    }
}

internal sealed class SubscribeOnDispatcher<T>(Observable<T> source, IDispatcher dispatcher) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return new _SubscribeOnDispatcher(observer, source, dispatcher).Run();
    }

    sealed class _SubscribeOnDispatcher : Observer<T>
    {
        readonly Action postCallback;

        readonly Observer<T> observer;
        readonly Observable<T> source;
        readonly IDispatcher dispatcher;
        SingleAssignmentDisposableCore disposable;

        public _SubscribeOnDispatcher(Observer<T> observer, Observable<T> source, IDispatcher dispatcher)
        {
            this.observer = observer;
            this.source = source;
            this.dispatcher = dispatcher;
            this.postCallback = Subscribe;
        }

        public IDisposable Run()
        {
            dispatcher.Dispatch(postCallback);
            return this;
        }

        void Subscribe()
        {
            disposable.Disposable = source.Subscribe(this);
        }

        protected override void OnNextCore(T value)
        {
            observer.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            observer.OnCompleted(result);
        }

        protected override void DisposeCore()
        {
            disposable.Dispose();
        }
    }
}
