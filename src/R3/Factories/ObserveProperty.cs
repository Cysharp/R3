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
        [CallerArgumentExpression(nameof(propertySelector))] string? expr = null)
        where T : INotifyPropertyChanged
    {
        if (expr == null) throw new ArgumentNullException(expr);

        var propertyName = expr!.Substring(expr.LastIndexOf('.') + 1);
        return new ObservePropertyChanged<T, TProperty>(value, propertySelector, propertyName, pushCurrentValueOnSubscribe, cancellationToken);
    }

    /// <summary>
    /// Convert INotifyPropertyChanged to Observable.
    /// `propertySelector1` and `propertySelector2` must be a Func specifying a simple property. For example, it extracts "Foo" from `x => x.Foo`.
    /// </summary>
    public static Observable<TProperty2> ObservePropertyChanged<T, TProperty1, TProperty2>(this T value,
        Func<T, TProperty1?> propertySelector1,
        Func<TProperty1, TProperty2> propertySelector2,
        bool pushCurrentValueOnSubscribe = true,
        CancellationToken cancellationToken = default,
        [CallerArgumentExpression(nameof(propertySelector1))] string? propertySelector1Expr = null,
        [CallerArgumentExpression(nameof(propertySelector2))] string? propertySelector2Expr = null)
        where T : INotifyPropertyChanged
        where TProperty1 : INotifyPropertyChanged
    {
        if (propertySelector1Expr == null) throw new ArgumentNullException(propertySelector1Expr);
        if (propertySelector2Expr == null) throw new ArgumentNullException(propertySelector2Expr);

        var property1Name = propertySelector1Expr!.Substring(propertySelector1Expr.LastIndexOf('.') + 1);
        var property2Name = propertySelector2Expr!.Substring(propertySelector2Expr.LastIndexOf('.') + 1);

        return new ObservePropertyChanged<T, TProperty1?>(value, propertySelector1, property1Name, true, cancellationToken)
            .Select(
                (propertySelector2, property2Name, pushCurrentValueOnSubscribe, cancellationToken),
                (firstPropertyValue, state) =>
                    firstPropertyValue is not null
                        ? new ObservePropertyChanged<TProperty1, TProperty2>(firstPropertyValue,
                            state.propertySelector2, state.property2Name, state.pushCurrentValueOnSubscribe,
                            state.cancellationToken)
                        : Empty<TProperty2>())
            .Switch();
    }

    /// <summary>
    /// Convert INotifyPropertyChanged to Observable.
    /// `propertySelector1`, `propertySelector2`, and `propertySelector3` must be a Func specifying a simple property. For example, it extracts "Foo" from `x => x.Foo`.
    /// </summary>
    public static Observable<TProperty3> ObservePropertyChanged<T, TProperty1, TProperty2, TProperty3>(this T value,
        Func<T, TProperty1?> propertySelector1,
        Func<TProperty1, TProperty2?> propertySelector2,
        Func<TProperty2, TProperty3> propertySelector3,
        bool pushCurrentValueOnSubscribe = true,
        CancellationToken cancellationToken = default,
        [CallerArgumentExpression(nameof(propertySelector1))] string? propertySelector1Expr = null,
        [CallerArgumentExpression(nameof(propertySelector2))] string? propertySelector2Expr = null,
        [CallerArgumentExpression(nameof(propertySelector3))] string? propertySelector3Expr = null)
        where T : INotifyPropertyChanged
        where TProperty1 : INotifyPropertyChanged
        where TProperty2 : INotifyPropertyChanged
    {
        if (propertySelector1Expr == null) throw new ArgumentNullException(propertySelector1Expr);
        if (propertySelector2Expr == null) throw new ArgumentNullException(propertySelector2Expr);
        if (propertySelector3Expr == null) throw new ArgumentNullException(propertySelector3Expr);

        var property1Name = propertySelector1Expr!.Substring(propertySelector1Expr.LastIndexOf('.') + 1);
        var property2Name = propertySelector2Expr!.Substring(propertySelector2Expr.LastIndexOf('.') + 1);
        var property3Name = propertySelector3Expr!.Substring(propertySelector3Expr.LastIndexOf('.') + 1);

        return new ObservePropertyChanged<T, TProperty1?>(value, propertySelector1, property1Name, true, cancellationToken)
            .Select(
                (propertySelector2, property2Name, propertySelector3, property3Name, pushCurrentValueOnSubscribe, cancellationToken),
                (firstPropertyValue, state) =>
                    firstPropertyValue is not null
                        ? new ObservePropertyChanged<TProperty1, TProperty2?>(firstPropertyValue, state.propertySelector2, state.property2Name, true, state.cancellationToken)
                            .Select(
                                (state.propertySelector3, state.property3Name, pushCurrentValueOnSubscribe, cancellationToken),
                                (secondPropertyValue, state2) =>
                                    secondPropertyValue is not null
                                        ? new ObservePropertyChanged<TProperty2, TProperty3>(secondPropertyValue,
                                            state2.propertySelector3, state2.property3Name, state2.pushCurrentValueOnSubscribe,
                                            state2.cancellationToken)
                                        : Empty<TProperty3>())
                            .Switch()
                        : Empty<TProperty3>())
            .Switch();
    }

    /// <summary>
    /// Convert INotifyPropertyChanging to Observable.
    /// `propertySelector` must be a Func specifying a simple property. For example, it extracts "Foo" from `x => x.Foo`.
    /// </summary>
    public static Observable<TProperty> ObservePropertyChanging<T, TProperty>(this T value,
        Func<T, TProperty> propertySelector,
        bool pushCurrentValueOnSubscribe = true,
        CancellationToken cancellationToken = default,
        [CallerArgumentExpression(nameof(propertySelector))] string? expr = null)
        where T : INotifyPropertyChanging
    {
        if (expr == null) throw new ArgumentNullException(expr);

        var propertyName = expr!.Substring(expr.LastIndexOf('.') + 1);
        return new ObservePropertyChanging<T, TProperty>(value, propertySelector, propertyName,
            pushCurrentValueOnSubscribe, cancellationToken);
    }

    /// <summary>
    /// Convert INotifyPropertyChanging to Observable.
    /// `propertySelector1` and `propertySelector2` must be a Func specifying a simple property. For example, it extracts "Foo" from `x => x.Foo`.
    /// </summary>
    public static Observable<TProperty2> ObservePropertyChanging<T, TProperty1, TProperty2>(this T value,
        Func<T, TProperty1?> propertySelector1,
        Func<TProperty1, TProperty2> propertySelector2,
        bool pushCurrentValueOnSubscribe = true,
        CancellationToken cancellationToken = default,
        [CallerArgumentExpression(nameof(propertySelector1))] string? propertySelector1Expr = null,
        [CallerArgumentExpression(nameof(propertySelector2))] string? propertySelector2Expr = null)
        where T : INotifyPropertyChanged
        where TProperty1 : INotifyPropertyChanging
    {
        if (propertySelector2Expr == null) throw new ArgumentNullException(propertySelector2Expr);

        var property2Name = propertySelector2Expr!.Substring(propertySelector2Expr.LastIndexOf('.') + 1);

        return ObservePropertyChanged(value, propertySelector1, true, cancellationToken, propertySelector1Expr)
            .Select(
                (propertySelector2, property2Name, pushCurrentValueOnSubscribe, cancellationToken),
                (firstPropertyValue, state) =>
                    firstPropertyValue is not null
                        ? new ObservePropertyChanging<TProperty1, TProperty2>(firstPropertyValue,
                            state.propertySelector2, state.property2Name, state.pushCurrentValueOnSubscribe,
                            state.cancellationToken)
                        : Empty<TProperty2>())
            .Switch();
    }

    /// <summary>
    /// Convert INotifyPropertyChanging to Observable.
    /// `propertySelector1`, `propertySelector2`, and `propertySelector3` must be a Func specifying a simple property. For example, it extracts "Foo" from `x => x.Foo`.
    /// </summary>
    public static Observable<TProperty3> ObservePropertyChanging<T, TProperty1, TProperty2, TProperty3>(this T value,
        Func<T, TProperty1?> propertySelector1,
        Func<TProperty1, TProperty2?> propertySelector2,
        Func<TProperty2, TProperty3> propertySelector3,
        bool pushCurrentValueOnSubscribe = true,
        CancellationToken cancellationToken = default,
        [CallerArgumentExpression(nameof(propertySelector1))] string? propertySelector1Expr = null,
        [CallerArgumentExpression(nameof(propertySelector2))] string? propertySelector2Expr = null,
        [CallerArgumentExpression(nameof(propertySelector3))] string? propertySelector3Expr = null)
        where T : INotifyPropertyChanged
        where TProperty1 : INotifyPropertyChanged
        where TProperty2 : INotifyPropertyChanging
    {
        if (propertySelector3Expr == null) throw new ArgumentNullException(propertySelector3Expr);

        var property3Name = propertySelector3Expr!.Substring(propertySelector3Expr.LastIndexOf('.') + 1);

        return ObservePropertyChanged(value, propertySelector1, propertySelector2, true, cancellationToken, propertySelector1Expr, propertySelector2Expr)
            .Select(
                (propertySelector3, property3Name, pushCurrentValueOnSubscribe, cancellationToken),
                (secondPropertyValue, state) =>
                    secondPropertyValue is not null
                        ? new ObservePropertyChanging<TProperty2, TProperty3>(secondPropertyValue,
                            state.propertySelector3, state.property3Name, state.pushCurrentValueOnSubscribe,
                            state.cancellationToken)
                        : Empty<TProperty3>())
            .Switch();
    }
}

