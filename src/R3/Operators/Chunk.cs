namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T[]> Chunk<T>(this Observable<T> source, int count)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException("count <= 0");
        return new Chunk<T>(source, count);
    }

    public static Observable<T[]> Chunk<T>(this Observable<T> source, TimeSpan timeSpan)
    {
        return Chunk(source, timeSpan, ObservableSystem.DefaultTimeProvider);
    }

    public static Observable<T[]> Chunk<T>(this Observable<T> source, TimeSpan timeSpan, TimeProvider timeProvider)
    {
        return new ChunkTime<T>(source, timeSpan.Normalize(), timeProvider);
    }

    public static Observable<T[]> Chunk<T>(this Observable<T> source, TimeSpan timeSpan, int count)
    {
        return Chunk(source, timeSpan, count, ObservableSystem.DefaultTimeProvider);
    }

    public static Observable<T[]> Chunk<T>(this Observable<T> source, TimeSpan timeSpan, int count, TimeProvider timeProvider)
    {
        return new ChunkTimeCount<T>(source, timeSpan.Normalize(), count, timeProvider);
    }

    public static Observable<TSource[]> Chunk<TSource, TWindowBoundary>(this Observable<TSource> source, Observable<TWindowBoundary> windowBoundaries)
    {
        return new ChunkWindow<TSource, TWindowBoundary>(source, windowBoundaries);
    }
}

// Count
internal sealed class Chunk<T>(Observable<T> source, int count) : Observable<T[]>
{
    protected override IDisposable SubscribeCore(Observer<T[]> observer)
    {
        return source.Subscribe(new _Chunk(observer, count));
    }

    sealed class _Chunk(Observer<T[]> observer, int count) : Observer<T>
    {
        T[] buffer = new T[count];
        int index;

        protected override void OnNextCore(T value)
        {
            buffer[index++] = value;
            if (index == count)
            {
                index = 0;
                observer.OnNext(buffer);
                buffer = new T[count];
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            if (index > 0)
            {
                observer.OnNext(buffer.AsSpan(0, index).ToArray());
            }

            observer.OnCompleted(result);
        }
    }
}

// Time
internal sealed class ChunkTime<T>(Observable<T> source, TimeSpan timeSpan, TimeProvider timeProvider) : Observable<T[]>
{
    protected override IDisposable SubscribeCore(Observer<T[]> observer)
    {
        return source.Subscribe(new _Chunk(observer, timeSpan, timeProvider));
    }

    sealed class _Chunk : Observer<T>
    {
        static readonly TimerCallback timerCallback = TimerCallback;

        readonly Observer<T[]> observer;
        readonly List<T> list; // lock gate
        readonly ITimer timer;

        public _Chunk(Observer<T[]> observer, TimeSpan timeSpan, TimeProvider timeProvider)
        {
            this.observer = observer;
            this.list = new List<T>();
            this.timer = timeProvider.CreateStoppedTimer(timerCallback, this);
            this.timer.Change(timeSpan, timeSpan);
        }

        protected override void OnNextCore(T value)
        {
            lock (list)
            {
                list.Add(value);
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            lock (list)
            {
                if (list.Count > 0)
                {
                    observer.OnNext(list.ToArray());
                    list.Clear();
                }
            }
            observer.OnCompleted(result);
        }

        protected override void DisposeCore()
        {
            timer.Dispose();
        }

        static void TimerCallback(object? state)
        {
            var self = (_Chunk)state!;
            lock (self.list)
            {
                if (self.list.Count == 0)
                {
                    self.observer.OnNext(Array.Empty<T>());
                }
                else
                {
                    self.observer.OnNext(self.list.ToArray());
                    self.list.Clear();
                }
            }
        }
    }
}

// Time + Count
internal sealed class ChunkTimeCount<T>(Observable<T> source, TimeSpan timeSpan, int count, TimeProvider timeProvider) : Observable<T[]>
{
    protected override IDisposable SubscribeCore(Observer<T[]> observer)
    {
        return source.Subscribe(new _Chunk(observer, timeSpan, count, timeProvider));
    }

