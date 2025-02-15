using Windows.UI.Core;
using Microsoft.UI.Xaml;
using R3.Collections;
using Windows.Foundation;

namespace R3; // using R3

public static class ObserveOnExtensions
{
    public static Observable<T> ObserveOnDispatcher<T>(this Observable<T> source, CoreDispatcher dispatcher, CoreDispatcherPriority? dispatcherPriority = null)
    {
        return new ObserveOnDispatcher<T>(source, dispatcher, dispatcherPriority);
    }

    public static Observable<T> ObserveOnCurrentWindowDispatcher<T>(this Observable<T> source, CoreDispatcherPriority? dispatcherPriority = null)
    {
        return ObserveOnDispatcher(source, Window.Current!.Dispatcher, dispatcherPriority);
    }

    public static Observable<T> SubscribeOnDispatcher<T>(this Observable<T> source, CoreDispatcher dispatcher, CoreDispatcherPriority? dispatcherPriority = null)
    {
        return new SubscribeOnDispatcher<T>(source, dispatcher, dispatcherPriority);
    }

    public static Observable<T> SubscribeOnCurrentWindowDispatcher<T>(this Observable<T> source, CoreDispatcherPriority? dispatcherPriority = null)
    {
        return SubscribeOnDispatcher(source, Window.Current!.Dispatcher, dispatcherPriority);
    }
}

internal sealed class ObserveOnDispatcher<T>(Observable<T> source, CoreDispatcher dispatcher, CoreDispatcherPriority? dispatcherPriority) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _ObserveOnDispatcher(observer, dispatcher, dispatcherPriority));
    }

    sealed class _ObserveOnDispatcher : Observer<T>
    {
        readonly DispatchedHandler postCallback;

        readonly Observer<T> observer;
        readonly CoreDispatcher dispatcher;
        readonly CoreDispatcherPriority? dispatcherPriority;
        readonly object gate = new object();
        SwapListCore<Notification<T>> list;
        bool running;

        protected override bool AutoDisposeOnCompleted => false;

        public _ObserveOnDispatcher(Observer<T> observer, CoreDispatcher dispatcher, CoreDispatcherPriority? dispatcherPriority)
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
                    if (dispatcherPriority == null)
                    {
                        dispatcher.RunAsync(CoreDispatcherPriority.Normal, postCallback);
                    }
                    else
                    {
                        dispatcher.RunAsync(dispatcherPriority.Value, postCallback);
                    }
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
                    if (dispatcherPriority == null)
                    {
                        dispatcher.RunAsync(CoreDispatcherPriority.Normal, postCallback);
                    }
                    else
                    {
                        dispatcher.RunAsync(dispatcherPriority.Value, postCallback);
                    }
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

internal sealed class SubscribeOnDispatcher<T>(Observable<T> source, CoreDispatcher dispatcher, CoreDispatcherPriority? dispatcherPriority) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return new _SubscribeOnDispatcher(observer, source, dispatcher, dispatcherPriority).Run();
    }

    sealed class _SubscribeOnDispatcher : Observer<T>
    {
        readonly DispatchedHandler postCallback;

        readonly Observer<T> observer;
        readonly Observable<T> source;
        readonly CoreDispatcher dispatcher;
        readonly CoreDispatcherPriority? dispatcherPriority;
        SingleAssignmentDisposableCore disposable;

        public _SubscribeOnDispatcher(Observer<T> observer, Observable<T> source, CoreDispatcher dispatcher, CoreDispatcherPriority? dispatcherPriority)
        {
            this.observer = observer;
            this.source = source;
            this.dispatcher = dispatcher;
            this.dispatcherPriority = dispatcherPriority;
            this.postCallback = Subscribe;
        }

        public IDisposable Run()
        {
            if (dispatcherPriority == null)
            {
                dispatcher.RunAsync(CoreDispatcherPriority.Normal, postCallback);
            }
            else
            {
                dispatcher.RunAsync(dispatcherPriority.Value, postCallback);
            }
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
