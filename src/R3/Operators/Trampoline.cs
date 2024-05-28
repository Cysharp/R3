namespace R3;

public static partial class ObservableExtensions
{
    /// <summary>
    /// Similar as ObserveOn(CurrentThreadScheduler) in dotnet/reactive, place the execution order of recursive calls after the call is completed.
    /// </summary>
    public static Observable<T> Trampoline<T>(this Observable<T> source)
    {
        return new Trampoline<T>(source);
    }
}

internal sealed class Trampoline<T>(Observable<T> source) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _Trampoline(observer));
    }

    sealed class _Trampoline(Observer<T> observer) : Observer<T>
    {
        readonly Queue<Notification<T>> queue = new();
        bool running;

        protected override bool AutoDisposeOnCompleted => false;

        protected override void OnNextCore(T value)
        {
            EnqueueMessage(new(value));
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            EnqueueMessage(new(error));
        }

        protected override void OnCompletedCore(Result result)
        {
            EnqueueMessage(new(result));
        }

        void EnqueueMessage(Notification<T> notification)
        {
            lock (queue)
            {
                queue.Enqueue(notification);
                if (!running)
                {
                    running = true;
                    DrainMessages();
                }
            }
        }

        void DrainMessages()
        {
        AGAIN:
            Notification<T> value;
            lock (queue)
            {
                if (IsDisposed)
                {
                    queue.Clear();
                    return;
                }

                if (!queue.TryDequeue(out value))
                {
                    running = false;
                    return;
                }
            }

            try
            {
                switch (value.Kind)
                {
                    case NotificationKind.OnNext:
                        observer.OnNext(value.Value);
                        break;
                    case NotificationKind.OnErrorResume:
                        observer.OnErrorResume(value.Error);
                        break;
                    case NotificationKind.OnCompleted:
                        try
                        {
                            observer.OnCompleted(value.Result);
                        }
                        finally
                        {
                            Dispose();
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    ObservableSystem.GetUnhandledExceptionHandler().Invoke(ex);
                }
                catch { }
            }

            goto AGAIN;
        }
    }
}
