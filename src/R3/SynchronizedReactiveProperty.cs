namespace R3;

public class SynchronizedReactiveProperty<T> : ReactiveProperty<T>
{
    public SynchronizedReactiveProperty()
        : base()
    {
    }

    public SynchronizedReactiveProperty(T value)
        : base(value)
    {
    }

    public SynchronizedReactiveProperty(T value, IEqualityComparer<T>? equalityComparer)
        : base(value, equalityComparer)
    {
    }

    public override T Value
    {
        get
        {
            lock (this)
            {
                return base.Value;
            }
        }
        set
        {
            lock (this)
            {
                base.Value = value;
            }
        }
    }

    public override void OnNext(T value)
    {
        lock (this)
        {
            base.OnNext(value);
        }
    }

    public override void ForceNotify()
    {
        lock (this)
        {
            base.ForceNotify();
        }
    }

    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        lock (this)
        {
            return base.SubscribeCore(observer);
        }
    }
}
