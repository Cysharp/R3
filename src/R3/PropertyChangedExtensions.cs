using System.ComponentModel;

namespace R3;

public static class PropertyChangedExtensions
{
    /// <summary>
    /// Extension method for INotifyPropertyChanged interface.
    /// It creates an observable sequence that produces a value when the specified property has changed.
    /// It will always emit the current value of the property upon subscription
    /// </summary>
    /// <typeparam name="TIn">The type of the object that implements INotifyPropertyChanged.</typeparam>
    /// <typeparam name="TOut">The type of the value to be produced by the observable sequence.</typeparam>
    /// <param name="propertyChanged">The object that implements INotifyPropertyChanged.</param>
    /// <param name="propertyNameIn">The name of the property to observe for changes.</param>
    /// <param name="valueSelect">A function to select the value to be produced by the observable sequence.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>An observable sequence that produces a value when the specified property has changed.</returns>

    public static Observable<TOut> WhenChanged<TIn, TOut>(this TIn propertyChanged, string propertyNameIn, Func<TIn, TOut> valueSelect, CancellationToken cancellationToken = default)
        where TIn : INotifyPropertyChanged
    {
        var initialValue = valueSelect(propertyChanged);

        return
            Observable.Merge(
                Observable.Return(initialValue),
                Observable
                    .FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                        static eventHandler =>
                        {
                            void Handler(object? sender, PropertyChangedEventArgs e) => eventHandler?.Invoke(e);
                            return Handler;
                        },
                        x => propertyChanged.PropertyChanged += x,
                        x => propertyChanged.PropertyChanged -= x,
                        cancellationToken)
                    .Where(propertyNameIn, static (args, state) => args.PropertyName.Equals(state))
                    .Select((valueSelect, propertyChanged), static (_, state) => state.valueSelect(state.propertyChanged)));
    }

    /// <summary>
    /// Extension method for INotifyPropertyChanging interface.
    /// It creates an observable sequence that produces a value when the specified property is changing.
    /// It will always emit the current value of the property upon subscription
    /// </summary>
    /// <typeparam name="TIn">The type of the object that implements INotifyPropertyChanging.</typeparam>
    /// <typeparam name="TOut">The type of the value to be produced by the observable sequence.</typeparam>
    /// <param name="propertyChanging">The object that implements INotifyPropertyChanging.</param>
    /// <param name="propertyNameIn">The name of the property to observe for changes.</param>
    /// <param name="valueSelect">A function to select the value to be produced by the observable sequence.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>An observable sequence that produces a value when the specified property is changing.</returns>

    public static Observable<TOut> WhenChanging<TIn, TOut>(this TIn propertyChanging, string propertyNameIn, Func<TIn, TOut> valueSelect, CancellationToken cancellationToken = default)
        where TIn : INotifyPropertyChanging
    {
        var initialValue = valueSelect(propertyChanging);

        return
            Observable.Merge(
                Observable.Return(initialValue),
                Observable
                    .FromEvent<PropertyChangingEventHandler, PropertyChangingEventArgs>(
                        static eventHandler =>
                        {
                            void Handler(object? sender, PropertyChangingEventArgs e) => eventHandler?.Invoke(e);
                            return Handler;
                        },
                        x => propertyChanging.PropertyChanging += x,
                        x => propertyChanging.PropertyChanging -= x,
                        cancellationToken)
                    .Where(propertyNameIn, static (args, state) => args.PropertyName.Equals(state))
                    .Select((valueSelect, propertyChanging), static (_, state) => state.valueSelect(state.propertyChanging)));
    }
}
