using R3.Internal;

namespace R3
{
    public static partial class EventExtensions
    {
        public static Event<TMessage> Delay<TMessage>(this Event<TMessage> source, TimeSpan dueTime, TimeProvider timeProvider)
        {
            return new Delay<TMessage>(source, dueTime, timeProvider);
        }

        //public static ICompletableEvent<TMessage, TComplete> Delay<TMessage, TComplete>(this ICompletableEvent<TMessage, TComplete> source, TimeSpan dueTime, TimeProvider timeProvider)
        //{
        //    return new Delay<TMessage, TComplete>(source, dueTime, timeProvider);
        //}
    }
}

namespace R3.Operators
{
    // TODO:dueTime validation
    internal sealed class Delay<TMessage>(Event<TMessage> source, TimeSpan dueTime, TimeProvider timeProvider) : Event<TMessage>
    {
        protected override IDisposable SubscribeCore(Subscriber<TMessage> subscriber)
        {
            var delay = new _Delay(subscriber, dueTime, timeProvider);
            source.Subscribe(delay);
            return delay;
        }

        class _Delay : Subscriber<TMessage>, IDisposable
        {
            static readonly TimerCallback timerCallback = DrainMessages;

            readonly Subscriber<TMessage> subscriber;
            readonly TimeSpan dueTime;
            readonly TimeProvider timeProvider;
            ITimer? timer;
            readonly Queue<(long timestamp, TMessage message)> queue = new Queue<(long, TMessage)>(); // lock gate

            bool running;

            public _Delay(Subscriber<TMessage> subscriber, TimeSpan dueTime, TimeProvider timeProvider)
            {
                this.subscriber = subscriber;
                this.dueTime = dueTime;
                this.timeProvider = timeProvider;
                this.timer = timeProvider.CreateStoppedTimer(timerCallback, this);
            }

            public override void OnNext(TMessage message)
            {
                var timestamp = timeProvider.GetTimestamp();
                lock (queue)
                {
                    if (timer == null)
                    {
                        return;
                    }

                    queue.Enqueue((timestamp, message));
                    if (queue.Count == 1 && !running)
                    {
                        // invoke timer
                        running = true;
                        timer.InvokeOnce(dueTime);
                    }
                }
            }

