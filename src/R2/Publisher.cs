using R2.Internal;
using System.Runtime.CompilerServices;

namespace R2;

public sealed class Publisher<TMessage> : IEvent<TMessage>, ISubscriber<TMessage>, IDisposable
{
    CompactListCore<Subscription> list;

    public Publisher()
    {
        list = new CompactListCore<Subscription>(this);
    }

    public void OnNext(TMessage message)
    {
        foreach (var subscriber in list.AsSpan())
        {
            if (subscriber != null)
            {
                subscriber.OnNext(message);
            }
        }
    }

    public IDisposable Subscribe(ISubscriber<TMessage> subscriber)
    {
        var subscription = new Subscription(this, subscriber);
        list.Add(subscription);
        return subscription;
    }

    void Unsubscribe(Subscription subscription)
    {
        list.Remove(subscription);
    }

    public void Dispose()
    {
        list.Dispose();
    }

    sealed class Subscription(Publisher<TMessage>? parent, ISubscriber<TMessage> subscriber) : IDisposable
    {
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

public sealed class CompletablePublisher<TMessage, TComplete> : ICompletableEvent<TMessage, TComplete>, ISubscriber<TMessage, TComplete>, IDisposable
{
    int calledCompleted = 0;
    CompactListCore<Subscription> list;

    public CompletablePublisher()
    {
        list = new CompactListCore<Subscription>(this);
    }

    public void OnNext(TMessage message)
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

    public void OnCompleted(TComplete complete)
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

    public IDisposable Subscribe(ISubscriber<TMessage, TComplete> subscriber)
    {
        var subscription = new Subscription(this, subscriber);
        list.Add(subscription);
        return subscription;
    }

    void Unsubscribe(Subscription subscription)
    {
        list.Remove(subscription);
    }

    public void Dispose()
    {
        list.Dispose();
    }

    sealed class Subscription(CompletablePublisher<TMessage, TComplete>? parent, ISubscriber<TMessage, TComplete> subscriber) : IDisposable
    {
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