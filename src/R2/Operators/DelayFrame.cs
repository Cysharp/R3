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

    class _DelayFrame : ISubscriber<TMessage>
    {
        static readonly TimerCallback timerCallback = DrainMessages;
        static readonly Action<_DelayFrame> disposeCallback = OnDisposed;

        public CallbackDisposable<_DelayFrame> Subscription { get; private set; }

        readonly ISubscriber<TMessage> subscriber;
        readonly int delayFrameCount;
        readonly FrameProvider frameProvider;
        readonly IFrameTimer timer;
        readonly Queue<(int frameCount, TMessage message)> queue = new Queue<(int, TMessage)>(); // lock gate

        bool running;

        public _DelayFrame(ISubscriber<TMessage> subscriber, int delayFrameCount, FrameProvider frameProvider)
        {
            this.subscriber = subscriber;
            this.delayFrameCount = delayFrameCount;
            this.frameProvider = frameProvider;
            this.timer = frameProvider.CreateStoppedTimer(timerCallback, this);
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
                    timer.InvokeOnce(delayFrameCount);
                }
            }
        }

        static void DrainMessages(object? state)
        {
            var self = (_DelayFrame)state!;
            var queue = self.queue;

            TMessage message;
            while (true)
            {
                lock (queue)
                {
                    if (self.Subscription.IsDisposed)
                    {
                        self.running = false;
                        return;
                    }

                    if (queue.TryPeek(out var msg))
                    {
                        var elapsed = self.frameProvider.GetElapsedFrameCount(msg.frameCount);
                        if (self.delayFrameCount <= elapsed)
                        {
                            message = queue.Dequeue().message;
                        }
                        else
                        {
                            // invoke timer again
                            self.timer.InvokeOnce(self.delayFrameCount - elapsed);
                            return;
                        }
                    }
                    else
                    {
                        // queue is empty, stop timer
                        self.running = false;
                        return;
                    }
                }

                try
                {
                    self.subscriber.OnNext(message);
                    continue; // loop to drain all messages
                }
                catch
                {
                    lock (queue)
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

                    throw; // go to IFrameTimer UnhandledException handler
                }
            }
        }

        static void OnDisposed(_DelayFrame self)
        {
            lock (self.queue)
            {
                self.timer.Dispose();
                self.queue.Clear();
            }
        }
    }
}
