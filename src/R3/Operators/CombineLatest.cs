namespace R3
{
    public static partial class EventExtensions
    {
        public static Event<TResult> CombineLatest<TLeft, TRight, TResult>(this Event<TLeft> left, Event<TRight> right, Func<TLeft, TRight, TResult> selector)
        {
            return new CombineLatest<TLeft, TRight, TResult>(left, right, selector);
        }

        public static CompletableEvent<TResult, TResultComplete> CombineLatest<TLeft, TRight, TLeftComplete, TRightComplete, TResult, TResultComplete>(
            this CompletableEvent<TLeft, TLeftComplete> left,
            CompletableEvent<TRight, TRightComplete> right,
            Func<TLeft, TRight, TResult> selector,
            Func<TLeftComplete, TRightComplete, TResultComplete> completeSelector)
        {
            return new CombineLatest<TLeft, TRight, TLeftComplete, TRightComplete, TResult, TResultComplete>(left, right, selector, completeSelector);
        }

        public static CompletableEvent<TResult, Unit> CombineLatest<TLeft, TRight, TResult>(
            this CompletableEvent<TLeft, Unit> left,
            CompletableEvent<TRight, Unit> right,
            Func<TLeft, TRight, TResult> selector)
        {
            return new CombineLatest<TLeft, TRight, Unit, Unit, TResult, Unit>(left, right, selector, static (x, y) => default);
        }
    }
}

namespace R3.Operators
{
    internal sealed class CombineLatest<TLeft, TRight, TResult>(Event<TLeft> left, Event<TRight> right, Func<TLeft, TRight, TResult> selector) : Event<TResult>
    {
        protected override IDisposable SubscribeCore(Subscriber<TResult> subscriber)
        {
            var method = new _CombineLatest(subscriber, selector);

            var d1 = left.Subscribe(new LeftSubscriber(method));
            try
            {
                var d2 = right.Subscribe(new RightSubscriber(method));
                return Disposable.Combine(d1, d2);
            }
            catch
            {
                d1.Dispose();
                throw;
            }
        }

        sealed class _CombineLatest(Subscriber<TResult> subscriber, Func<TLeft, TRight, TResult> selector)
        {
            internal TLeft? message1;
            internal bool hasMessage1;

            internal TRight? message2;
            internal bool hasMessage2;

            public void OnErrorResume(Exception error)
            {
                subscriber.OnErrorResume(error);
            }

            internal void Publish()
            {
                if (hasMessage1 && hasMessage2)
                {
                    var result = selector(message1!, message2!);
                    subscriber.OnNext(result);
                }
            }
        }

        sealed class LeftSubscriber(_CombineLatest parent) : Subscriber<TLeft>
        {
            protected override void OnNextCore(TLeft message)
            {
                lock (parent) // `_CombineLatest` is hide in Disposable.Combine so safe to use lock
                {
                    parent.hasMessage1 = true;
                    parent.message1 = message;
                    parent.Publish();
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                lock (parent)
                {
                    parent.OnErrorResume(error);
                }
            }
        }

        sealed class RightSubscriber(_CombineLatest parent) : Subscriber<TRight>
        {
            protected override void OnNextCore(TRight message)
            {
                lock (parent) // `_CombineLatest` is hide in Disposable.Combine so safe to use lock
                {
                    parent.hasMessage2 = true;
                    parent.message2 = message;
                    parent.Publish();
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                lock (parent)
                {
                    parent.OnErrorResume(error);
                }
            }
        }
    }

    internal sealed class CombineLatest<TLeft, TRight, TLeftComplete, TRightComplete, TResult, TResultComplete>(
        CompletableEvent<TLeft, TLeftComplete> left,
        CompletableEvent<TRight, TRightComplete> right,
        Func<TLeft, TRight, TResult> selector,
        Func<TLeftComplete, TRightComplete, TResultComplete> completeSelector) : CompletableEvent<TResult, TResultComplete>
    {
        protected override IDisposable SubscribeCore(Subscriber<TResult, TResultComplete> subscriber)
        {
            var method = new _CombineLatest(subscriber, selector, completeSelector);

            var d1 = left.Subscribe(new LeftSubscriber(method));
            try
            {
                var d2 = right.Subscribe(new RightSubscriber(method));
                return Disposable.Combine(d1, d2);
            }
            catch
            {
                d1.Dispose();
                throw;
            }
        }

        sealed class _CombineLatest(Subscriber<TResult, TResultComplete> subscriber, Func<TLeft, TRight, TResult> selector, Func<TLeftComplete, TRightComplete, TResultComplete> completeSelector)
        {
            internal TLeft? message1;
            internal bool hasMessage1;
            internal TLeftComplete? complete1;
            internal bool hasComplete1;

            internal TRight? message2;
            internal bool hasMessage2;
            internal TRightComplete? complete2;
            internal bool hasComplete2;

            public void OnErrorResume(Exception error)
            {
                subscriber.OnErrorResume(error);
            }

            internal void Publish()
            {
                if (hasMessage1 && hasMessage2)
                {
                    var result = selector(message1!, message2!);
                    subscriber.OnNext(result);
                }
            }

            internal void Complete()
            {
                if (hasComplete1 && hasComplete2)
                {
                    var result = completeSelector(complete1!, complete2!);
                    subscriber.OnCompleted(result);
                }
            }
        }

        sealed class LeftSubscriber(_CombineLatest parent) : Subscriber<TLeft, TLeftComplete>
        {
            protected override void OnNextCore(TLeft message)
            {
                lock (parent) // `_CombineLatest` is hide in Disposable.Combine so safe to use lock
                {
                    parent.hasMessage1 = true;
                    parent.message1 = message;
                    parent.Publish();
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                lock (parent)
                {
                    parent.OnErrorResume(error);
                }
            }

            protected override void OnCompletedCore(TLeftComplete complete)
            {
                lock (parent)
                {
                    parent.hasComplete1 = true;
                    parent.complete1 = complete;
                    parent.Complete();
                }
            }
        }

        sealed class RightSubscriber(_CombineLatest parent) : Subscriber<TRight, TRightComplete>
        {
            protected override void OnNextCore(TRight message)
            {
                lock (parent) // `_CombineLatest` is hide in Disposable.Combine so safe to use lock
                {
                    parent.hasMessage2 = true;
                    parent.message2 = message;
                    parent.Publish();
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                lock (parent)
                {
                    parent.OnErrorResume(error);
                }
            }

            protected override void OnCompletedCore(TRightComplete complete)
            {
                lock (parent)
                {
                    parent.hasComplete2 = true;
                    parent.complete2 = complete;
                    parent.Complete();
                }
            }
        }
    }
}
