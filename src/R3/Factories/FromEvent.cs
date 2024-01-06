namespace R3;

public static partial class Observable
{
    public static Observable<(object? sender, EventArgs e)> FromEventHandler(Action<EventHandler> addHandler, Action<EventHandler> removeHandler, CancellationToken cancellationToken = default)
    {
        return new FromEvent<EventHandler, (object? sender, EventArgs e)>(h => (sender, e) => h((sender, e)), addHandler, removeHandler, cancellationToken);
    }

    public static Observable<(object? sender, TEventArgs e)> FromEventHandler<TEventArgs>(Action<EventHandler<TEventArgs>> addHandler, Action<EventHandler<TEventArgs>> removeHandler, CancellationToken cancellationToken = default)
    {
        return new FromEvent<EventHandler<TEventArgs>, (object? sender, TEventArgs e)>(h => (sender, e) => h((sender, e)), addHandler, removeHandler, cancellationToken);
    }

    public static Observable<Unit> FromEvent(Action<Action> addHandler, Action<Action> removeHandler, CancellationToken cancellationToken = default)
    {
        return new FromEvent<Action>(static h => h, addHandler, removeHandler, cancellationToken);
    }

    public static Observable<T> FromEvent<T>(Action<Action<T>> addHandler, Action<Action<T>> removeHandler, CancellationToken cancellationToken = default)
    {
        return new FromEvent<Action<T>, T>(static h => h, addHandler, removeHandler, cancellationToken);
    }

    public static Observable<Unit> FromEvent<TDelegate>(Func<Action, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler, CancellationToken cancellationToken = default)
    {
        return new FromEvent<TDelegate>(conversion, addHandler, removeHandler, cancellationToken);
    }

    public static Observable<T> FromEvent<TDelegate, T>(Func<Action<T>, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler, CancellationToken cancellationToken = default)
    {
        return new FromEvent<TDelegate, T>(conversion, addHandler, removeHandler, cancellationToken);
    }
}

internal sealed class FromEvent<TDelegate>(Func<Action, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler, CancellationToken cancellationToken)
    : Observable<Unit>
{
    protected override IDisposable SubscribeCore(Observer<Unit> observer)
    {
        return new _FromEventPattern(conversion, addHandler, removeHandler, observer, cancellationToken);
    }

    sealed class _FromEventPattern : IDisposable
    {
        Observer<Unit>? observer;
        Action<TDelegate>? removeHandler;
        TDelegate registeredHandler;
        CancellationTokenRegistration cancellationTokenRegistration;

        public _FromEventPattern(Func<Action, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler, Observer<Unit> observer, CancellationToken cancellationToken)
        {
            this.observer = observer;
            this.removeHandler = removeHandler;
            this.registeredHandler = conversion(OnNext);
            addHandler(this.registeredHandler);

            if (cancellationToken.CanBeCanceled)
            {
                this.cancellationTokenRegistration = cancellationToken.UnsafeRegister(static state =>
                {
                    var s = (_FromEventPattern)state!;
                    s.CompleteDispose();
                }, this);
            }
        }

        void OnNext()
        {
            observer?.OnNext(default);
        }

        void CompleteDispose()
        {
            observer?.OnCompleted();
            Dispose();
        }

        public void Dispose()
        {
            var handler = Interlocked.Exchange(ref removeHandler, null);
            if (handler != null)
            {
                observer = null;
                removeHandler = null;
                cancellationTokenRegistration.Dispose();
                handler(this.registeredHandler);
            }
        }
    }
}

internal sealed class FromEvent<TDelegate, T>(Func<Action<T>, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler, CancellationToken cancellationToken)
    : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return new _FromEventPattern(conversion, addHandler, removeHandler, observer, cancellationToken);
    }

    sealed class _FromEventPattern : IDisposable
    {
        Observer<T>? observer;
        Action<TDelegate>? removeHandler;
        TDelegate registeredHandler;
        CancellationTokenRegistration cancellationTokenRegistration;

        public _FromEventPattern(Func<Action<T>, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler, Observer<T> observer, CancellationToken cancellationToken)
        {
            this.observer = observer;
            this.removeHandler = removeHandler;
            this.registeredHandler = conversion(OnNext);
            addHandler(this.registeredHandler);

            if (cancellationToken.CanBeCanceled)
            {
                this.cancellationTokenRegistration = cancellationToken.UnsafeRegister(static state =>
                {
                    var s = (_FromEventPattern)state!;
                    s.CompleteDispose();
                }, this);
            }
        }

        void OnNext(T value)
        {
            observer?.OnNext(value);
        }

        void CompleteDispose()
        {
            observer?.OnCompleted();
            Dispose();
        }

        public void Dispose()
        {
            var handler = Interlocked.Exchange(ref removeHandler, null);
            if (handler != null)
            {
                observer = null;
                removeHandler = null;
                cancellationTokenRegistration.Dispose();
                handler(this.registeredHandler);
            }
        }
    }
}
