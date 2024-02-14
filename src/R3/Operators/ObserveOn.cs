using System.Collections.Concurrent;

namespace R3;

public static partial class ObservableExtensions
{
    /// <summary>ObserveOn SynchronizationContext.Current</summary>
    public static Observable<T> ObserveOnCurrentSynchronizationContext<T>(this Observable<T> source)
    {
        return ObserveOn<T>(source, SynchronizationContext.Current);
    }

    public static Observable<T> ObserveOnThreadPool<T>(this Observable<T> source)
    {
        return new ObserveOnThreadPool<T>(source);
    }

    public static Observable<T> ObserveOn<T>(this Observable<T> source, SynchronizationContext? synchronizationContext)
    {
        if (synchronizationContext == null)
        {
            return new ObserveOnThreadPool<T>(source); // use ThreadPool instead
        }

        return new ObserveOnSynchronizationContext<T>(source, synchronizationContext);
    }

    public static Observable<T> ObserveOn<T>(this Observable<T> source, TimeProvider timeProvider)
    {
        if (timeProvider == TimeProvider.System)
        {
            return new ObserveOnThreadPool<T>(source);
        }

        return new ObserveOnTimeProvider<T>(source, timeProvider);
    }

    public static Observable<T> ObserveOn<T>(this Observable<T> source, FrameProvider frameProvider)
    {
        return new ObserveOnFrameProvider<T>(source, frameProvider);
    }
}

internal sealed class ObserveOnSynchronizationContext<T>(Observable<T> source, SynchronizationContext synchronizationContext) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _ObserveOn(observer, synchronizationContext));
    }

    sealed class _ObserveOn : Observer<T>
    {
        static readonly SendOrPostCallback postCallback = DrainMessages;

        readonly Observer<T> observer;
        readonly SynchronizationContext synchronizationContext;
        readonly object gate = new object();
        SwapListCore<Notification<T>> list;
        bool running;

        protected override bool AutoDisposeOnCompleted => false;

        public _ObserveOn(Observer<T> observer, SynchronizationContext synchronizationContext)
        {
            this.observer = observer;
            this.synchronizationContext = synchronizationContext;
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
                    synchronizationContext.Post(postCallback, this);
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

        static void DrainMessages(object? state)
        {
            var self = (_ObserveOn)state!;

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
                                self.observer.OnCompleted(value.Result);
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
                    self.synchronizationContext.Post(postCallback, self);
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

internal sealed class ObserveOnThreadPool<T>(Observable<T> source) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _ObserveOn(observer));
    }

    sealed class _ObserveOn(Observer<T> observer) : Observer<T>, IThreadPoolWorkItem
    {
        Observer<T> observer = observer;
        ConcurrentQueue<Notification<T>> q = new();
        bool running = false;

        protected override bool AutoDisposeOnCompleted => false;

        protected override void OnNextCore(T value)
        {
            q.Enqueue(new(value));
            TryStartWorker();
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            q.Enqueue(new(error));
            TryStartWorker();
        }

        protected override void OnCompletedCore(Result result)
        {
            q.Enqueue(new(result));
            TryStartWorker();
        }

        void TryStartWorker()
        {
            lock (q)
            {
                if (!running)
                {
                    running = true;
                    ThreadPool.UnsafeQueueUserWorkItem(this, preferLocal: false);
                }
            }
        }

        void IThreadPoolWorkItem.Execute()
        {
        AGAIN:
            while (q.TryDequeue(out var item))
            {
                switch (item.Kind)
                {
                    case NotificationKind.OnNext:
                        observer.OnNext(item.Value!);
                        break;
                    case NotificationKind.OnErrorResume:
                        observer.OnErrorResume(item.Error!);
                        break;
                    case NotificationKind.OnCompleted:
                        try
                        {
                            observer.OnCompleted(item.Result!);
                        }
                        finally
                        {
                            Dispose();
                        }
                        break;
                }
            }

            lock (q)
            {
                if (q.Count != 0)
                {
                    goto AGAIN;
                }
                running = false;
                return;
            }
        }
    }
}

