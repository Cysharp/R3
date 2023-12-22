namespace R3;

public static partial class Observable
{
    public static Observable<(object? sender, EventArgs e)> FromEventHandler(Action<EventHandler> addHandler, Action<EventHandler> removeHandler)
    {
        return new FromEvent<EventHandler, (object? sender, EventArgs e)>(h => (sender, e) => h((sender, e)), addHandler, removeHandler);
    }

    public static Observable<(object? sender, TEventArgs e)> FromEventHandler<TEventArgs>(Action<EventHandler<TEventArgs>> addHandler, Action<EventHandler<TEventArgs>> removeHandler)
    {
        return new FromEvent<EventHandler<TEventArgs>, (object? sender, TEventArgs e)>(h => (sender, e) => h((sender, e)), addHandler, removeHandler);
    }

    public static Observable<Unit> FromEvent(Action<Action> addHandler, Action<Action> removeHandler)
    {
        return new FromEvent<Action>(static h => h, addHandler, removeHandler);
    }

    public static Observable<T> FromEvent<T>(Action<Action<T>> addHandler, Action<Action<T>> removeHandler)
    {
        return new FromEvent<Action<T>, T>(static h => h, addHandler, removeHandler);
    }

    public static Observable<Unit> FromEvent<TDelegate>(Func<Action, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler)
    {
        return new FromEvent<TDelegate>(conversion, addHandler, removeHandler);
    }

    public static Observable<T> FromEvent<TDelegate, T>(Func<Action<T>, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler)
    {
        return new FromEvent<TDelegate, T>(conversion, addHandler, removeHandler);
    }
}

internal sealed class FromEvent<TDelegate>(Func<Action, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler)
    : Observable<Unit>
{
    protected override IDisposable SubscribeCore(Observer<Unit> observer)
    {
        return new _FromEventPattern(conversion, addHandler, removeHandler, observer);
    }

    sealed class _FromEventPattern : IDisposable
    {
        Observer<Unit>? observer;
        Action<TDelegate>? removeHandler;
        TDelegate registeredHandler;

        public _FromEventPattern(Func<Action, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler, Observer<Unit> observer)
        {
            this.observer = observer;
            this.removeHandler = removeHandler;
            this.registeredHandler = conversion(OnNext);
            addHandler(this.registeredHandler);
        }

        void OnNext()
        {
            observer?.OnNext(default);
        }

        public void Dispose()
        {
            var handler = Interlocked.Exchange(ref removeHandler, null);
            if (handler != null)
            {
                observer = null;
                removeHandler = null;
                handler(this.registeredHandler);
            }
        }
    }
}

internal sealed class FromEvent<TDelegate, T>(Func<Action<T>, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler)
    : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return new _FromEventPattern(conversion, addHandler, removeHandler, observer);
    }

    sealed class _FromEventPattern : IDisposable
    {
        Observer<T>? observer;
        Action<TDelegate>? removeHandler;
        TDelegate registeredHandler;

        public _FromEventPattern(Func<Action<T>, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler, Observer<T> observer)
        {
            this.observer = observer;
            this.removeHandler = removeHandler;
            this.registeredHandler = conversion(OnNext);
            addHandler(this.registeredHandler);
        }

        void OnNext(T value)
        {
            observer?.OnNext(value);
        }

        public void Dispose()
        {
            var handler = Interlocked.Exchange(ref removeHandler, null);
            if (handler != null)
            {
                observer = null;
                removeHandler = null;
                handler(this.registeredHandler);
            }
        }
    }
}
