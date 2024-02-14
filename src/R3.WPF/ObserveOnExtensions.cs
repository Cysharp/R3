using R3.Collections;
using System.Windows.Threading;

namespace R3; // using R3

public static class ObserveOnExtensions
{
    public static Observable<T> ObserveOnDispatcher<T>(this Observable<T> source, Dispatcher dispatcher, DispatcherPriority dispatcherPriority = DispatcherPriority.Normal)
    {
        return new ObserveOnDispatcher<T>(source, dispatcher, dispatcherPriority);
    }

    public static Observable<T> ObserveOnCurrentDispatcher<T>(this Observable<T> source, DispatcherPriority dispatcherPriority = DispatcherPriority.Normal)
    {
        return ObserveOnDispatcher(source, Dispatcher.CurrentDispatcher, dispatcherPriority);
    }

    public static Observable<T> SubscribeOnDispatcher<T>(this Observable<T> source, Dispatcher dispatcher, DispatcherPriority dispatcherPriority = DispatcherPriority.Normal)
    {
        return new SubscribeOnDispatcher<T>(source, dispatcher, dispatcherPriority);
    }

    public static Observable<T> SubscribeOnCurrentDispatcher<T>(this Observable<T> source, DispatcherPriority dispatcherPriority = DispatcherPriority.Normal)
    {
        return SubscribeOnDispatcher(source, Dispatcher.CurrentDispatcher, dispatcherPriority);
    }
}

internal sealed class ObserveOnDispatcher<T>(Observable<T> source, Dispatcher dispatcher, DispatcherPriority dispatcherPriority) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _ObserveOnDispatcher(observer, dispatcher, dispatcherPriority));
    }

    sealed class _ObserveOnDispatcher : Observer<T>
    {
        readonly Action postCallback;

        readonly Observer<T> observer;
        readonly Dispatcher dispatcher;
        readonly DispatcherPriority dispatcherPriority;
        readonly object gate = new object();
        SwapListCore<Notification<T>> list;
        bool running;

        protected override bool AutoDisposeOnCompleted => false;

        public _ObserveOnDispatcher(Observer<T> observer, Dispatcher dispatcher, DispatcherPriority dispatcherPriority)
        {
            this.observer = observer;
            this.dispatcher = dispatcher;
            this.dispatcherPriority = dispatcherPriority;
            this.postCallback = DrainMessages;
        }

        protected override void OnNextCore(T value)
        {
            EnqueueValue(new(value));
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            EnqueueValue(new(error));
        }

        protected override void OnCompletedCore(Result result)
        {
            EnqueueValue(new(result));
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
                    dispatcher.InvokeAsync(postCallback, dispatcherPriority);
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
                    dispatcher.InvokeAsync(postCallback, dispatcherPriority);
                    return;
                }
                else
                {
                    self.running = false;
                    return;
                }
            }
        }
    }
}

internal sealed class SubscribeOnDispatcher<T>(Observable<T> source, Dispatcher dispatcher, DispatcherPriority dispatcherPriority) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return new _SubscribeOnDispatcher(observer, source, dispatcher, dispatcherPriority).Run();
    }

    sealed class _SubscribeOnDispatcher : Observer<T>
    {
        readonly Action postCallback;

        readonly Observer<T> observer;
        readonly Observable<T> source;
        readonly Dispatcher dispatcher;
        readonly DispatcherPriority dispatcherPriority;
        SingleAssignmentDisposableCore disposable;

        public _SubscribeOnDispatcher(Observer<T> observer, Observable<T> source, Dispatcher dispatcher, DispatcherPriority dispatcherPriority)
        {
            this.observer = observer;
            this.source = source;
            this.dispatcher = dispatcher;
            this.dispatcherPriority = dispatcherPriority;
            this.postCallback = Subscribe;
        }

        public IDisposable Run()
        {
            dispatcher.InvokeAsync(postCallback, dispatcherPriority);
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
