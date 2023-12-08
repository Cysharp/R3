using R2.Internal;

namespace R2;

public static partial class EventExtensions
{
    public static IEvent<TMessage> DelayFrame<TMessage>(this IEvent<TMessage> source, int delayFrameCount, FrameProvider frameProvider)
    {
        return new DelayFrame<TMessage>(source, delayFrameCount, frameProvider);
    }
}

// TODO:dueTime validation
internal sealed class DelayFrame<TMessage>(IEvent<TMessage> source, int delayFrameCount, FrameProvider frameProvider) : IEvent<TMessage>
{
    public IDisposable Subscribe(ISubscriber<TMessage> subscriber)
    {
        var delay = new _DelayFrame(subscriber, delayFrameCount, frameProvider);
        var sourceSubscription = source.Subscribe(delay);
        var delaySubscription = delay.Subscription;
        return Disposable.Combine(delaySubscription, sourceSubscription); // call delay's dispose first
    }

    class _DelayFrame : ISubscriber<TMessage>, IFrameRunnerWorkItem
    {
        static readonly Action<_DelayFrame> disposeCallback = OnDisposed;

        public CallbackDisposable<_DelayFrame> Subscription { get; private set; }

        readonly ISubscriber<TMessage> subscriber;
        readonly int delayFrameCount;
        readonly FrameProvider frameProvider;
        readonly Queue<(long frameCount, TMessage message)> queue = new Queue<(long, TMessage)>(); // lock gate

        bool running;
        long nextTick;
        bool stopRunner;

        public _DelayFrame(ISubscriber<TMessage> subscriber, int delayFrameCount, FrameProvider frameProvider)
        {
            this.subscriber = subscriber;
            this.delayFrameCount = delayFrameCount;
            this.frameProvider = frameProvider;
            this.Subscription = Disposable.Callback(disposeCallback, this);
        }

        public void OnNext(TMessage message)
        {
            var currentCount = frameProvider.GetFrameCount();
            lock (queue)
            {
                if (Subscription.IsDisposed)
                {
                    return;
                }

                queue.Enqueue((currentCount, message));
                if (queue.Count == 1 && !running)
                {
                    // invoke timer
                    running = true;
                    nextTick = currentCount + delayFrameCount;
                    frameProvider.Register(this); // start runner
                }
            }
        }

        public bool MoveNext(long framecount)
        {
            if (stopRunner)
            {
                return false;
            }

            if (nextTick < framecount)
            {
                return true;
            }

            TMessage message;
            while (true)
            {
                lock (queue)
                {
                    if (Subscription.IsDisposed)
                    {
                        running = false;
                        return false;
                    }

                    if (queue.TryPeek(out var msg))
                    {
                        var elapsed = framecount - msg.frameCount;
                        if (delayFrameCount <= elapsed)
                        {
                            message = queue.Dequeue().message;
                        }
                        else
                        {
                            // invoke timer again
                            nextTick = framecount + (delayFrameCount - elapsed);
                            return true;
                        }
                    }
                    else
                    {
                        // queue is empty, stop timer
                        running = false;
                        return false;
                    }
                }

                try
                {
                    subscriber.OnNext(message);
                    continue; // loop to drain all messages
                }
                catch
                {
                    lock (queue)
                    {
                        if (queue.Count != 0)
                        {
                            nextTick = queue.Peek().frameCount + delayFrameCount;
                            frameProvider.Register(this); // register once more(this loop will be stopped soon)
                        }
                        else
                        {
                            running = false;
                        }
                    }

                    throw; // go to IFrameTimer UnhandledException handler and will Remove this work item
                }
            }
        }

        static void OnDisposed(_DelayFrame self)
        {
            lock (self.queue)
            {
                self.stopRunner = true;
                self.queue.Clear();
            }
        }
    }
}
