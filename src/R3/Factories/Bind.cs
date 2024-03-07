using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace R3;

public static partial class Observable
{
    public static IDisposable TwoWayBind<TIn, TProperty, TOut>(
        this TIn valueIn, Func<TIn, TProperty> valueInPropertyGet, Action<TIn, TProperty> valueInPropertySet,
        TOut valueOut, Func<TOut, TProperty> valueOutPropertyGet, Action<TOut, TProperty> valueOutPropertySet,
        CancellationToken cancellationToken = default,
        [CallerArgumentExpression(nameof(valueInPropertyGet))] string? exprIn = null,
        [CallerArgumentExpression(nameof(valueOutPropertyGet))] string? exprOut = null)
        where TIn : INotifyPropertyChanged
        where TOut : INotifyPropertyChanged
    {
        return Disposable.Combine(
            valueIn
                .ObservePropertyChanged(valueInPropertyGet, true, cancellationToken, exprIn)
                .Subscribe(
                    (valueOut, valueOutPropertySet),
                    (x, state) =>
                    {
                        state.valueOutPropertySet(state.valueOut, x);
                    }),
            valueOut
                .ObservePropertyChanged(valueOutPropertyGet, false, cancellationToken, exprOut)
                .Subscribe(
                    (valueIn, valueInPropertySet),
                    (x, state) =>
                    {
                        state.valueInPropertySet(state.valueIn, x);
                    }));
    }

    /** Could potentially come up ways to mix and match INPC with EVC
    public static IDisposable TwoWayBind<TIn, TProperty, TOut>(
        this TIn valueIn, Func<TIn, TProperty> valueInPropertyGet, Func<TProperty, TIn> valueInPropertySet,
        TOut valueOut, Func<TOut, TProperty> valueOutPropertyGet, Func<TProperty, TOut> valueOutPropertySet,
        CancellationToken cancellationToken = default,
        [CallerArgumentExpression(nameof(valueInPropertyGet))] string? exprIn = null)
        where TIn : INotifyPropertyChanged
        where TOut : class
    {
        return Disposable.Combine(
            valueIn
                .ObservePropertyChanged(valueInPropertyGet, true, cancellationToken, exprIn)
                .Subscribe(valueOutPropertySet, (x, state) => state(x)),
            EveryValueChanged(valueOut, valueOutPropertyGet, cancellationToken)
                .Skip(1)
                .Subscribe(valueInPropertySet, (x, state) => state(x)));
    }

    public static IDisposable TwoWayBind<TIn, TProperty, TOut>(
        this TIn valueIn, Func<TIn, TProperty> valueInPropertyGet, Func<TProperty, TIn> valueInPropertySet,
        TOut valueOut, Func<TOut, TProperty> valueOutPropertyGet, Func<TProperty, TOut> valueOutPropertySet,
        CancellationToken cancellationToken = default,
        [CallerArgumentExpression(nameof(valueOutPropertyGet))] string? exprOut = null)
        where TIn : class
        where TOut : INotifyPropertyChanged
    {
        return Disposable.Combine(
            EveryValueChanged(valueIn, valueInPropertyGet, cancellationToken)
                .Subscribe(valueOutPropertySet, (x, state) => state(x)),
            valueOut
                .ObservePropertyChanged(valueOutPropertyGet, false, cancellationToken, exprOut)
                .Subscribe(valueInPropertySet, (x, state) => state(x)));
    }

    public static IDisposable TwoWayBind<TIn, TProperty, TOut>(
        this TIn valueIn, Func<TIn, TProperty> valueInPropertyGet, Func<TProperty, TIn> valueInPropertySet,
        TOut valueOut, Func<TOut, TProperty> valueOutPropertyGet, Func<TProperty, TOut> valueOutPropertySet,
        CancellationToken cancellationToken = default)
        where TIn : class
        where TOut : class
    {
        return Disposable.Combine(
            EveryValueChanged(valueIn, valueInPropertyGet, cancellationToken)
                .Subscribe(valueOutPropertySet, (x, state) => state(x)),
            EveryValueChanged(valueOut, valueOutPropertyGet, cancellationToken)
                .Skip(1)
                .Subscribe(valueInPropertySet, (x, state) => state(x)));
    }
    **/
}
