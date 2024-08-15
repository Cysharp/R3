using System.Collections;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

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

    public static IReadOnlyBindableReactiveProperty<T> ToReadOnlyBindableReactiveProperty<T>(this Observable<T> source, T initialValue = default!)
    {
        return new ReadOnlyBindableReactiveProperty<T>(new BindableReactiveProperty<T>(source, initialValue, EqualityComparer<T>.Default));
    }

    public static IReadOnlyBindableReactiveProperty<T> ToReadOnlyBindableReactiveProperty<T>(this Observable<T> source, IEqualityComparer<T>? equalityComparer, T initialValue = default!)
    {
        return new ReadOnlyBindableReactiveProperty<T>(new BindableReactiveProperty<T>(source, initialValue, equalityComparer));
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

internal sealed class ReadOnlyBindableReactiveProperty<T>(BindableReactiveProperty<T> property) : IReadOnlyBindableReactiveProperty<T>
{
    public T Value => ((IReadOnlyBindableReactiveProperty<T>)property).Value;

    public bool HasErrors => ((INotifyDataErrorInfo)property).HasErrors;

    object? IReadOnlyBindableReactiveProperty.Value => ((IReadOnlyBindableReactiveProperty)property).Value;

    PropertyChangedEventHandler? propertyChangedCore;

    public event PropertyChangedEventHandler? PropertyChanged
    {
        add
        {
            propertyChangedCore += value;
            ((INotifyPropertyChanged)property).PropertyChanged += PropertyChangedEventHandler;
        }

        remove
        {
            propertyChangedCore -= value;
            ((INotifyPropertyChanged)property).PropertyChanged -= PropertyChangedEventHandler;
        }
    }

    EventHandler<DataErrorsChangedEventArgs>? errorsChangedCore;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged
    {
        add
        {
            errorsChangedCore += value;
            ((INotifyDataErrorInfo)property).ErrorsChanged += ErrorsChangedEventHandler;
        }

        remove
        {
            errorsChangedCore -= value;
            ((INotifyDataErrorInfo)property).ErrorsChanged -= ErrorsChangedEventHandler;
        }
    }

    void PropertyChangedEventHandler(object? sender, PropertyChangedEventArgs e)
    {
        // parent sender is BindableReactiveProperty<T>, change to self.
        propertyChangedCore?.Invoke(this, e);
    }

    void ErrorsChangedEventHandler(object? sender, DataErrorsChangedEventArgs e)
    {
        errorsChangedCore?.Invoke(this, e);
    }

    public Observable<T> AsObservable()
    {
        return ((IReadOnlyBindableReactiveProperty<T>)property).AsObservable();
    }

    public void Dispose()
    {
        ((IDisposable)property).Dispose();
    }

    public IReadOnlyBindableReactiveProperty<T> EnableValidation()
    {
        return ((IReadOnlyBindableReactiveProperty<T>)property).EnableValidation();
    }

    public IReadOnlyBindableReactiveProperty<T> EnableValidation(Func<T, Exception?> validator)
    {
        return ((IReadOnlyBindableReactiveProperty<T>)property).EnableValidation(validator);
    }

    public IReadOnlyBindableReactiveProperty<T> EnableValidation<TClass>([CallerMemberName] string? propertyName = null)
    {
        return ((IReadOnlyBindableReactiveProperty<T>)property).EnableValidation<TClass>(propertyName);
    }

    public IReadOnlyBindableReactiveProperty<T> EnableValidation(Expression<Func<IReadOnlyBindableReactiveProperty<T>?>> selfSelector)
    {
        return ((IReadOnlyBindableReactiveProperty<T>)property).EnableValidation(selfSelector);
    }

    public IEnumerable GetErrors(string? propertyName)
    {
        return ((INotifyDataErrorInfo)property).GetErrors(propertyName);
    }

    public override string? ToString()
    {
        return property.ToString();
    }
}