            static void DrainMessages(object? state)
            {
                var self = (_Delay)state!;
                var queue = self.queue;

                TMessage message;
                while (true)
                {
                    lock (queue)
                    {
                        if (self.timer == null)
                        {
                            self.running = false;
                            return;
                        }

                        if (queue.TryPeek(out var msg))
                        {
                            var elapsed = self.timeProvider.GetElapsedTime(msg.timestamp);
                            if (self.dueTime <= elapsed)
                            {
                                message = queue.Dequeue().message;
                            }
                            else
                            {
                                // invoke timer again
                                self.timer.InvokeOnce(self.dueTime - elapsed);
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
                    timer?.Dispose();
                    timer = null!;
                    queue.Clear();
                }
            }
        }
    }

    //internal sealed class Delay<TMessage, TComplete>(ICompletableEvent<TMessage, TComplete> source, TimeSpan dueTime, TimeProvider timeProvider) : ICompletableEvent<TMessage, TComplete>
    //{
    //    public IDisposable Subscribe(ISubscriber<TMessage, TComplete> subscriber)
    //    {
    //        var delay = new _Delay(subscriber, dueTime, timeProvider);
    //        var sourceSubscription = source.Subscribe(delay);
    //        var delaySubscription = delay.Subscription;
    //        return Disposable.Combine(delaySubscription, sourceSubscription); // call delay's dispose first
    //    }

    //    class _Delay : ISubscriber<TMessage, TComplete>
    //    {
    //        static readonly TimerCallback timerCallback = DrainMessages;
    //        static readonly Action<object?> disposeCallback = OnDisposed;

    //        public CallbackDisposable Subscription { get; private set; }

    //        readonly ISubscriber<TMessage, TComplete> subscriber;
    //        readonly TimeSpan dueTime;
    //        readonly TimeProvider timeProvider;
    //        readonly ITimer timer;
    //        readonly Queue<(long timestamp, TMessage message)> queue = new Queue<(long, TMessage)>(); // lock gate

    //        bool running;

    //        // for Completed event
    //        TComplete? completeMessage;
    //        DateTimeOffset completeAt;
    //        bool isCompleted;


    //        public _Delay(ISubscriber<TMessage, TComplete> subscriber, TimeSpan dueTime, TimeProvider timeProvider)
    //        {
    //            this.dueTime = dueTime;
    //            this.subscriber = subscriber;
    //            this.timeProvider = timeProvider;
    //            this.timer = timeProvider.CreateStoppedTimer(timerCallback, this);
    //            this.Subscription = new CallbackDisposable(disposeCallback, this);
    //        }

    //        public void OnNext(TMessage message)
    //        {
    //            var current = timeProvider.GetTimestamp();
    //            lock (queue)
    //            {
    //                if (Subscription.IsDisposed)
    //                {
    //                    return;
    //                }

    //                queue.Enqueue((current, message));
    //                if (queue.Count == 1 && !running)
    //                {
    //                    // invoke timer
    //                    running = true;
    //                    timer.Change(dueTime, dueTime);
    //                }
    //            }
    //        }

    //        public void OnCompleted(TComplete complete)
    //        {
    //            var completeAt = timeProvider.GetUtcNow() + dueTime;
    //            lock (queue)
    //            {
    //                if (Subscription.IsDisposed)
    //                {
    //                    return;
    //                }

    //                this.completeAt = completeAt;
    //                this.completeMessage = complete;
    //                this.isCompleted = true;

    //                if (queue.Count == 0 && !running)
    //                {
    //                    // invoke timer
    //                    running = true;
    //                    timer.Change(dueTime, dueTime);
    //                }
    //            }
    //        }

    //        static void DrainMessages(object? state)
    //        {
    //            var self = (_Delay)state!;
    //            var queue = self.queue;

    //            TMessage message;
    //            bool invokeCompleted = false;
    //            while (true)
    //            {
    //                lock (queue)
    //                {
    //                    if (self.Subscription.IsDisposed)
    //                    {
    //                        self.running = false;
    //                        return;
    //                    }

    //                    if (queue.TryPeek(out var msg))
    //                    {
    //                        var elapsed = self.timeProvider.GetElapsedTime(msg.timestamp);
    //                        if (elapsed <= self.dueTime)
    //                        {
    //                            message = queue.Dequeue().message;
    //                        }
    //                        else
    //                        {
    //                            // invoke timer again
    //                            self.timer.InvokeOnce(self.dueTime - elapsed);
    //                            return;
    //                        }
    //                    }
    //                    else
    //                    {
    //                        // queue is empty, check completed
    //                        if (self.isCompleted)
    //                        {
    //                            invokeCompleted = true;
    //                            break;
    //                        }

    //                        // queue is empty, stop timer
    //                        self.running = false;
    //                        return;
    //                    }
    //                }

    //                if (!invokeCompleted)
    //                {
    //                    try
    //                    {
    //                        self.subscriber.OnNext(message);
    //                        continue; // succeed, loop to drain all messages
    //                    }
    //                    catch
    //                    {
    //                        lock (queue)
    //                        {
    //                            if (queue.TryPeek(out var msg))
    //                            {
    //                                var now = self.timeProvider.GetUtcNow();
    //                                if (msg.nextTime <= now)
    //                                {
    //                                    // yield timer immediately
    //                                    self.timer.RestartImmediately();
    //                                }
    //                                else
    //                                {
    //                                    // invoke timer again
    //                                    self.timer.InvokeOnce(msg.nextTime - now);
    //                                }
    //                            }
    //                            else
    //                            {
    //                                self.running = false;
    //                            }
    //                        }
    //                        return;
    //                    }
    //                }
    //                else
    //                {
    //                    try
    //                    {
    //                        self.subscriber.OnCompleted(self.completeMessage!);
    //                    }
    //                    finally
    //                    {
    //                        self.Subscription.Dispose(); // stop self
    //                    }
    //                    return;
    //                }
    //            }
    //        }

    //        static void OnDisposed(object? state)
    //        {
    //            var self = (_Delay)state!;
    //            lock (self.queue)
    //            {
    //                self.timer.Dispose();
    //                self.queue.Clear();
    //            }
    //        }
    //    }
    //}
}