internal sealed class ObserveOnTimeProvider<T>(Observable<T> source, TimeProvider timeProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _ObserveOn(observer, timeProvider));
    }

    internal sealed class _ObserveOn : Observer<T>
    {
        static readonly TimerCallback timerCallback = DrainMessages;

        readonly Observer<T> observer;
        readonly TimeProvider timeProvider;
        readonly Queue<Notification<T>> queue; // local queue, lock gate
        ITimer? timer;
        bool running;

        protected override bool AutoDisposeOnCompleted => false;

        public _ObserveOn(Observer<T> observer, TimeProvider timeProvider)
        {
            this.observer = observer;
            this.timeProvider = timeProvider;
            this.queue = new();
            this.timer = timeProvider.CreateStoppedTimer(timerCallback, this);
        }

        protected override void OnNextCore(T value)
        {
            lock (queue)
            {
                if (timer == null) return;
                queue.Enqueue(new(value));
                if (queue.Count == 1 && !running)
                {
                    running = true;
                    timer.RestartImmediately();
                }
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            lock (queue)
            {
                if (timer == null) return;
                queue.Enqueue(new(error));
                if (queue.Count == 1 && !running)
                {
                    running = true;
                    timer.RestartImmediately();
                }
            }
        }

        protected override void OnCompletedCore(Result result)
        {
            lock (queue)
            {
                if (timer == null) return;
                queue.Enqueue(new(result));
                if (queue.Count == 1 && !running)
                {
                    running = true;
                    timer.RestartImmediately();
                }
            }
        }

        static void DrainMessages(object? state)
        {
            var self = (_ObserveOn)state!;
            var queue = self.queue;

            Notification<T> value = default;
            while (true)
            {
                lock (queue)
                {
                    if (self.timer == null || !queue.TryDequeue(out value))
                    {
                        self.running = false;
                        return;
                    }
                }

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
                                self.observer.OnCompleted(value.Result);
                            }
                            finally
                            {
                                self.Dispose();
                            }
                            break;
                        default:
                            break;
                    }

                    continue; // loop to drain all messages
                }
                catch (Exception ex)
                {
                    ObservableSystem.GetUnhandledExceptionHandler().Invoke(ex);
                    continue;
                }
            }
        }

        protected override void DisposeCore()
        {
            lock (queue)
            {
                if (timer != null)
                {
                    timer.Dispose(); // stop timer.
                    timer = null;
                }
                queue.Clear();
            }
        }
    }
}

internal sealed class ObserveOnFrameProvider<T>(Observable<T> source, FrameProvider frameProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _ObserveOn(observer, frameProvider));
    }

    internal sealed class _ObserveOn : Observer<T>, IFrameRunnerWorkItem
    {
        readonly Observer<T> observer;
        readonly FrameProvider frameProvider;
        readonly object gate = new object();
        SwapListCore<Notification<T>> list;
        bool running;

        protected override bool AutoDisposeOnCompleted => false;

        public _ObserveOn(Observer<T> observer, FrameProvider frameProvider)
        {
            this.observer = observer;
            this.frameProvider = frameProvider;
            this.running = false;
            this.list = new SwapListCore<Notification<T>>();
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
                    frameProvider.Register(this);
                }
            }
        }

        public bool MoveNext(long frameCount)
        {
            ReadOnlySpan<Notification<T>> values;
            bool token;
            lock (gate)
            {
                values = list.Swap(out token);
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
                            observer.OnNext(value.Value!);
                            break;
                        case NotificationKind.OnErrorResume:
                            observer.OnErrorResume(value.Error!);
                            break;
                        case NotificationKind.OnCompleted:
                            try
                            {
                                observer.OnCompleted(value.Result!);
                            }
                            finally
                            {
                                Dispose();
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
            lock (gate)
            {
                list.Clear(token);

                if (IsDisposed)
                {
                    running = false;
                    return false;
                }

                if (list.HasValue)
                {
                    return true;
                }
                else
                {
                    running = false;
                    return false;
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
    }
}
