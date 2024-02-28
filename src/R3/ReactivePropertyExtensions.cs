namespace R3;

public static class ReactivePropertyExtensions
{
    public static ReadOnlyReactiveProperty<T> ToReadOnlyReactiveProperty<T>(this Observable<T> source, T initialValue = default!)
    {
        return source.ToReadOnlyReactiveProperty(EqualityComparer<T>.Default, initialValue);
    }

    public static ReadOnlyReactiveProperty<T> ToReadOnlyReactiveProperty<T>(this Observable<T> source, IEqualityComparer<T>? equalityComparer, T initialValue = default!)
    {
        if (source is ReadOnlyReactiveProperty<T> rrp)
        {
            return rrp;
        }

        // allow to cast ReactiveProperty<T>
        return new ConnectedReactiveProperty<T>(source, initialValue, equalityComparer);
    }

    // ToBindable

    public static BindableReactiveProperty<T> ToBindableReactiveProperty<T>(this Observable<T> source, T initialValue = default!)
    {
        return new BindableReactiveProperty<T>(source, initialValue, EqualityComparer<T>.Default);
    }

    public static BindableReactiveProperty<T> ToBindableReactiveProperty<T>(this Observable<T> source, IEqualityComparer<T>? equalityComparer, T initialValue = default!)
    {
        return new BindableReactiveProperty<T>(source, initialValue, equalityComparer);
    }

    public static ReadOnlyBindableReactiveProperty<T> ToReadOnlyBindableReactiveProperty<T>(this Observable<T> source, T initialValue = default!)
    {
        if (source is ReadOnlyBindableReactiveProperty<T> rrp)
        {
            return rrp;
        }
        return source.ToReadOnlyBindableReactiveProperty(EqualityComparer<T>.Default, initialValue);
    }

    public static ReadOnlyBindableReactiveProperty<T> ToReadOnlyBindableReactiveProperty<T>(this Observable<T> source, IEqualityComparer<T>? equalityComparer, T initialValue = default!)
    {
        if (source is ReadOnlyBindableReactiveProperty<T> rrp)
        {
            return rrp;
        }
        // allow to cast ReactiveProperty<T>
        return new ConnectedBindableReactiveProperty<T>(source, initialValue, equalityComparer);
    }
}

internal sealed class ConnectedReactiveProperty<T> : ReactiveProperty<T>
{
    readonly IDisposable sourceSubscription;

    public ConnectedReactiveProperty(Observable<T> source, T initialValue, IEqualityComparer<T>? equalityComparer)
        : base(initialValue, equalityComparer)
    {
        this.sourceSubscription = source.Subscribe(new Observer(this));
    }

    protected override void DisposeCore()
    {
        sourceSubscription.Dispose();
    }

    class Observer(ConnectedReactiveProperty<T> parent) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            parent.Value = value;
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

internal sealed class ConnectedBindableReactiveProperty<T> : BindableReactiveProperty<T>
{
    readonly IDisposable sourceSubscription;

    public ConnectedBindableReactiveProperty(Observable<T> source, T initialValue, IEqualityComparer<T>? equalityComparer)
        : base(initialValue, equalityComparer)
    {
        this.sourceSubscription = source.Subscribe(new Observer(this));
    }

    protected override void DisposeCore()
    {
        sourceSubscription.Dispose();
    }

    class Observer(ConnectedBindableReactiveProperty<T> parent) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            parent.Value = value;
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

