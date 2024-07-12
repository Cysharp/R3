namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> SubscribeOnCurrentSynchronizationContext<T>(this Observable<T> source)
    {
        return SubscribeOn<T>(source, SynchronizationContext.Current);
    }

    public static Observable<T> SubscribeOnThreadPool<T>(this Observable<T> source)
    {
        return new SubscribeOnThreadPool<T>(source);
    }

    public static Observable<T> SubscribeOnSynchronize<T>(this Observable<T> source, object gate, bool rawObserver = false)
    {
        return new SubscribeOnSynchronize<T>(source, gate, rawObserver);
    }

    public static Observable<T> SubscribeOn<T>(this Observable<T> source, SynchronizationContext? synchronizationContext)
    {
        if (synchronizationContext == null)
        {
            return new SubscribeOnThreadPool<T>(source); // use ThreadPool instead
        }

        return new SubscribeOnSynchronizationContext<T>(source, synchronizationContext);
    }

    public static Observable<T> SubscribeOn<T>(this Observable<T> source, TimeProvider timeProvider)
    {
        if (timeProvider == TimeProvider.System)
        {
            return new SubscribeOnThreadPool<T>(source);
        }

        return new SubscribeOnTimeProvider<T>(source, timeProvider);
    }

    public static Observable<T> SubscribeOn<T>(this Observable<T> source, FrameProvider frameProvider)
    {
        return new SubscribeOnFrameProvider<T>(source, frameProvider);
    }
}

internal sealed class SubscribeOnSynchronizationContext<T>(Observable<T> source, SynchronizationContext synchronizationContext) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return new _SubscribeOn(observer, source, synchronizationContext).Run();
    }

    sealed class _SubscribeOn : Observer<T>
    {
        static readonly SendOrPostCallback postCallback = Subscribe;

        readonly Observer<T> observer;
        readonly Observable<T> source;
        readonly SynchronizationContext synchronizationContext;
        SingleAssignmentDisposableCore disposable;

        public _SubscribeOn(Observer<T> observer, Observable<T> source, SynchronizationContext synchronizationContext)
        {
            this.observer = observer;
            this.source = source;
            this.synchronizationContext = synchronizationContext;
        }

        public IDisposable Run()
        {
            synchronizationContext.Post(postCallback, this);
            return this;
        }

        static void Subscribe(object? state)
        {
            var self = (_SubscribeOn)state!;
            self.disposable.Disposable = self.source.Subscribe(self);
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

internal sealed class SubscribeOnThreadPool<T>(Observable<T> source) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return new _SubscribeOn(observer, source).Run();
    }

    sealed class _SubscribeOn : Observer<T>, IThreadPoolWorkItem
    {
        readonly Observer<T> observer;
        readonly Observable<T> source;
        SingleAssignmentDisposableCore disposable;

        public _SubscribeOn(Observer<T> observer, Observable<T> source)
        {
            this.observer = observer;
            this.source = source;
        }

        public IDisposable Run()
        {
            ThreadPool.UnsafeQueueUserWorkItem(this, preferLocal: false);
            return this;
        }

        public void Execute()
        {
            try
            {
                disposable.Disposable = source.Subscribe(this);
            }
            catch (Exception ex)
            {
                ObservableSystem.GetUnhandledExceptionHandler().Invoke(ex);
                Dispose();
            }
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

internal sealed class SubscribeOnSynchronize<T>(Observable<T> source, object gate, bool rawObserver) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        observer = rawObserver ? observer : observer.Wrap();
        lock (gate)
        {
            return source.Subscribe(observer);
        }
    }
}

internal sealed class SubscribeOnTimeProvider<T>(Observable<T> source, TimeProvider timeProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return new _SubscribeOn(observer, source, timeProvider).Run();
    }

    sealed class _SubscribeOn : Observer<T>
    {
        static readonly TimerCallback timerCallback = Subscribe;

        readonly Observer<T> observer;
        readonly Observable<T> source;
        readonly TimeProvider timeProvider;
        readonly ITimer timer;
        SingleAssignmentDisposableCore disposable;

        public _SubscribeOn(Observer<T> observer, Observable<T> source, TimeProvider timeProvider)
        {
            this.observer = observer;
            this.source = source;
            this.timeProvider = timeProvider;
            this.timer = timeProvider.CreateStoppedTimer(timerCallback, this);
        }

        public IDisposable Run()
        {
            timer.RestartImmediately();
            return this;
        }

        static void Subscribe(object? state)
        {
            var self = (_SubscribeOn)state!;
            self.disposable.Disposable = self.source.Subscribe(self);
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
            timer.Dispose();
            disposable.Dispose();
        }
    }
}

internal sealed class SubscribeOnFrameProvider<T>(Observable<T> source, FrameProvider frameProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return new _SubscribeOn(observer, source, frameProvider).Run();
    }

    sealed class _SubscribeOn : Observer<T>, IFrameRunnerWorkItem
    {
        static readonly SendOrPostCallback postCallback = Subscribe;

        readonly Observer<T> observer;
        readonly Observable<T> source;
        readonly FrameProvider frameProvider;
        SingleAssignmentDisposableCore disposable;

        public _SubscribeOn(Observer<T> observer, Observable<T> source, FrameProvider frameProvider)
        {
            this.observer = observer;
            this.source = source;
            this.frameProvider = frameProvider;
        }

        public IDisposable Run()
        {
            frameProvider.Register(this);
            return this;
        }

        static void Subscribe(object? state)
        {
            var self = (_SubscribeOn)state!;
            self.disposable.Disposable = self.source.Subscribe(self);
        }

        bool IFrameRunnerWorkItem.MoveNext(long frameCount)
        {
            if (disposable.IsDisposed) return false;

            disposable.Disposable = source.Subscribe(this);
            return false;
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
