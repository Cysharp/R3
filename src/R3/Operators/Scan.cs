﻿namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<TSource> Scan<TSource>(this Observable<TSource> source, Func<TSource, TSource, TSource> accumulator)
    {
        return new Scan<TSource>(source, accumulator);
    }

    public static Observable<TAccumulate> Scan<TSource, TAccumulate>(this Observable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator)
    {
        return new Scan<TSource, TAccumulate>(source, seed, accumulator);
    }
}

internal sealed class Scan<TSource>(Observable<TSource> source, Func<TSource, TSource, TSource> accumulator) : Observable<TSource>
{
    protected override IDisposable SubscribeCore(Observer<TSource> observer)
    {
        return source.Subscribe(new _Scan(observer, accumulator));
    }

    sealed class _Scan : Observer<TSource>
    {
        readonly Observer<TSource> observer;
        readonly Func<TSource, TSource, TSource> accumulator;
        TSource accumulation;
        bool hasValue;

        public _Scan(Observer<TSource> observer, Func<TSource, TSource, TSource> accumulator)
        {
            this.observer = observer;
            this.accumulation = default!;
            this.accumulator = accumulator;
        }

        protected override void OnNextCore(TSource value)
        {
            if (!hasValue)
            {
                hasValue = true;
                accumulation = value;
                observer.OnNext(accumulation);
                return;
            }

            accumulation = accumulator(accumulation, value);
            observer.OnNext(accumulation);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            observer.OnCompleted(result);
        }
    }
}

internal sealed class Scan<TSource, TAccumulate>(Observable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator) : Observable<TAccumulate>
{
    protected override IDisposable SubscribeCore(Observer<TAccumulate> observer)
    {
        return source.Subscribe(new _Scan(observer, seed, accumulator));
    }

    sealed class _Scan : Observer<TSource>
    {
        readonly Observer<TAccumulate> observer;
        readonly Func<TAccumulate, TSource, TAccumulate> accumulator;
        TAccumulate accumulation;

        public _Scan(Observer<TAccumulate> observer, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator)
        {
            this.observer = observer;
            this.accumulation = seed;
            this.accumulator = accumulator;
        }

        protected override void OnNextCore(TSource value)
        {
            accumulation = accumulator(accumulation, value);
            observer.OnNext(accumulation);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            observer.OnCompleted(result);
        }
    }
}
