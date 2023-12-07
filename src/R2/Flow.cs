using R2.Internal;
using System.Runtime.CompilerServices;

namespace R2;

public interface IReadOnlyFlow<T> : IEvent<T>
{
    T Value { get; }
}

public interface IFlow<T> : IReadOnlyFlow<T>
{
    new T Value { get; set; }
}

public class Flow<T> : IFlow<T>, IDisposable
{
    T value;
    IEqualityComparer<T>? equalityComparer;
    CompactListCore<Subscription> list;

    public IEqualityComparer<T>? EqualityComparer => equalityComparer;

    public T Value
    {
        get => this.value;
        set
        {
            if (EqualityComparer != null)
            {
                if (EqualityComparer.Equals(this.value, value))
                {
                    return;
                }
            }

            this.value = value;
            foreach (var subscriber in list.AsSpan())
            {
                subscriber?.OnNext(value);
            }
        }
    }

    public Flow(T value)
        : this(value, EqualityComparer<T>.Default)
    {
    }

    public Flow(T value, EqualityComparer<T>? equalityComparer)
    {
        this.value = value;
        this.equalityComparer = equalityComparer;
        this.list = new CompactListCore<Subscription>(this);
    }

    public IDisposable Subscribe(ISubscriber<T> subscriber)
    {
        var value = this.value;
        ObjectDisposedException.ThrowIf(list.IsDisposed, this);

        // raise latest value on subscribe
        subscriber.OnNext(value);

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

    public override string? ToString()
    {
        return (value == null) ? "(null)" : value.ToString();
    }

    sealed class Subscription(Flow<T>? parent, ISubscriber<T> subscriber) : IDisposable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNext(T message)
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
