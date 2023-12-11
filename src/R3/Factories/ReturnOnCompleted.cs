namespace R3
{
    public static partial class EventFactory
    {
        // similar as Empty, only return OnCompleted

        public static CompletableEvent<TMessage, TComplete> ReturnOnCompleted<TMessage, TComplete>(TComplete complete)
        {
            return new ImmediateScheduleReturnOnCompleted<TMessage, TComplete>(complete); // immediate
        }

        public static CompletableEvent<TMessage, TComplete> ReturnOnCompleted<TMessage, TComplete>(TComplete complete, TimeProvider timeProvider)
        {
            return ReturnOnCompleted<TMessage, TComplete>(complete, TimeSpan.Zero, timeProvider);
        }

        public static CompletableEvent<TMessage, TComplete> ReturnOnCompleted<TMessage, TComplete>(TComplete complete, TimeSpan dueTime, TimeProvider timeProvider)
        {
            if (dueTime == TimeSpan.Zero)
            {
                if (timeProvider == TimeProvider.System)
                {
                    return new ThreadPoolScheduleReturnOnCompleted<TMessage, TComplete>(complete, null); // optimize for SystemTimeProvidr, use ThreadPool.UnsafeQueueUserWorkItem
                }
                else if (timeProvider is SafeTimerTimeProvider t && t.IsSystemTimeProvider)
                {
                    return new ThreadPoolScheduleReturnOnCompleted<TMessage, TComplete>(complete, t.UnhandledExceptionHandler); // use with SafeTimeProvider.UnhandledExceptionHandler
                }
            }

            return new ReturnOnCompleted<TMessage, TComplete>(complete, dueTime, timeProvider); // use ITimer
        }
    }
}

namespace R3.Factories
{
    internal class ReturnOnCompleted<TMessage, TComplete>(TComplete complete, TimeSpan dueTime, TimeProvider timeProvider) : CompletableEvent<TMessage, TComplete>
    {
        protected override IDisposable SubscribeCore(Subscriber<TMessage, TComplete> subscriber)
        {
            var method = new _ReturnOnCompleted(complete, subscriber);
            method.Timer = timeProvider.CreateStoppedTimer(_ReturnOnCompleted.timerCallback, method);
            method.Timer.InvokeOnce(dueTime);
            return method;
        }

        sealed class _ReturnOnCompleted(TComplete complete, Subscriber<TMessage, TComplete> subscriber) : IDisposable
        {
            public static readonly TimerCallback timerCallback = NextTick;

            readonly TComplete complete = complete;
            readonly Subscriber<TMessage, TComplete> subscriber = subscriber;

            public ITimer? Timer { get; set; }

            static void NextTick(object? state)
            {
                var self = (_ReturnOnCompleted)state!;
                try
                {
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

    internal class ImmediateScheduleReturnOnCompleted<TMessage, TComplete>(TComplete complete) : CompletableEvent<TMessage, TComplete>
    {
        protected override IDisposable SubscribeCore(Subscriber<TMessage, TComplete> subscriber)
        {
            subscriber.OnCompleted(complete);
            return Disposable.Empty;
        }
    }

    internal class ThreadPoolScheduleReturnOnCompleted<TMessage, TComplete>(TComplete complete, Action<Exception>? unhandledExceptionHandler) : CompletableEvent<TMessage, TComplete>
    {
        protected override IDisposable SubscribeCore(Subscriber<TMessage, TComplete> subscriber)
        {
            var method = new _ReturnOnCompleted(complete, unhandledExceptionHandler, subscriber);
            ThreadPool.UnsafeQueueUserWorkItem(method, preferLocal: false);
            return method;
        }

        sealed class _ReturnOnCompleted(TComplete complete, Action<Exception>? unhandledExceptionHandler, Subscriber<TMessage, TComplete> subscriber) : IDisposable, IThreadPoolWorkItem
        {
            bool stop;

            public void Execute()
            {
                if (stop) return;

                try
                {
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
