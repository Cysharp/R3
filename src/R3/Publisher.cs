using R2.Internal;
using System.Runtime.CompilerServices;

namespace R3;

public interface IEventPublisher<TMessage>
{
    void PublishOnNext(TMessage message);
}

public interface ICompletableEventPublisher<TMessage, TComplete>
{
    void PublishOnNext(TMessage message);
    void PublishOnCompleted(TComplete complete);
}

public sealed class Publisher<TMessage> : Event<TMessage>, IEventPublisher<TMessage>, IDisposable
{
    FreeListCore<_Publisher> list;

    public Publisher()
    {
        list = new FreeListCore<_Publisher>(this);
    }

    public void PublishOnNext(TMessage message)
    {
        foreach (var subscriber in list.AsSpan())
        {
            if (subscriber != null)
            {
                subscriber.OnNext(message);
            }
        }
    }

    protected override IDisposable SubscribeCore(Subscriber<TMessage> subscriber)
    {
        var subscription = new _Publisher(this, subscriber);
        subscription.removeKey = list.Add(subscription);
        return subscription;
    }

    void Unsubscribe(_Publisher subscription)
    {
        list.Remove(subscription.removeKey);
    }

    public void Dispose()
    {
        list.Dispose();
    }

    sealed class _Publisher(Publisher<TMessage>? parent, Subscriber<TMessage> subscriber) : IDisposable
    {
        public int removeKey;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNext(TMessage message)
        {
            subscriber.OnNext(message);
        }

        public void Dispose()
        {
            var p = Interlocked.Exchange(ref parent, null);
            if (p == null) return;

            p.Unsubscribe(this);
        }
    }
}

public sealed class CompletablePublisher<TMessage, TComplete> : CompletableEvent<TMessage, TComplete>, ICompletableEventPublisher<TMessage, TComplete>, IDisposable
{
    int calledCompleted = 0;
    FreeListCore<_CompletablePublisher> list;

    public CompletablePublisher()
    {
        list = new FreeListCore<_CompletablePublisher>(this);
    }

    public void PublishOnNext(TMessage message)
    {
        if (Volatile.Read(ref calledCompleted) != 0) return;

        foreach (var subscriber in list.AsSpan())
        {
            if (subscriber != null)
            {
                subscriber.OnNext(message);
            }
        }
    }

    public void PublishOnCompleted(TComplete complete)
    {
        var locationValue = Interlocked.CompareExchange(ref calledCompleted, 1, 0);
        if (locationValue != 0) return;

        foreach (var subscriber in list.AsSpan())
        {
            if (subscriber != null)
            {
                subscriber.OnCompleted(complete);
            }
        }
    }

    protected override IDisposable SubscribeCore(Subscriber<TMessage, TComplete> subscriber)
    {
        var subscription = new _CompletablePublisher(this, subscriber);
        subscription.removeKey = list.Add(subscription);
        return subscription;
    }

    void Unsubscribe(_CompletablePublisher subscription)
    {
        list.Remove(subscription.removeKey);
    }

    public void Dispose()
    {
        list.Dispose();
    }

    sealed class _CompletablePublisher(CompletablePublisher<TMessage, TComplete>? parent, Subscriber<TMessage, TComplete> subscriber) : IDisposable
    {
        public int removeKey;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNext(TMessage message)
        {
            subscriber.OnNext(message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(TComplete complete)
        {
            subscriber.OnCompleted(complete);
        }

        public void Dispose()
        {
            var p = Interlocked.Exchange(ref parent, null);
            if (p == null) return;

            p.Unsubscribe(this);
        }
    }
}
