namespace R3;

public static partial class EventExtensions
{
    public static Event<T> TakeUntil<T, TOther>(this Event<T> source, Event<TOther> other)
    {
        return new TakeUntil<T, TOther>(source, other);
    }

    public static Event<T> TakeUntil<T>(this Event<T> source, CancellationToken cancellationToken)
    {
        if (!cancellationToken.CanBeCanceled)
        {
            return source;
        }
        if (cancellationToken.IsCancellationRequested)
        {
            return Event.Empty<T>();
        }

        return new TakeUntilC<T>(source, cancellationToken);
    }

    public static Event<T> TakeUntil<T>(this Event<T> source, Task task)
    {
        return new TakeUntilT<T>(source, task);
    }
}

internal sealed class TakeUntil<T, TOther>(Event<T> source, Event<TOther> other) : Event<T>
{
    protected override IDisposable SubscribeCore(Subscriber<T> subscriber)
    {
        var takeUntil = new _TakeUntil(subscriber);
        var stopperSubscription = other.Subscribe(takeUntil.stopper);
        try
        {
            return source.Subscribe(takeUntil); // subscription contains self and stopper.
        }
        catch
        {
            stopperSubscription.Dispose();
            throw;
        }
    }

    sealed class _TakeUntil : Subscriber<T>
    {
        readonly Subscriber<T> subscriber;
        internal readonly TakeUntilStopperSubscriber stopper; // this instance is not exposed to public so can use lock.

        public _TakeUntil(Subscriber<T> subscriber)
        {
            this.subscriber = subscriber;
            this.stopper = new TakeUntilStopperSubscriber(this);
        }

        protected override void OnNextCore(T value)
        {
            subscriber.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            subscriber.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            subscriber.OnCompleted(result);
        }

        protected override void DisposeCore()
        {
            stopper.Dispose();
        }
    }

    sealed class TakeUntilStopperSubscriber(_TakeUntil parent) : Subscriber<TOther>
    {
        protected override void OnNextCore(TOther value)
        {
            parent.OnCompleted(Result.Success);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            parent.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            parent.OnCompleted(result);
        }
    }
}

internal sealed class TakeUntilC<T>(Event<T> source, CancellationToken cancellationToken) : Event<T>
{
    protected override IDisposable SubscribeCore(Subscriber<T> subscriber)
    {
        return source.Subscribe(new _TakeUntil(subscriber, cancellationToken));
    }

    sealed class _TakeUntil : Subscriber<T>, IDisposable
    {
        static readonly Action<object?> cancellationCallback = CancellationCallback;

        readonly Subscriber<T> subscriber;
        CancellationTokenRegistration tokenRegistration;

        public _TakeUntil(Subscriber<T> subscriber, CancellationToken cancellationToken)
        {
            this.subscriber = subscriber;
            this.tokenRegistration = cancellationToken.Register(cancellationCallback, this);
        }

        protected override void OnNextCore(T value)
        {
            subscriber.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            subscriber.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            subscriber.OnCompleted(result);
        }

        static void CancellationCallback(object? state)
        {
            var self = (_TakeUntil)state!;
            self.OnCompleted();
        }

        protected override void DisposeCore()
        {
            tokenRegistration.Dispose();
        }
    }
}

internal sealed class TakeUntilT<T>(Event<T> source, Task task) : Event<T>
{
    protected override IDisposable SubscribeCore(Subscriber<T> subscriber)
    {
        return source.Subscribe(new _TakeUntil(subscriber, task));
    }

    sealed class _TakeUntil : Subscriber<T>, IDisposable
    {
        readonly Subscriber<T> subscriber;

        public _TakeUntil(Subscriber<T> subscriber, Task task)
        {
            this.subscriber = subscriber;
            TaskAwait(task);
        }

        protected override void OnNextCore(T value)
        {
            subscriber.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            subscriber.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            subscriber.OnCompleted(result);
        }

        async void TaskAwait(Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
                OnCompleted(Result.Success);
            }
            catch (Exception ex)
            {
                OnCompleted(Result.Failure(ex));
            }
        }
    }
}
