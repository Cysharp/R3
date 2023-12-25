using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace R3;

public static partial class ObservableExtensions
{
    // TODO: this is working space, will remove this file after complete.


    // AsUnitObservable

    // Time based
    // Debounce, Throttle, ThrottleFirst, Sample, Delay, DelaySubscription
    // + frame variation

    // TImeInterval <-> FrameInterval

    // OnErrorStop

    // Observe

    // Rx Merging:
    //CombineLatest, Merge, Zip, WithLatestFrom, ZipLatest, Switch, MostRecent

    // Standard Query:
    // Concat, Append, Prepend, Distinct, DistinctUntilChanged, Scan, Select, SelectMany

    // SkipTake:
    // Skip, SkipLast, SkipUntil, SkipWhile

    // return tasks:
    // All, Any, Contains, SequenceEqual, IsEmpty, MaxBy, MinBy, ToDictionary, ToLookup,



    // ObserveOn extension method
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
                observer.OnCompleted(result);
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
        static readonly Action<(Observer<T>, T)> onNext = PostOnNext;
        static readonly Action<(Observer<T>, Exception)> onErrorResume = PostOnErrorResume;
        static readonly Action<(Observer<T>, Result)> onCompleted = PostOnCompleted;

        protected override void OnNextCore(T value)
        {
            var item = PooledThreadPoolWorkItem<(Observer<T>, T)>.Create(onNext, (observer, value));
            ThreadPool.UnsafeQueueUserWorkItem(item, preferLocal: false);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            var item = PooledThreadPoolWorkItem<(Observer<T>, Exception)>.Create(onErrorResume, (observer, error));
            ThreadPool.UnsafeQueueUserWorkItem(item, preferLocal: false);
        }

        protected override void OnCompletedCore(Result result)
        {
            var item = PooledThreadPoolWorkItem<(Observer<T>, Result)>.Create(onCompleted, (observer, result));
            ThreadPool.UnsafeQueueUserWorkItem(item, preferLocal: false);
        }

        static void PostOnNext((Observer<T>, T) state)
        {
            state.Item1.OnNext(state.Item2);
        }

        static void PostOnErrorResume((Observer<T>, Exception) state)
        {
            state.Item1.OnErrorResume(state.Item2);
        }

        static void PostOnCompleted((Observer<T>, Result) state)
        {
            state.Item1.OnCompleted(state.Item2);
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
                            self.observer.OnCompleted(value.Result!.Value);
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
        RunningState state;

        public _ObserveOn(Observer<T> observer, FrameProvider frameProvider)
        {
            this.observer = observer;
            this.frameProvider = frameProvider;
            this.state = RunningState.Stop;
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
                switch (state)
                {
                    case ObserveOnFrameProvider<T>.RunningState.Stop:
                        if (listA == null) listA = new();
                        listA.Add(value);
                        frameProvider.Register(this);
                        break;
                    case ObserveOnFrameProvider<T>.RunningState.ListA:
                        if (listB == null) listB = new();
                        listB.Add(value);
                        break;
                    case ObserveOnFrameProvider<T>.RunningState.ListB:
                        if (listA == null) listA = new();
                        listA.Add(value);
                        break;
                    default:
                        break;
                }
            }
        }

        public bool MoveNext(long frameCount)
        {
            List<Notification<T>>? list = null;
            lock (gate)
            {
                switch (state)
                {
                    case ObserveOnFrameProvider<T>.RunningState.ListA:
                        list = listA;
                        state = RunningState.ListB;
                        break;
                    case ObserveOnFrameProvider<T>.RunningState.ListB:
                        list = listB;
                        state = RunningState.ListA;
                        break;
                    default:
                        break;
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
                            observer.OnCompleted(value.Result!.Value);
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
                if (IsDisposed)
                {
                    if (list != null)
                    {
                        list.Clear();
                    }
                    state = RunningState.Stop;
                    return false;
                }

                switch (state)
                {
                    case ObserveOnFrameProvider<T>.RunningState.ListA:
                        listB?.Clear();
                        break;
                    case ObserveOnFrameProvider<T>.RunningState.ListB:
                        listA?.Clear();
                        break;
                    default:
                        break;
                }

                if (listA?.Count == 0 && listB?.Count == 0)
                {
                    state = RunningState.Stop;
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

    enum RunningState
    {
        Stop, ListA, ListB
    }
}
