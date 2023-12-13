namespace R3;

public static partial class EventExtensions
{
    //public static Event<TMessage> DelayFrame<TMessage>(this Event<TMessage> source, int delayFrameCount, FrameProvider frameProvider)
    //{
    //    return new DelayFrame<TMessage>(source, delayFrameCount, frameProvider);
    //}
}

// TODO:dueTime validation
// TODO:impl minaosi.
//internal sealed class DelayFrame<TMessage>(Event<TMessage> source, int delayFrameCount, FrameProvider frameProvider) : Event<TMessage>
//{
//    protected override IDisposable SubscribeCore(Subscriber<TMessage> subscriber)
//    {
//        var delay = new _DelayFrame(subscriber, delayFrameCount, frameProvider);
//        source.Subscribe(delay); // source subscription is included in _DelayFrame
//        return delay;
//    }

//    class _DelayFrame : Subscriber<TMessage>, IFrameRunnerWorkItem
//    {
//        readonly Subscriber<TMessage> subscriber;
//        readonly int delayFrameCount;
//        readonly FrameProvider frameProvider;
//        readonly Queue<(long frameCount, TMessage message)> queue = new Queue<(long, TMessage)>(); // lock gate

//        bool running;
//        long nextTick;
//        bool stopRunner;

//        public _DelayFrame(Subscriber<TMessage> subscriber, int delayFrameCount, FrameProvider frameProvider)
//        {
//            this.subscriber = subscriber;
//            this.delayFrameCount = delayFrameCount;
//            this.frameProvider = frameProvider;
//        }

//        protected override void OnNextCore(TMessage message)
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

//            TMessage message;
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
//                    subscriber.OnNext(message);
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