    sealed class _Chunk : Observer<T>
    {
        static readonly TimerCallback timerCallback = TimerCallback;

        readonly Observer<T[]> observer;
        readonly ITimer timer;
        readonly int count;
        readonly TimeSpan timeSpan;
        readonly object gate = new object();
        T[] buffer;
        int index;
        int timerId;

        public _Chunk(Observer<T[]> observer, TimeSpan timeSpan, int count, TimeProvider timeProvider)
        {
            this.observer = observer;
            this.count = count;
            this.timeSpan = timeSpan;
            this.buffer = new T[count];
            this.timer = timeProvider.CreateStoppedTimer(timerCallback, this);
            this.timer.Change(timeSpan, timeSpan);
        }

        protected override void OnNextCore(T value)
        {
            lock (gate)
            {
                buffer[index++] = value;
                if (index == count)
                {
                    timer.Stop(); // stop timer for restart

                    try
                    {
                        index = 0;
                        observer.OnNext(buffer);
                        buffer = new T[count];
                    }
                    finally
                    {
                        // Restart and increment timerId
                        timerId = unchecked(timerId += 1);
                        timer.Change(timeSpan, timeSpan);
                    }
                }
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            lock (gate)
            {
                if (index > 0)
                {
                    observer.OnNext(buffer.AsSpan(0, index).ToArray());
                }
            }
            observer.OnCompleted(result);
        }

        protected override void DisposeCore()
        {
            timer.Dispose();
        }

        static void TimerCallback(object? state)
        {
            var self = (_Chunk)state!;
            var id = Volatile.Read(ref self.timerId);
            lock (self.gate)
            {
                if (Volatile.Read(ref self.timerId) != id) return; // check timer restarted

                if (self.index == 0)
                {
                    self.observer.OnNext(Array.Empty<T>());
                }
                else
                {
                    var span = self.buffer.AsSpan(0, self.index);
                    self.observer.OnNext(span.ToArray());
                    // reuse buffer
                    span.Clear();
                    self.index = 0;
                }
            }
        }
    }
}

// Window
internal sealed class ChunkWindow<T, TWindowBoundary>(Observable<T> source, Observable<TWindowBoundary> windowBoundaries) : Observable<T[]>
{
    protected override IDisposable SubscribeCore(Observer<T[]> observer)
    {
        return new _Chunk(observer).Run(source, windowBoundaries);
    }

    sealed class _Chunk(Observer<T[]> observer) : Observer<T>
    {
        readonly Observer<T[]> observer = observer;
        readonly List<T> list = new List<T>();
        IDisposable? windowSubscription;

        public IDisposable Run(Observable<T> source, Observable<TWindowBoundary> windowBoundaries)
        {
            this.windowSubscription = windowBoundaries.Subscribe(new WindowBoundaryObserver(this));
            try
            {
                return source.Subscribe(this);
            }
            catch
            {
                windowSubscription.Dispose();
                throw;
            }
        }

        protected override void OnNextCore(T value)
        {
            lock (list)
            {
                list.Add(value);
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            lock (list)
            {
                if (list.Count > 0)
                {
                    observer.OnNext(list.ToArray());
                    list.Clear();
                }
            }
            observer.OnCompleted(result);
        }

        protected override void DisposeCore()
        {
            windowSubscription?.Dispose();
        }

        sealed class WindowBoundaryObserver(_Chunk parent) : Observer<TWindowBoundary>
        {
            protected override void OnNextCore(TWindowBoundary _)
            {
                lock (parent.list)
                {
                    if (parent.list.Count == 0)
                    {
                        parent.observer.OnNext(Array.Empty<T>());
                    }
                    else
                    {
                        parent.observer.OnNext(parent.list.ToArray());
                        parent.list.Clear();
                    }
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                parent.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                parent.OnCompleted();
            }
        }
    }
}
