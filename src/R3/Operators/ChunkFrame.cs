namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T[]> ChunkFrame<T>(this Observable<T> source)
    {
        return ChunkFrame(source, 0, ObservableSystem.DefaultFrameProvider);
    }

    public static Observable<T[]> ChunkFrame<T>(this Observable<T> source, int frameCount)
    {
        return ChunkFrame(source, frameCount, ObservableSystem.DefaultFrameProvider);
    }

    public static Observable<T[]> ChunkFrame<T>(this Observable<T> source, int frameCount, FrameProvider frameProvider)
    {
        // allow frameCount == 0
        if (frameCount < 0) throw new ArgumentOutOfRangeException("frameCount < 0");
        return new ChunkFrame<T>(source, frameCount.NormalizeFrame(), frameProvider);
    }

    public static Observable<T[]> ChunkFrame<T>(this Observable<T> source, int frameCount, int count)
    {
        return ChunkFrame(source, frameCount, count, ObservableSystem.DefaultFrameProvider);
    }

    public static Observable<T[]> ChunkFrame<T>(this Observable<T> source, int frameCount, int count, FrameProvider frameProvider)
    {
        // allow frameCount == 0
        if (frameCount < 0) throw new ArgumentOutOfRangeException("frameCount < 0");
        return new ChunkFrameCount<T>(source, frameCount.NormalizeFrame(), count, frameProvider);
    }
}

internal sealed class ChunkFrame<T>(Observable<T> source, int frameCount, FrameProvider frameProvider) : Observable<T[]>
{
    protected override IDisposable SubscribeCore(Observer<T[]> observer)
    {
        return source.Subscribe(new _Chunk(observer, frameCount, frameProvider));
    }

    sealed class _Chunk : Observer<T>, IFrameRunnerWorkItem
    {
        readonly Observer<T[]> observer;
        readonly List<T> list; // lock gate
        readonly FrameProvider frameProvider;
        readonly int periodFrame;
        int currentFrame;
        bool running;

        public _Chunk(Observer<T[]> observer, int frameCount, FrameProvider frameProvider)
        {
            this.observer = observer;
            this.periodFrame = frameCount;
            this.frameProvider = frameProvider;
            this.list = new List<T>();
        }

        protected override void OnNextCore(T value)
        {
            lock (list)
            {
                list.Add(value);
                if (!running)
                {
                    currentFrame = 0;
                    running = true;
                    frameProvider.Register(this);
                }
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

        bool IFrameRunnerWorkItem.MoveNext(long _)
        {
            if (this.IsDisposed) return false;

            lock (list)
            {
                if (++currentFrame == periodFrame)
                {
                    observer.OnNext(list.ToArray());
                    list.Clear();
                    running = false;
                    return false;
                }
            }
            return true;
        }
    }
}

internal sealed class ChunkFrameCount<T>(Observable<T> source, int frameCount, int count, FrameProvider frameProvider) : Observable<T[]>
{
    protected override IDisposable SubscribeCore(Observer<T[]> observer)
    {
        return source.Subscribe(new _Chunk(observer, frameCount, count, frameProvider));
    }

    sealed class _Chunk : Observer<T>, IFrameRunnerWorkItem
    {
        readonly Observer<T[]> observer;
        readonly int periodFrame;
        readonly int count;
        readonly object gate = new object();
        readonly FrameProvider frameProvider;
        bool running;
        T[] buffer;
        int index;
        int currentFrame;

        public _Chunk(Observer<T[]> observer, int frameCount, int count, FrameProvider frameProvider)
        {
            this.observer = observer;
            this.periodFrame = frameCount;
            this.count = count;
            this.buffer = new T[count];
            this.frameProvider = frameProvider;
        }

        protected override void OnNextCore(T value)
        {
            lock (gate)
            {
                buffer[index++] = value;
                if (index == count)
                {
                    currentFrame = 0;

                    index = 0;
                    observer.OnNext(buffer);
                    buffer = new T[count];
                }
                else if (!running)
                {
                    currentFrame = 0;
                    running = true;
                    frameProvider.Register(this);
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

        bool IFrameRunnerWorkItem.MoveNext(long _)
        {
            if (this.IsDisposed) return false;

            lock (gate)
            {
                if (index == 0) // stop when count buffer is published
                {
                    running = false;
                    return false;
                }

                if (++currentFrame == periodFrame)
                {
                    var span = buffer.AsSpan(0, index);
                    observer.OnNext(span.ToArray());
                    // reuse buffer
                    span.Clear();
                    index = 0;

                    running = false;
                    return false;
                }
            }
            return true;
        }
    }
}
