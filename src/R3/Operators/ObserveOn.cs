using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> ObserveOn<T>(this Observable<T> source, SynchronizationContext? synchronizationContext)
    {
        if (synchronizationContext == null)
        {
            return ObserveOn<T>(source, TimeProvider.System); // use ThreadPool instead
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

// TODO: use local-queue(careful re-entrant) implementation
internal sealed class ObserveOnSynchronizationContext<T>(Observable<T> source, SynchronizationContext synchronizationContext) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _ObserveOn(observer, synchronizationContext));
    }

    sealed class _ObserveOn : Observer<T>
    {
        readonly Observer<T> observer;
        readonly SynchronizationContext synchronizationContext;
        SendOrPostCallback onNext;
        SendOrPostCallback onErrorResume;

        protected override bool AutoDisposeOnCompleted => false;

        public _ObserveOn(Observer<T> observer, SynchronizationContext synchronizationContext)
        {
            this.observer = observer;
            this.synchronizationContext = synchronizationContext;
            // make closure(capture observer)
            onNext = PostOnNext;
            onErrorResume = PostOnErrorResume;
        }


        protected override void OnNextCore(T value)
        {
            synchronizationContext.Post(onNext, value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            synchronizationContext.Post(onErrorResume, error);
        }

        protected override void OnCompletedCore(Result result)
        {
            // OnCompletedCore is call once, observer capture here.
            synchronizationContext.Post(_ =>
            {
                try
                {
                    observer.OnCompleted(result);
                }
                finally
                {
                    Dispose();
                }
            }, null);
        }

        void PostOnNext(object? state)
        {
            observer.OnNext((T)state!);
        }

        void PostOnErrorResume(object? state)
        {
            observer.OnErrorResume((Exception)state!);
        }
    }
}

internal sealed class ObserveOnThreadPool<T>(Observable<T> source) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _ObserveOn(observer));
    }

    sealed class _ObserveOn(Observer<T> observer) : Observer<T>
    {
        static readonly Action<_ObserveOn> drainMessages = DrainMessages;

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
                    ThreadPool.UnsafeQueueUserWorkItem(drainMessages, this, preferLocal: false);
                }
            }
        }

        static void DrainMessages(_ObserveOn state)
        {
        AGAIN:
            while (state.q.TryDequeue(out var item))
            {
                switch (item.Kind)
                {
                    case NotificationKind.OnNext:
                        state.observer.OnNext(item.Value!);
                        break;
                    case NotificationKind.OnErrorResume:
                        state.observer.OnErrorResume(item.Error!);
                        break;
                    case NotificationKind.OnCompleted:
                        try
                        {
                            state.observer.OnCompleted(item.Result!.Value);
                        }
                        finally
                        {
                            state.Dispose();
                        }
                        break;
                }
            }

            lock (state.q)
            {
                if (state.q.Count != 0)
                {
                    goto AGAIN;
                }
                state.running = false;
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

            Notification<T> value;
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
                                self.observer.OnCompleted(value.Result!.Value);
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
                catch
                {
                    lock (queue)
                    {
                        if (self.timer != null)
                        {
                            if (queue.Count != 0)
                            {
                                self.timer.RestartImmediately(); // reserve next timer
                            }
                            else
                            {
                                self.running = false;
                            }
                        }
                    }

                    throw; // go to ITimer UnhandledException handler
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
        List<Notification<T>>? listA;
        List<Notification<T>>? listB;
        bool running;
        bool useListA;

        protected override bool AutoDisposeOnCompleted => false;

        public _ObserveOn(Observer<T> observer, FrameProvider frameProvider)
        {
            this.observer = observer;
            this.frameProvider = frameProvider;
            this.running = false;
            this.useListA = true;
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
                if (useListA)
                {
                    if (listA == null) listA = new();
                    listA.Add(value);
                }
                else
                {
                    if (listB == null) listB = new();
                    listB.Add(value);
                }

                if (!running)
                {
                    running = true;
                    frameProvider.Register(this);
                }
            }
        }

        public bool MoveNext(long frameCount)
        {
            List<Notification<T>>? list = null;
            lock (gate)
            {
                if (useListA)
                {
                    list = listA;
                    useListA = false; // switch to listB
                }
                else
                {
                    list = listB;
                    useListA = true; // swith to listA
                }
            }

            if (list == null)
            {
                goto FINALIZE;
            }

            foreach (var value in CollectionsMarshal.AsSpan(list))
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
                                observer.OnCompleted(value.Result!.Value);
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
                if (list != null)
                {
                    list.Clear();
                }

                if (IsDisposed)
                {
                    running = false;
                    return false;
                }

                if ((listA?.Count ?? 0) == 0 && (listB?.Count ?? 0) == 0)
                {
                    running = false;
                    useListA = true;
                    return false;
                }

                return true;
            }
        }

        protected override void DisposeCore()
        {
            lock (gate)
            {
                listA = null; // not call Clear, because list is used in MoveNext
                listB = null;
            }
        }
    }
}
