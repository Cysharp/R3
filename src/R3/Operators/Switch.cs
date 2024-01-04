namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> Switch<T>(this Observable<Observable<T>> sources)
    {
        return new Switch<T>(sources);
    }
}

internal sealed class Switch<T>(Observable<Observable<T>> sources) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return sources.Subscribe(new _Switch(observer));
    }

    sealed class _Switch(Observer<T> observer) : Observer<Observable<T>>
    {
        public Observer<T> observer = observer;
        public readonly object gate = new object();

        SerialDisposableCore subscription;
        public ulong id;
        public bool runningInner;
        public bool stoppedOuter;

        // keep when inner is running
        protected override bool AutoDisposeOnCompleted => false;

        protected override void OnNextCore(Observable<T> value)
        {
            var innerId = default(ulong);
            lock (gate)
            {
                innerId = id = unchecked(id + 1);
                runningInner = true;
            }

            // subscribe new inner with id token
            subscription.Disposable = value.Subscribe(new SwitchObserver(this, innerId));
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            lock (gate)
            {
                if (result.IsFailure)
                {
                    try
                    {
                        observer.OnCompleted(result);
                    }
                    finally
                    {
                        Dispose();
                    }
                    return;
                }

                // stop both outer and inner, complete.
                stoppedOuter = true;
                if (!runningInner)
                {
                    try
                    {
                        observer.OnCompleted();
                    }
                    finally
                    {
                        Dispose();
                    }
                }
            }
        }

        protected override void DisposeCore()
        {
            subscription.Dispose();
        }
    }

    sealed class SwitchObserver(_Switch parent, ulong id) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            lock (parent.gate)
            {
                if (parent.id == id) // if not matched, subscribe new inner has been started
                {
                    parent.observer.OnNext(value);
                }
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            lock (parent.gate)
            {
                if (parent.id == id)
                {
                    parent.observer.OnErrorResume(error);
                }
            }
        }

        protected override void OnCompletedCore(Result result)
        {
            lock (parent.gate)
            {
                if (parent.id == id)
                {
                    if (result.IsFailure)
                    {
                        parent.observer.OnCompleted(result);
                    }
                    else
                    {
                        // if already outer is stopped, complete.
                        parent.runningInner = false;
                        if (parent.stoppedOuter)
                        {
                            parent.observer.OnCompleted(result);
                        }
                    }
                }
            }
        }
    }
}
