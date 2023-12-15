namespace R3;

public static partial class ObservableExtensions
{
    //public static Event<T> DelayFrame<T>(this Event<T> source, int delayFrameCount, FrameProvider frameProvider)
    //{
    //    return new DelayFrame<T>(source, delayFrameCount, frameProvider);
    //}
}

// TODO:dueTime validation
// TODO:impl minaosi.
//internal sealed class DelayFrame<T>(Event<T> source, int delayFrameCount, FrameProvider frameProvider) : Event<T>
//{
//    protected override IDisposable SubscribeCore(observer<T> observer)
//    {
//        var delay = new _DelayFrame(observer, delayFrameCount, frameProvider);
//        source.Subscribe(delay); // source subscription is included in _DelayFrame
//        return delay;
//    }

//    class _DelayFrame : observer<T>, IFrameRunnerWorkItem
//    {
//        readonly observer<T> observer;
//        readonly int delayFrameCount;
//        readonly FrameProvider frameProvider;
//        readonly Queue<(long frameCount, T value)> queue = new Queue<(long, TMessage)>(); // lock gate

//        bool running;
//        long nextTick;
//        bool stopRunner;

//        public _DelayFrame(observer<T> observer, int delayFrameCount, FrameProvider frameProvider)
//        {
//            this.observer = observer;
//            this.delayFrameCount = delayFrameCount;
//            this.frameProvider = frameProvider;
//        }

//        protected override void OnNextCore(T value)
//        {
//            var currentCount = frameProvider.GetFrameCount();
//            lock (queue)
//            {
//                if (IsDisposed)
//                {
//                    return;
//                }

//                queue.Enqueue((currentCount, message));
//                if (queue.Count == 1 && !running)
//                {
//                    // invoke timer
//                    running = true;
//                    nextTick = currentCount + delayFrameCount;
//                    frameProvider.Register(this); // start runner
//                }
//            }
//        }

//        protected override void OnErrorResumeCore(Exception error)
//        {
//            // TODO:not yet
//            throw new NotImplementedException();
//        }

//        public bool MoveNext(long framecount)
//        {
//            if (stopRunner)
//            {
//                return false;
//            }

//            if (nextTick < framecount)
//            {
//                return true;
//            }

//            T value;
//            while (true)
//            {
//                lock (queue)
//                {
//                    if (IsDisposed)
//                    {
//                        running = false;
//                        return false;
//                    }

//                    if (queue.TryPeek(out var msg))
//                    {
//                        var elapsed = framecount - msg.frameCount;
//                        if (delayFrameCount <= elapsed)
//                        {
//                            message = queue.Dequeue().message;
//                        }
//                        else
//                        {
//                            // invoke timer again
//                            nextTick = framecount + (delayFrameCount - elapsed);
//                            return true;
//                        }
//                    }
//                    else
//                    {
//                        // queue is empty, stop timer
//                        running = false;
//                        return false;
//                    }
//                }

//                try
//                {
//                    observer.OnNext(value);
//                    continue; // loop to drain all messages
//                }
//                catch
//                {
//                    lock (queue)
//                    {
//                        if (queue.Count != 0)
//                        {
//                            nextTick = queue.Peek().frameCount + delayFrameCount;
//                            frameProvider.Register(this); // register once more(this loop will be stopped soon)
//                        }
//                        else
//                        {
//                            running = false;
//                        }
//                    }

//                    throw; // go to IFrameTimer UnhandledException handler and will Remove this work item
//                }
//            }
//        }

//        protected override void DisposeCore()
//        {
//            lock (queue)
//            {
//                stopRunner = true;
//                queue.Clear();
//            }
//        }
//    }
//}