internal sealed class ObservePropertyChanged<T, TProperty>(
    T value,
    Func<T, TProperty> propertySelector,
    string propertyName,
    bool pushCurrentValueOnSubscribe,
    CancellationToken cancellationToken)
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

internal sealed class ObservePropertyChanging<T, TProperty>(
    T value,
    Func<T, TProperty> propertySelector,
    string propertyName,
    bool pushCurrentValueOnSubscribe,
    CancellationToken cancellationToken)
    : Observable<TProperty> where T : INotifyPropertyChanging
{
    protected override IDisposable SubscribeCore(Observer<TProperty> observer)
    {
        if (pushCurrentValueOnSubscribe)
        {
            observer.OnNext(propertySelector(value));
        }

        return new _ObservePropertyChanging(observer, value, propertySelector, propertyName, cancellationToken);
    }

    sealed class _ObservePropertyChanging : IDisposable
    {
        readonly Observer<TProperty> observer;
        readonly T value;
        readonly Func<T, TProperty> propertySelector;
        readonly string propertyName;
        PropertyChangingEventHandler? eventHandler;
        CancellationTokenRegistration cancellationTokenRegistration;

        public _ObservePropertyChanging(Observer<TProperty> observer, T value, Func<T, TProperty> propertySelector, string propertyName, CancellationToken cancellationToken)
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
                    var s = (_ObservePropertyChanging)state!;
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
