namespace R3
{
    public static partial class EventFactory
    {
        public static CompletableEvent<TMessage, Unit> Return<TMessage>(TMessage value)
        {
            return new ImmediateScheduleReturn<TMessage, Unit>(value, default); // immediate
        }

        public static CompletableEvent<TMessage, Unit> Return<TMessage>(TMessage value, TimeProvider timeProvider)
        {
            return Return(value, TimeSpan.Zero, timeProvider);
        }

        public static CompletableEvent<TMessage, Unit> Return<TMessage>(TMessage value, TimeSpan dueTime, TimeProvider timeProvider)
        {
            if (dueTime == TimeSpan.Zero)
            {
                if (timeProvider == TimeProvider.System)
                {
                    return new ThreadPoolScheduleReturn<TMessage, Unit>(value, default, null); // optimize for SystemTimeProvidr, use ThreadPool.UnsafeQueueUserWorkItem
                }
            }

            return new Return<TMessage, Unit>(value, default, dueTime, timeProvider); // use ITimer
        }

        // OnCompleted

        public static CompletableEvent<TMessage, TComplete> Return<TMessage, TComplete>(TMessage value, TComplete complete)
        {
            return new ImmediateScheduleReturn<TMessage, TComplete>(value, complete); // immediate
        }

        public static CompletableEvent<TMessage, TComplete> Return<TMessage, TComplete>(TMessage value, TComplete complete, TimeProvider timeProvider)
        {
            return Return(value, complete, TimeSpan.Zero, timeProvider);
        }

        public static CompletableEvent<TMessage, TComplete> Return<TMessage, TComplete>(TMessage value, TComplete complete, TimeSpan dueTime, TimeProvider timeProvider)
        {
            if (dueTime == TimeSpan.Zero)
            {
                if (timeProvider == TimeProvider.System)
                {
                    return new ThreadPoolScheduleReturn<TMessage, TComplete>(value, complete, null); // optimize for SystemTimeProvidr, use ThreadPool.UnsafeQueueUserWorkItem
                }
            }

            return new Return<TMessage, TComplete>(value, complete, dueTime, timeProvider); // use ITimer
        }
    }
}

namespace R3.Factories
{
    internal class Return<TMessage, TComplete>(TMessage value, TComplete complete, TimeSpan dueTime, TimeProvider timeProvider) : CompletableEvent<TMessage, TComplete>
    {
        protected override IDisposable SubscribeCore(Subscriber<TMessage, TComplete> subscriber)
        {
            var method = new _Return(value, complete, subscriber);
            method.Timer = timeProvider.CreateStoppedTimer(_Return.timerCallback, method);
            method.Timer.InvokeOnce(dueTime);
            return method;
        }

        sealed class _Return(TMessage value, TComplete complete, Subscriber<TMessage, TComplete> subscriber) : IDisposable
        {
            public static readonly TimerCallback timerCallback = NextTick;

            readonly TMessage value = value;
            readonly TComplete complete = complete;
            readonly Subscriber<TMessage, TComplete> subscriber = subscriber;

            public ITimer? Timer { get; set; }

            static void NextTick(object? state)
            {
                var self = (_Return)state!;
                try
                {
                    self.subscriber.OnNext(self.value);
                    self.subscriber.OnCompleted(self.complete);
                }
                finally
                {
                    self.Dispose();
                }
            }

            public void Dispose()
            {
                Timer?.Dispose();
                Timer = null;
            }
        }
    }

    internal class ImmediateScheduleReturn<TMessage, TComplete>(TMessage value, TComplete complete) : CompletableEvent<TMessage, TComplete>
    {
        protected override IDisposable SubscribeCore(Subscriber<TMessage, TComplete> subscriber)
        {
            subscriber.OnNext(value);
            subscriber.OnCompleted(complete);
            return Disposable.Empty;
        }
    }

    internal class ThreadPoolScheduleReturn<TMessage, TComplete>(TMessage value, TComplete complete, Action<Exception>? unhandledExceptionHandler) : CompletableEvent<TMessage, TComplete>
    {
        protected override IDisposable SubscribeCore(Subscriber<TMessage, TComplete> subscriber)
        {
            var method = new _Return(value, complete, unhandledExceptionHandler, subscriber);
            ThreadPool.UnsafeQueueUserWorkItem(method, preferLocal: false);
            return method;
        }

        sealed class _Return(TMessage value, TComplete complete, Action<Exception>? unhandledExceptionHandler, Subscriber<TMessage, TComplete> subscriber) : IDisposable, IThreadPoolWorkItem
        {
            bool stop;

            public void Execute()
            {
                if (stop) return;

                try
                {
                    subscriber.OnNext(value);
                    subscriber.OnCompleted(complete);
                }
                catch (Exception ex)
                {
                    if (unhandledExceptionHandler == null)
                    {
                        throw;
                    }
                    else
                    {
                        unhandledExceptionHandler?.Invoke(ex);
                    }
                }
            }

            public void Dispose()
            {
                stop = true;
            }
        }
    }
}
