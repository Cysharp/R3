using R3.Internal;
using System.Runtime.CompilerServices;

namespace R3;

public abstract class ReadOnlyReactiveProperty<T> : Observable<T>
{
    public abstract T CurrentValue { get; }
}

// allow inherit
public class ReactiveProperty<T> : ReadOnlyReactiveProperty<T>, IDisposable
{
    T value;
    IEqualityComparer<T>? equalityComparer;
    FreeListCore<Subscription> list;

    public IEqualityComparer<T>? EqualityComparer => equalityComparer;

    public override T CurrentValue => value;

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
            foreach (var observer in list.AsSpan())
            {
                observer?.OnNext(value);
            }
        }
    }

    public ReactiveProperty(T value)
        : this(value, EqualityComparer<T>.Default)
    {
    }

    public ReactiveProperty(T value, EqualityComparer<T>? equalityComparer)
    {
        this.value = value;
        this.equalityComparer = equalityComparer;
        this.list = new FreeListCore<Subscription>(this);
    }

    public void PublishOnErrorResume(Exception error)
    {
        foreach (var observer in list.AsSpan())
        {
            observer?.OnErrorResume(error);
        }
    }

    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var value = this.value;
        ObjectDisposedException.ThrowIf(list.IsDisposed, this);

        // raise latest value on subscribe
        observer.OnNext(value);

        var subscription = new Subscription(this, observer);
        subscription.removeKey = list.Add(subscription);
        return subscription;
    }

    void Unsubscribe(Subscription subscription)
    {
        list.Remove(subscription.removeKey);
    }

    public void Dispose()
    {
        // TODO: call OnCompleted on Dispose.
        list.Dispose();
    }

    public override string? ToString()
    {
        return (value == null) ? "(null)" : value.ToString();
    }

    sealed class Subscription(ReactiveProperty<T>? parent, Observer<T> observer) : IDisposable
    {
        public int removeKey;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNext(T value)
        {
            observer.OnNext(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnErrorResume(Exception error)
        {
            observer.OnErrorResume(error);
        }

        public void Dispose()
        {
            var p = Interlocked.Exchange(ref parent, null);
            if (p == null) return;

            p.Unsubscribe(this);
        }
    }
}
