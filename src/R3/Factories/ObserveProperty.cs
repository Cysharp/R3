using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace R3;

public static partial class Observable
{
    /// <summary>
    /// Convert INotifyPropertyChanged to Observable.
    /// `propertySelector` must be a Func specifying a simple property. For example, it extracts "Foo" from `x => x.Foo`.
    /// </summary>
    public static Observable<TProperty> ObservePropertyChanged<T, TProperty>(this T value,
        Func<T, TProperty> propertySelector,
        bool pushCurrentValueOnSubscribe = true,
        CancellationToken cancellationToken = default,
        [CallerArgumentExpression("propertySelector")] string? expr = null)
        where T : INotifyPropertyChanged
    {
        if (expr == null) throw new ArgumentNullException(expr);

        var propertyName = expr!.Substring(expr.LastIndexOf('.') + 1);
        return new ObservePropertyChanged<T, TProperty>(value, propertySelector, propertyName, pushCurrentValueOnSubscribe, cancellationToken);
    }

    /// <summary>
    /// Convert INotifyPropertyChanging to Observable.
    /// `propertySelector` must be a Func specifying a simple property. For example, it extracts "Foo" from `x => x.Foo`.
    /// </summary>
    public static Observable<TProperty> ObservePropertyChanging<T, TProperty>(this T value,
        Func<T, TProperty> propertySelector,
        bool pushCurrentValueOnSubscribe = true,
        CancellationToken cancellationToken = default,
        [CallerArgumentExpression("propertySelector")] string? expr = null)
        where T : INotifyPropertyChanging
    {
        if (expr == null) throw new ArgumentNullException(expr);

        var propertyName = expr!.Substring(expr.LastIndexOf('.') + 1);
        return new ObservePropertyChanging<T, TProperty>(value, propertySelector, propertyName, pushCurrentValueOnSubscribe, cancellationToken);
    }
}

internal sealed class ObservePropertyChanged<T, TProperty>(T value, Func<T, TProperty> propertySelector, string propertyName, bool pushCurrentValueOnSubscribe, CancellationToken cancellationToken)
    : Observable<TProperty> where T : INotifyPropertyChanged
{
    protected override IDisposable SubscribeCore(Observer<TProperty> observer)
    {
        if (pushCurrentValueOnSubscribe)
        {
            observer.OnNext(propertySelector(value));
        }

        return new _ObservePropertyChanged(observer, value, propertySelector, propertyName, cancellationToken);
    }

    sealed class _ObservePropertyChanged : IDisposable
    {
        readonly Observer<TProperty> observer;
        readonly T value;
        readonly Func<T, TProperty> propertySelector;
        readonly string propertyName;
        PropertyChangedEventHandler? eventHandler;
        CancellationTokenRegistration cancellationTokenRegistration;

        public _ObservePropertyChanged(Observer<TProperty> observer, T value, Func<T, TProperty> propertySelector, string propertyName, CancellationToken cancellationToken)
        {
            this.observer = observer;
            this.value = value;
            this.propertySelector = propertySelector;
            this.propertyName = propertyName;
            this.eventHandler = PublishOnNext;

            value.PropertyChanged += eventHandler;

            if (cancellationToken.CanBeCanceled)
            {
                this.cancellationTokenRegistration = cancellationToken.UnsafeRegister(static state =>
                {
                    var s = (_ObservePropertyChanged)state!;
                    s.CompleteDispose();
                }, this);
            }
        }

        void PublishOnNext(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == propertyName)
            {
                TProperty prop;
                try
                {
                    prop = propertySelector(value);
                }
                catch (Exception ex)
                {
                    observer.OnErrorResume(ex);
                    return;
                }

                observer.OnNext(prop);
            }
        }

        void CompleteDispose()
        {
            observer.OnCompleted();
            Dispose();
        }

        public void Dispose()
        {
            var handler = Interlocked.Exchange(ref eventHandler, null);
            if (handler != null)
            {
                cancellationTokenRegistration.Dispose();
                value.PropertyChanged -= eventHandler;
            }
        }
    }
}

internal sealed class ObservePropertyChanging<T, TProperty>(T value, Func<T, TProperty> propertySelector, string propertyName, bool pushCurrentValueOnSubscribe, CancellationToken cancellationToken)
    : Observable<TProperty> where T : INotifyPropertyChanging
{
    protected override IDisposable SubscribeCore(Observer<TProperty> observer)
    {
        if (pushCurrentValueOnSubscribe)
        {
            observer.OnNext(propertySelector(value));
        }

        return new _ObservePropertyChanged(observer, value, propertySelector, propertyName, cancellationToken);
    }

    sealed class _ObservePropertyChanged : IDisposable
    {
        readonly Observer<TProperty> observer;
        readonly T value;
        readonly Func<T, TProperty> propertySelector;
        readonly string propertyName;
        PropertyChangingEventHandler? eventHandler;
        CancellationTokenRegistration cancellationTokenRegistration;

        public _ObservePropertyChanged(Observer<TProperty> observer, T value, Func<T, TProperty> propertySelector, string propertyName, CancellationToken cancellationToken)
        {
            this.observer = observer;
            this.value = value;
            this.propertySelector = propertySelector;
            this.propertyName = propertyName;
            this.eventHandler = PublishOnNext;

            value.PropertyChanging += eventHandler;

            if (cancellationToken.CanBeCanceled)
            {
                this.cancellationTokenRegistration = cancellationToken.UnsafeRegister(static state =>
                {
                    var s = (_ObservePropertyChanged)state!;
                    s.CompleteDispose();
                }, this);
            }
        }

        void PublishOnNext(object? sender, PropertyChangingEventArgs e)
        {
            if (e.PropertyName == propertyName)
            {
                TProperty prop;
                try
                {
                    prop = propertySelector(value);
                }
                catch (Exception ex)
                {
                    observer.OnErrorResume(ex);
                    return;
                }

                observer.OnNext(prop);
            }
        }

        void CompleteDispose()
        {
            observer.OnCompleted();
            Dispose();
        }

        public void Dispose()
        {
            var handler = Interlocked.Exchange(ref eventHandler, null);
            if (handler != null)
            {
                cancellationTokenRegistration.Dispose();
                value.PropertyChanging -= eventHandler;
            }
        }
    }
}
