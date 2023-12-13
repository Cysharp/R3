namespace R3
{
    public static class SubscriberExtensions
    {
        public static void OnCompleted<T>(this Subscriber<T, Unit> subscriber)
        {
            subscriber.OnCompleted(default);
        }

        public static IObserver<T> ToObserver<T>(this Subscriber<T, Unit> subscriber)
        {
            return new SubscriberToObserver<T>(subscriber);
        }

        public static IObserver<T> ToObserver<T>(this Subscriber<T, Result<Unit>> subscriber)
        {
            return new SubscriberToObserverR<T>(subscriber);
        }
    }
}

namespace R3.Operators
{
    internal sealed class SubscriberToObserver<T>(Subscriber<T, Unit> subscriber) : IObserver<T>
    {
        public void OnNext(T value)
        {
            subscriber.OnNext(value);
        }

        public void OnError(Exception error)
        {
            try
            {
                subscriber.OnErrorResume(error);
            }
            finally
            {
                subscriber.Dispose();
            }
        }

        public void OnCompleted()
        {
            subscriber.OnCompleted(default);
        }
    }

    internal sealed class SubscriberToObserverR<T>(Subscriber<T, Result<Unit>> subscriber) : IObserver<T>
    {
        public void OnNext(T value)
        {
            subscriber.OnNext(value);
        }

        public void OnError(Exception error)
        {
            subscriber.OnCompleted(Result.Failure<Unit>(error));
        }

        public void OnCompleted()
        {
            subscriber.OnCompleted(Result.Success(Unit.Default));
        }
    }
}
