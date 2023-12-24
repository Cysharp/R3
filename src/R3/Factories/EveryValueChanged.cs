namespace R3;

public static partial class Observable
{
    public static Observable<TProperty> EveryValueChanged<TSource, TProperty>(TSource source, Func<TSource, TProperty> propertySelector, CancellationToken cancellationToken = default)
        where TSource : class
    {
        return EveryValueChanged(source, propertySelector, ObservableSystem.DefaultFrameProvider, EqualityComparer<TProperty>.Default, cancellationToken);
    }

    public static Observable<TProperty> EveryValueChanged<TSource, TProperty>(TSource source, Func<TSource, TProperty> propertySelector, FrameProvider frameProvider, CancellationToken cancellationToken = default)
        where TSource : class
    {
        return EveryValueChanged(source, propertySelector, frameProvider, EqualityComparer<TProperty>.Default, cancellationToken);
    }

    public static Observable<TProperty> EveryValueChanged<TSource, TProperty>(TSource source, Func<TSource, TProperty> propertySelector, EqualityComparer<TProperty> equalityComparer, CancellationToken cancellationToken = default)
        where TSource : class
    {
        return EveryValueChanged(source, propertySelector, ObservableSystem.DefaultFrameProvider, equalityComparer, cancellationToken);
    }

    public static Observable<TProperty> EveryValueChanged<TSource, TProperty>(TSource source, Func<TSource, TProperty> propertySelector, FrameProvider frameProvider, EqualityComparer<TProperty> equalityComparer, CancellationToken cancellationToken = default)
        where TSource : class
    {
        return new EveryValueChanged<TSource, TProperty>(source, propertySelector, frameProvider, equalityComparer, cancellationToken);
    }
}

internal sealed class EveryValueChanged<TSource, TProperty>(TSource source, Func<TSource, TProperty> propertySelector, FrameProvider frameProvider, EqualityComparer<TProperty> equalityComparer, CancellationToken cancellationToken) : Observable<TProperty>
    where TSource : class
{
    protected override IDisposable SubscribeCore(Observer<TProperty> observer)
    {
        // raise latest value on subscribe
        var value = propertySelector(source);
        observer.OnNext(value);
        if (observer.IsDisposed)
        {
            return Disposable.Empty;
        }

        var runner = new EveryValueChangedRunnerWorkItem(observer, source, value, propertySelector, equalityComparer, cancellationToken);
        frameProvider.Register(runner);
        return runner;
    }

    sealed class EveryValueChangedRunnerWorkItem(Observer<TProperty> observer, TSource source, TProperty previousValue, Func<TSource, TProperty> propertySelector, EqualityComparer<TProperty> equalityComparer, CancellationToken cancellationToken)
        : CancellableFrameRunnerWorkItemBase<TProperty>(observer, cancellationToken)
    {
        protected override bool MoveNextCore(long _)
        {
            TProperty currentValue;
            try
            {
                currentValue = propertySelector(source);
            }
            catch (Exception ex)
            { 
                PublishOnCompleted(ex); // when error, stop.
                return false;
            }

            if (equalityComparer.Equals(previousValue, currentValue))
            {
                // don't emit but continue.
                return true;
            }

            previousValue = currentValue;
            PublishOnNext(currentValue); // emit latest
            return true;
        }
    }
}


