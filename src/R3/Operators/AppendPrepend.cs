namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> Append<T>(this Observable<T> source, T value)
    {
        return new AppendPrepend<T>(source, value, append: true);
    }

    public static Observable<T> Append<T>(this Observable<T> source, IEnumerable<T> values)
    {
        return new AppendPrependEnumerable<T>(source, values, append: true);
    }

    public static Observable<T> Append<T>(this Observable<T> source, Func<T> valueFactory)
    {
        return new AppendPrependFactory<T>(source, valueFactory, append: true);
    }

    public static Observable<T> Append<T, TState>(this Observable<T> source, TState state, Func<TState, T> valueFactory)
    {
        return new AppendPrependFactory<T, TState>(source, state, valueFactory, append: true);
    }

    public static Observable<T> Prepend<T>(this Observable<T> source, T value)
    {
        return new AppendPrepend<T>(source, value, append: false);
    }

    public static Observable<T> Prepend<T>(this Observable<T> source, IEnumerable<T> values)
    {
        return new AppendPrependEnumerable<T>(source, values, append: false);
    }

    public static Observable<T> Prepend<T>(this Observable<T> source, Func<T> valueFactory)
    {
        return new AppendPrependFactory<T>(source, valueFactory, append: false);
    }

    public static Observable<T> Prepend<T, TState>(this Observable<T> source, TState state, Func<TState, T> valueFactory)
    {
        return new AppendPrependFactory<T, TState>(source, state, valueFactory, append: false);
    }
}

internal sealed class AppendPrepend<T>(Observable<T> source, T value, bool append) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        if (!append) // prepend
        {
            observer.OnNext(value);
            return source.Subscribe(observer.Wrap());
        }

        return source.Subscribe(new _Append(observer, value));
    }

    sealed class _Append(Observer<T> observer, T value) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            observer.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            if (result.IsFailure)
            {
                observer.OnCompleted(result);
            }
            else
            {
                observer.OnNext(value);
                observer.OnCompleted();
            }
        }
    }
}

internal sealed class AppendPrependEnumerable<T>(Observable<T> source, IEnumerable<T> values, bool append) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        if (!append) // prepend
        {
            if (values is T[] array)
            {
                foreach (var value in array)
                {
                    observer.OnNext(value);
                }
            }
            else
            {
                foreach (var value in values)
                {
                    observer.OnNext(value);
                }
            }

            return source.Subscribe(observer.Wrap());
        }

        return source.Subscribe(new _Append(observer, values));
    }

    sealed class _Append(Observer<T> observer, IEnumerable<T> values) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            observer.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            if (result.IsFailure)
            {
                observer.OnCompleted(result);
            }
            else
            {
                if (values is T[] array)
                {
                    foreach (var value in array)
                    {
                        observer.OnNext(value);
                    }
                }
                else
                {
                    foreach (var value in values)
                    {
                        observer.OnNext(value);
                    }
                }

                observer.OnCompleted();
            }
        }
    }
}

internal sealed class AppendPrependFactory<T>(Observable<T> source, Func<T> valueFactory, bool append) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        if (!append) // prepend
        {
            observer.OnNext(valueFactory());
            return source.Subscribe(observer.Wrap());
        }

        return source.Subscribe(new _Append(observer, valueFactory));
    }

    sealed class _Append(Observer<T> observer, Func<T> valueFactory) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            observer.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            if (result.IsFailure)
            {
                observer.OnCompleted(result);
            }
            else
            {
                observer.OnNext(valueFactory());
                observer.OnCompleted();
            }
        }
    }
}

internal sealed class AppendPrependFactory<T, TState>(Observable<T> source, TState state, Func<TState, T> valueFactory, bool append) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        if (!append) // prepend
        {
            observer.OnNext(valueFactory(state));
            return source.Subscribe(observer.Wrap());
        }

        return source.Subscribe(new _Append(observer, state, valueFactory));
    }

    sealed class _Append(Observer<T> observer, TState state, Func<TState, T> valueFactory) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            observer.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            if (result.IsFailure)
            {
                observer.OnCompleted(result);
            }
            else
            {
                observer.OnNext(valueFactory(state));
                observer.OnCompleted();
            }
        }
    }
}
