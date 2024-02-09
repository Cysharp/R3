namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> Delay<T>(this Observable<T> source, TimeSpan dueTime)
    {
        return new Delay<T>(source, dueTime, ObservableSystem.DefaultTimeProvider);
    }

    public static Observable<T> Delay<T>(this Observable<T> source, TimeSpan dueTime, TimeProvider timeProvider)
    {
        return new Delay<T>(source, dueTime, timeProvider);
    }
}

internal sealed class Delay<T>(Observable<T> source, TimeSpan dueTime, TimeProvider timeProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _Delay(observer, dueTime.Normalize(), timeProvider));
    }

    sealed class _Delay : Observer<T>
    {
        static readonly TimerCallback timerCallback = DrainMessages;

        readonly Observer<T> observer;
        readonly TimeSpan dueTime;
        readonly TimeProvider timeProvider;
        readonly Queue<(long timestamp, Notification<T> value)> queue = new(); // lock gate
        ITimer timer;
        bool running;

        protected override bool AutoDisposeOnCompleted => false;

        public _Delay(Observer<T> observer, TimeSpan dueTime, TimeProvider timeProvider)
        {
            this.dueTime = dueTime;
            this.observer = observer;
            this.timeProvider = timeProvider;
            this.timer = timeProvider.CreateStoppedTimer(timerCallback, this);
        }

        protected override void OnNextCore(T value)
        {
            lock (queue)
            {
                queue.Enqueue((timeProvider.GetTimestamp(), new(value)));
                if (queue.Count == 1 && !running)
                {
                    running = true;
                    timer.RestartImmediately();
                }
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            lock (queue)
            {
                queue.Enqueue((timeProvider.GetTimestamp(), new(error)));
                if (queue.Count == 1 && !running)
                {
                    running = true;
                    timer.RestartImmediately();
                }
            }
        }

        protected override void OnCompletedCore(Result result)
        {
            lock (queue)
            {
                queue.Enqueue((timeProvider.GetTimestamp(), new(result)));
                if (queue.Count == 1 && !running)
                {
                    running = true;
                    timer.RestartImmediately();
                }
            }
        }

        protected override void DisposeCore()
        {
            lock (queue)
            {
                timer.Dispose(); // stop timer.
                queue.Clear();
            }
        }

        static void DrainMessages(object? state)
        {
            var self = (_Delay)state!;
            var queue = self.queue;

            Notification<T> value;
            while (true)
            {
                if (self.IsDisposed) return;

                lock (queue)
                {
                    if (!queue.TryPeek(out var msg))
                    {
                        self.running = false;
                        return;
                    }

                    // check timestamp
                    var elapsed = self.timeProvider.GetElapsedTime(msg.timestamp);
                    if (elapsed >= self.dueTime)
                    {
                        value = queue.Dequeue().value;
                    }
                    else
                    {
                        // invoke timer again
                        self.timer.InvokeOnce(self.dueTime - elapsed);
                        return;
                    }
                }

                try
                {
                    switch (value.Kind)
                    {
                        case NotificationKind.OnNext:
                            self.observer.OnNext(value.Value!);
                            break;
                        case NotificationKind.OnErrorResume:
                            self.observer.OnErrorResume(value.Error!);
                            break;
                        case NotificationKind.OnCompleted:
                            try
                            {
                                self.observer.OnCompleted(value.Result);
                            }
                            finally
                            {
                                self.Dispose();
                            }
                            break;
                        default:
                            break;
                    }

                    continue; // loop to drain all messages
                }
                catch (Exception ex)
                {
                    ObservableSystem.GetUnhandledExceptionHandler().Invoke(ex);
                    continue;
                }
            }
        }
    }
}
