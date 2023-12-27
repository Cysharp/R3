using R3.Internal;

namespace R3;

//TODO:not yet implemented.
public static partial class ObservableExtensions
{
    //public static Event<T> Delay<T>(this Event<T> source, TimeSpan dueTime, TimeProvider timeProvider)
    //{
    //    return new Delay<T>(source, dueTime, timeProvider);
    //}

    //public static ICompletableEvent<T> Delay<T>(this ICompletableEvent<T> source, TimeSpan dueTime, TimeProvider timeProvider)
    //{
    //    return new Delay<T>(source, dueTime, timeProvider);
    //}
}

// TODO:dueTime validation
//internal sealed class Delay<T>(Event<T> source, TimeSpan dueTime, TimeProvider timeProvider) : Event<T>
//{
//    protected override IDisposable SubscribeCore(observer<T> observer)
//    {
//        var delay = new _Delay(observer, dueTime, timeProvider);
//        source.Subscribe(delay);
//        return delay;
//    }

//    class _Delay : observer<T>, IDisposable
//    {
//        static readonly TimerCallback timerCallback = DrainMessages;

//        readonly observer<T> observer;
//        readonly TimeSpan dueTime;
//        readonly TimeProvider timeProvider;
//        ITimer? timer;
//        readonly Queue<(long timestamp, T value)> queue = new Queue<(long, TMessage)>(); // lock gate

//        bool running;

//        public _Delay(observer<T> observer, TimeSpan dueTime, TimeProvider timeProvider)
//        {
//            this.observer = observer;
//            this.dueTime = dueTime;
//            this.timeProvider = timeProvider;
//            this.timer = timeProvider.CreateStoppedTimer(timerCallback, this);
//        }

//        protected override void OnNextCore(T value)
//        {
//            var timestamp = timeProvider.GetTimestamp();
//            lock (queue)
//            {
//                if (timer == null)
//                {
//                    return;
//                }

//                queue.Enqueue((timestamp, message));
//                if (queue.Count == 1 && !running)
//                {
//                    // invoke timer
//                    running = true;
//                    timer.InvokeOnce(dueTime);
//                }
//            }
//        }

//        protected override void OnErrorResumeCore(Exception error)
//        {
//            // TODO: what should we do?
//            throw new NotImplementedException();
//        }

//        static void DrainMessages(object? state)
//        {
//            var self = (_Delay)state!;
//            var queue = self.queue;

//            T value;
//            while (true)
//            {
//                lock (queue)
//                {
//                    if (self.timer == null)
//                    {
//                        self.running = false;
//                        return;
//                    }

//                    if (queue.TryPeek(out var msg))
//                    {
//                        var elapsed = self.timeProvider.GetElapsedTime(msg.timestamp);
//                        if (self.dueTime <= elapsed)
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
//                        // queue is empty, stop timer
//                        self.running = false;
//                        return;
//                    }
//                }

//                try
//                {
//                    self.observer.OnNext(value);
//                    continue; // loop to drain all messages
//                }
//                catch
//                {
//                    lock (queue)
//                    {
//                        if (self.timer != null)
//                        {
//                            if (queue.Count != 0)
//                            {
//                                self.timer.RestartImmediately(); // reserve next timer
//                            }
//                            else
//                            {
//                                self.running = false;
//                            }
//                        }
//                    }

//                    throw; // go to ITimer UnhandledException handler
//                }
//            }
//        }

//        protected override void DisposeCore()
//        {
//            lock (queue)
//            {
//                timer?.Dispose();
//                timer = null!;
//                queue.Clear();
//            }
//        }
//    }
//}

//internal sealed class Delay<T>(ICompletableEvent<T> source, TimeSpan dueTime, TimeProvider timeProvider) : ICompletableEvent<T>
//{
//    public IDisposable Subscribe(Iobserver<T> observer)
//    {
//        var delay = new _Delay(observer, dueTime, timeProvider);
//        var sourceSubscription = source.Subscribe(delay);
//        var delaySubscription = delay.Subscription;
//        return Disposable.Combine(delaySubscription, sourceSubscription); // call delay's dispose first
//    }

//    class _Delay : Iobserver<T>
//    {
//        static readonly TimerCallback timerCallback = DrainMessages;
//        static readonly Action<object?> disposeCallback = OnDisposed;

//        public CallbackDisposable Subscription { get; private set; }

//        readonly Iobserver<T> observer;
//        readonly TimeSpan dueTime;
//        readonly TimeProvider timeProvider;
//        readonly ITimer timer;
//        readonly Queue<(long timestamp, T value)> queue = new Queue<(long, TMessage)>(); // lock gate

//        bool running;

//        // for Completed event
//        Result? completeMessage;
//        DateTimeOffset completeAt;
//        bool isCompleted;


//        public _Delay(Iobserver<T> observer, TimeSpan dueTime, TimeProvider timeProvider)
//        {
//            this.dueTime = dueTime;
//            this.observer = observer;
//            this.timeProvider = timeProvider;
//            this.timer = timeProvider.CreateStoppedTimer(timerCallback, this);
//            this.Subscription = new CallbackDisposable(disposeCallback, this);
//        }

//        public void OnNext(T value)
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

//        public void OnCompleted(Result complete)
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

//            T value;
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
//                        self.observer.OnNext(value);
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
//                        self.observer.OnCompleted(self.completeMessage!);
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
