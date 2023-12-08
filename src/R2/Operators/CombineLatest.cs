namespace R2;

public static partial class EventExtensions
{
    public static IEvent<TResult> CombineLatest<TLeft, TRight, TResult>(this IEvent<TLeft> left, IEvent<TRight> right, Func<TLeft, TRight, TResult> selector)
    {
        return new CombineLatest<TLeft, TRight, TResult>(left, right, selector);
    }
}

internal sealed class CombineLatest<TLeft, TRight, TResult>(IEvent<TLeft> left, IEvent<TRight> right, Func<TLeft, TRight, TResult> selector) : IEvent<TResult>
{
    public IDisposable Subscribe(ISubscriber<TResult> subscriber)
    {
        var method = new _CombineLatest(subscriber, selector);

        var d1 = left.Subscribe(new LeftSubscriber(method));
        var d2 = right.Subscribe(new RightSubscriber(method));

        return Disposable.Combine(d1, d2);
    }

    class _CombineLatest(ISubscriber<TResult> subscriber, Func<TLeft, TRight, TResult> selector)
    {
        TLeft? message1;
        bool hasMessage1;
        TRight? message2;
        bool hasMessage2;

        public void OnNext(TLeft message)
        {
            var canPublish = false;
            TRight? msg2 = default;
            lock (this)
            {
                hasMessage1 = true;
                message1 = message;

                if (hasMessage2)
                {
                    canPublish = true;
                    msg2 = message2;
                }
            }

            if (canPublish)
            {
                Publish(message, msg2!);
            }
        }

        public void OnNext(TRight message)
        {
            var canPublish = false;
            TLeft? msg1 = default;
            lock (this)
            {
                hasMessage2 = true;
                message2 = message;

                if (hasMessage1)
                {
                    canPublish = true;
                    msg1 = message1;
                }
            }

            if (canPublish)
            {
                Publish(msg1!, message);
            }
        }

        void Publish(TLeft m1, TRight m2)
        {
            var result = selector(m1, m2);
            subscriber.OnNext(result);
        }
    }

    sealed class LeftSubscriber(_CombineLatest parent) : ISubscriber<TLeft>
    {
        public void OnNext(TLeft message)
        {
            parent.OnNext(message);
        }
    }

    sealed class RightSubscriber(_CombineLatest parent) : ISubscriber<TRight>
    {
        public void OnNext(TRight message)
        {
            parent.OnNext(message);
        }
    }
}
