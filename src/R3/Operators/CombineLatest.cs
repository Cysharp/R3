//namespace R3;

//public static partial class EventExtensions
//{
//    public static Event<TResult> CombineLatest<TLeft, TRight, TResult>(
//        this Event<TLeft> left,
//        Event<TRight> right,
//        Func<TLeft, TRight, TResult> selector)
//    {
//        return new CombineLatest<TLeft, TRight, TResult>(left, right, selector);
//    }
//}

//internal sealed class CombineLatest<TLeft, TRight, TResult>(Event<TLeft> left, Event<TRight> right, Func<TLeft, TRight, TResult> selector) : Event<TResult>
//{
//    protected override IDisposable SubscribeCore(Subscriber<TResult> subscriber)
//    {
//        var method = new _CombineLatest(subscriber, selector);
//        var left = new LeftSubscriber(method);
//        var right = new RightSubscriber(method);

//        var leftD = left.Subscribe(new LeftSubscriber(method));
//        try
//        {
//            var rightD = right.Subscribe(new RightSubscriber(method));
//            return Disposable.Combine(leftD, rightD);
//        }
//        catch
//        {
//            leftD.Dispose();
//            throw;
//        }
//    }

//    internal sealed class _CombineLatest(Subscriber<TResult> subscriber, Func<TLeft, TRight, TResult> selector)
//    {
//        internal TLeft? message1;
//        internal bool hasMessage1;
//        internal bool hasComplete1;

//        internal TRight? message2;
//        internal bool hasMessage2;
//        internal bool hasComplete2;

//        public void OnErrorResume(Exception error)
//        {
//            subscriber.OnErrorResume(error);
//        }

//        internal void Publish()
//        {
//            if (hasMessage1 && hasMessage2)
//            {
//                var result = selector(message1!, message2!);
//                subscriber.OnNext(result);
//            }
//        }

//        internal void Complete()
//        {
//            if (hasComplete1 && hasComplete2)
//            {
//                var result = completeSelector(complete1!, complete2!);
//                subscriber.OnCompleted(result);
//            }
//        }

//        internal void CompleteError()
//        {
//            if (hasComplete1 && hasComplete2)
//            {
//                var result = completeSelector(complete1!, complete2!);
//                subscriber.OnCompleted(result);
//            }
//        }
//    }

//    internal sealed class LeftSubscriber(_CombineLatest parent) : Subscriber<TLeft>
//    {
//        protected override void OnNextCore(TLeft message)
//        {
//            lock (parent) // `_CombineLatest` is hide in Disposable.Combine so safe to use lock
//            {
//                parent.hasMessage1 = true;
//                parent.message1 = message;
//                parent.Publish();
//            }
//        }

//        protected override void OnErrorResumeCore(Exception error)
//        {
//            lock (parent)
//            {
//                parent.OnErrorResume(error);
//            }
//        }

//        protected override void OnCompletedCore(Result complete)
//        {
//            lock (parent)
//            {
//                parent.hasComplete1 = true;
//                parent.complete1 = complete;
//                parent.Complete();
//            }
//        }
//    }

//    internal sealed class RightSubscriber(_CombineLatest parent) : Subscriber<TRight>
//    {
//        protected override void OnNextCore(TRight message)
//        {
//            lock (parent) // `_CombineLatest` is hide in Disposable.Combine so safe to use lock
//            {
//                parent.hasMessage2 = true;
//                parent.message2 = message;
//                parent.Publish();
//            }
//        }

//        protected override void OnErrorResumeCore(Exception error)
//        {
//            lock (parent)
//            {
//                parent.OnErrorResume(error);
//            }
//        }

//        protected override void OnCompletedCore(Result complete)
//        {
//            lock (parent)
//            {
//                parent.hasComplete2 = true;
//                parent.complete2 = complete;
//                parent.Complete();
//            }
//        }
//    }
//}



