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
        readonly int periodFrame;
        int currentFrame;

        public _Chunk(Observer<T[]> observer, int frameCount, FrameProvider frameProvider)
        {
            this.observer = observer;
            this.periodFrame = frameCount;
            this.list = new List<T>();
            frameProvider.Register(this);
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

        bool IFrameRunnerWorkItem.MoveNext(long _)
        {
            if (this.IsDisposed) return false;

            lock (list)
            {
                if (++currentFrame == periodFrame)
                {
                    currentFrame = 0;
                    if (list.Count == 0)
                    {
                        observer.OnNext(Array.Empty<T>());
                    }
                    else
                    {
                        observer.OnNext(list.ToArray());
                        list.Clear();
                    }
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
        T[] buffer;
        int index;
        int currentFrame;

        public _Chunk(Observer<T[]> observer, int frameCount, int count, FrameProvider frameProvider)
        {
            this.observer = observer;
            this.periodFrame = frameCount;
            this.count = count;
            this.buffer = new T[count];
            frameProvider.Register(this);
        }

        protected override void OnNextCore(T value)
        {
            lock (gate)
            {
                buffer[index++] = value;
                if (index == count)
                {
                    // reset currentFrame
                    currentFrame = 0;

                    index = 0;
                    observer.OnNext(buffer);
                    buffer = new T[count];
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
                if (++currentFrame == periodFrame)
                {
                    currentFrame = 0;
                    if (index == 0)
                    {
                        observer.OnNext(Array.Empty<T>());
                    }
                    else
                    {
                        var span = buffer.AsSpan(0, index);
                        observer.OnNext(span.ToArray());
                        // reuse buffer
                        span.Clear();
                        index = 0;
                    }
                }
            }
            return true;
        }
    }
}
