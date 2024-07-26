namespace R3;

public static partial class Observable
{
    public static Observable<TResult> ZipLatest<T1, T2, TResult>(
        this
        Observable<T1> source1,
        Observable<T2> source2,
        Func<T1, T2, TResult> resultSelector)
    {
        return new ZipLatest<T1, T2, TResult>(source1, source2, resultSelector);
    }

    public static Observable<TResult> ZipLatest<T1, T2, T3, TResult>(
        this
        Observable<T1> source1,
        Observable<T2> source2,
        Observable<T3> source3,
        Func<T1, T2, T3, TResult> resultSelector)
    {
        return new ZipLatest<T1, T2, T3, TResult>(source1, source2, source3, resultSelector);
    }

    public static Observable<TResult> ZipLatest<T1, T2, T3, T4, TResult>(
        this
        Observable<T1> source1,
        Observable<T2> source2,
        Observable<T3> source3,
        Observable<T4> source4,
        Func<T1, T2, T3, T4, TResult> resultSelector)
    {
        return new ZipLatest<T1, T2, T3, T4, TResult>(source1, source2, source3, source4, resultSelector);
    }

    public static Observable<TResult> ZipLatest<T1, T2, T3, T4, T5, TResult>(
        this
        Observable<T1> source1,
        Observable<T2> source2,
        Observable<T3> source3,
        Observable<T4> source4,
        Observable<T5> source5,
        Func<T1, T2, T3, T4, T5, TResult> resultSelector)
    {
        return new ZipLatest<T1, T2, T3, T4, T5, TResult>(source1, source2, source3, source4, source5, resultSelector);
    }

    public static Observable<TResult> ZipLatest<T1, T2, T3, T4, T5, T6, TResult>(
        this
        Observable<T1> source1,
        Observable<T2> source2,
        Observable<T3> source3,
        Observable<T4> source4,
        Observable<T5> source5,
        Observable<T6> source6,
        Func<T1, T2, T3, T4, T5, T6, TResult> resultSelector)
    {
        return new ZipLatest<T1, T2, T3, T4, T5, T6, TResult>(source1, source2, source3, source4, source5, source6, resultSelector);
    }

    public static Observable<TResult> ZipLatest<T1, T2, T3, T4, T5, T6, T7, TResult>(
        this
        Observable<T1> source1,
        Observable<T2> source2,
        Observable<T3> source3,
        Observable<T4> source4,
        Observable<T5> source5,
        Observable<T6> source6,
        Observable<T7> source7,
        Func<T1, T2, T3, T4, T5, T6, T7, TResult> resultSelector)
    {
        return new ZipLatest<T1, T2, T3, T4, T5, T6, T7, TResult>(source1, source2, source3, source4, source5, source6, source7, resultSelector);
    }

    public static Observable<TResult> ZipLatest<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(
        this
        Observable<T1> source1,
        Observable<T2> source2,
        Observable<T3> source3,
        Observable<T4> source4,
        Observable<T5> source5,
        Observable<T6> source6,
        Observable<T7> source7,
        Observable<T8> source8,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> resultSelector)
    {
        return new ZipLatest<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(source1, source2, source3, source4, source5, source6, source7, source8, resultSelector);
    }

    public static Observable<TResult> ZipLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(
        this
        Observable<T1> source1,
        Observable<T2> source2,
        Observable<T3> source3,
        Observable<T4> source4,
        Observable<T5> source5,
        Observable<T6> source6,
        Observable<T7> source7,
        Observable<T8> source8,
        Observable<T9> source9,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> resultSelector)
    {
        return new ZipLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(source1, source2, source3, source4, source5, source6, source7, source8, source9, resultSelector);
    }

    public static Observable<TResult> ZipLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(
        this
        Observable<T1> source1,
        Observable<T2> source2,
        Observable<T3> source3,
        Observable<T4> source4,
        Observable<T5> source5,
        Observable<T6> source6,
        Observable<T7> source7,
        Observable<T8> source8,
        Observable<T9> source9,
        Observable<T10> source10,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> resultSelector)
    {
        return new ZipLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, resultSelector);
    }

    public static Observable<TResult> ZipLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(
        this
        Observable<T1> source1,
        Observable<T2> source2,
        Observable<T3> source3,
        Observable<T4> source4,
        Observable<T5> source5,
        Observable<T6> source6,
        Observable<T7> source7,
        Observable<T8> source8,
        Observable<T9> source9,
        Observable<T10> source10,
        Observable<T11> source11,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> resultSelector)
    {
        return new ZipLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, resultSelector);
    }

    public static Observable<TResult> ZipLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(
        this
        Observable<T1> source1,
        Observable<T2> source2,
        Observable<T3> source3,
        Observable<T4> source4,
        Observable<T5> source5,
        Observable<T6> source6,
        Observable<T7> source7,
        Observable<T8> source8,
        Observable<T9> source9,
        Observable<T10> source10,
        Observable<T11> source11,
        Observable<T12> source12,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> resultSelector)
    {
        return new ZipLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, resultSelector);
    }

    public static Observable<TResult> ZipLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(
        this
        Observable<T1> source1,
        Observable<T2> source2,
        Observable<T3> source3,
        Observable<T4> source4,
        Observable<T5> source5,
        Observable<T6> source6,
        Observable<T7> source7,
        Observable<T8> source8,
        Observable<T9> source9,
        Observable<T10> source10,
        Observable<T11> source11,
        Observable<T12> source12,
        Observable<T13> source13,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> resultSelector)
    {
        return new ZipLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, source13, resultSelector);
    }

    public static Observable<TResult> ZipLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(
        this
        Observable<T1> source1,
        Observable<T2> source2,
        Observable<T3> source3,
        Observable<T4> source4,
        Observable<T5> source5,
        Observable<T6> source6,
        Observable<T7> source7,
        Observable<T8> source8,
        Observable<T9> source9,
        Observable<T10> source10,
        Observable<T11> source11,
        Observable<T12> source12,
        Observable<T13> source13,
        Observable<T14> source14,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> resultSelector)
    {
        return new ZipLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, source13, source14, resultSelector);
    }

    public static Observable<TResult> ZipLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(
        this
        Observable<T1> source1,
        Observable<T2> source2,
        Observable<T3> source3,
        Observable<T4> source4,
        Observable<T5> source5,
        Observable<T6> source6,
        Observable<T7> source7,
        Observable<T8> source8,
        Observable<T9> source9,
        Observable<T10> source10,
        Observable<T11> source11,
        Observable<T12> source12,
        Observable<T13> source13,
        Observable<T14> source14,
        Observable<T15> source15,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> resultSelector)
    {
        return new ZipLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, source13, source14, source15, resultSelector);
    }

}

internal sealed class ZipLatest<T1, T2, TResult>(
    Observable<T1> source1,
    Observable<T2> source2,
    Func<T1, T2, TResult> resultSelector) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        return new _ZipLatest(observer, source1, source2, resultSelector).Run();
    }

    sealed class _ZipLatest : IDisposable
    {
        readonly Observer<TResult> observer;
        readonly Observable<T1> source1;
        readonly Observable<T2> source2;
        readonly Func<T1, T2, TResult> resultSelector;
        readonly ZipLatestObserver<T1> observer1;
        readonly ZipLatestObserver<T2> observer2;
        
        readonly object gate = new object();

        public _ZipLatest(
            Observer<TResult> observer,
            Observable<T1> source1,
            Observable<T2> source2,
            Func<T1, T2, TResult> resultSelector)
        {
            this.observer = observer;
            this.source1 = source1;
            this.source2 = source2;
            this.resultSelector = resultSelector;
            this.observer1 = new ZipLatestObserver<T1>(this);
            this.observer2 = new ZipLatestObserver<T2>(this);
        }

        public IDisposable Run()
        {
            try
            {
                source1.Subscribe(observer1);
                source2.Subscribe(observer2);
            }
            catch
            {
                Dispose();
                throw;
            }
            return this;
        }

        public void TryPublishOnNext()
        {
            if (observer1.HasValue && observer2.HasValue)
            {
                var result = resultSelector(observer1.GetValue(), observer2.GetValue());
                observer.OnNext(result);
                
                if (observer1.IsCompleted || observer2.IsCompleted)
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void TryPublishOnCompleted(Result result, bool empty)
        {
            if (result.IsFailure)
            {
                observer.OnCompleted(result);
                Dispose();
            }
            else
            {
                if (empty || (observer1.IsCompleted && observer2.IsCompleted))
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void Dispose()
        {
            observer1.Dispose();
            observer2.Dispose();
        }

        sealed class ZipLatestObserver<T>(_ZipLatest parent) : Observer<T>
        {
            T? value;
            public bool HasValue { get; private set; }
            public bool IsCompleted { get; private set; }

            public T GetValue()
            {
                var v = this.value;
                this.value = default;
                this.HasValue = false;
                return v!;
            }

            protected override void OnNextCore(T value)
            {
                lock (parent.gate)
                {
                    this.value = value;
                    this.HasValue = true;
                    parent.TryPublishOnNext();
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                parent.observer.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                lock (parent.gate)
                {
                    IsCompleted = true;
                    parent.TryPublishOnCompleted(result, !HasValue);
                }
            }
        }
    }
}

internal sealed class ZipLatest<T1, T2, T3, TResult>(
    Observable<T1> source1,
    Observable<T2> source2,
    Observable<T3> source3,
    Func<T1, T2, T3, TResult> resultSelector) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        return new _ZipLatest(observer, source1, source2, source3, resultSelector).Run();
    }

    sealed class _ZipLatest : IDisposable
    {
        readonly Observer<TResult> observer;
        readonly Observable<T1> source1;
        readonly Observable<T2> source2;
        readonly Observable<T3> source3;
        readonly Func<T1, T2, T3, TResult> resultSelector;
        readonly ZipLatestObserver<T1> observer1;
        readonly ZipLatestObserver<T2> observer2;
        readonly ZipLatestObserver<T3> observer3;
        
        readonly object gate = new object();

        public _ZipLatest(
            Observer<TResult> observer,
            Observable<T1> source1,
            Observable<T2> source2,
            Observable<T3> source3,
            Func<T1, T2, T3, TResult> resultSelector)
        {
            this.observer = observer;
            this.source1 = source1;
            this.source2 = source2;
            this.source3 = source3;
            this.resultSelector = resultSelector;
            this.observer1 = new ZipLatestObserver<T1>(this);
            this.observer2 = new ZipLatestObserver<T2>(this);
            this.observer3 = new ZipLatestObserver<T3>(this);
        }

        public IDisposable Run()
        {
            try
            {
                source1.Subscribe(observer1);
                source2.Subscribe(observer2);
                source3.Subscribe(observer3);
            }
            catch
            {
                Dispose();
                throw;
            }
            return this;
        }

        public void TryPublishOnNext()
        {
            if (observer1.HasValue && observer2.HasValue && observer3.HasValue)
            {
                var result = resultSelector(observer1.GetValue(), observer2.GetValue(), observer3.GetValue());
                observer.OnNext(result);
                
                if (observer1.IsCompleted || observer2.IsCompleted || observer3.IsCompleted)
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void TryPublishOnCompleted(Result result, bool empty)
        {
            if (result.IsFailure)
            {
                observer.OnCompleted(result);
                Dispose();
            }
            else
            {
                if (empty || (observer1.IsCompleted && observer2.IsCompleted && observer3.IsCompleted))
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void Dispose()
        {
            observer1.Dispose();
            observer2.Dispose();
            observer3.Dispose();
        }

        sealed class ZipLatestObserver<T>(_ZipLatest parent) : Observer<T>
        {
            T? value;
            public bool HasValue { get; private set; }
            public bool IsCompleted { get; private set; }

            public T GetValue()
            {
                var v = this.value;
                this.value = default;
                this.HasValue = false;
                return v!;
            }

            protected override void OnNextCore(T value)
            {
                lock (parent.gate)
                {
                    this.value = value;
                    this.HasValue = true;
                    parent.TryPublishOnNext();
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                parent.observer.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                lock (parent.gate)
                {
                    IsCompleted = true;
                    parent.TryPublishOnCompleted(result, !HasValue);
                }
            }
        }
    }
}

internal sealed class ZipLatest<T1, T2, T3, T4, TResult>(
    Observable<T1> source1,
    Observable<T2> source2,
    Observable<T3> source3,
    Observable<T4> source4,
    Func<T1, T2, T3, T4, TResult> resultSelector) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        return new _ZipLatest(observer, source1, source2, source3, source4, resultSelector).Run();
    }

    sealed class _ZipLatest : IDisposable
    {
        readonly Observer<TResult> observer;
        readonly Observable<T1> source1;
        readonly Observable<T2> source2;
        readonly Observable<T3> source3;
        readonly Observable<T4> source4;
        readonly Func<T1, T2, T3, T4, TResult> resultSelector;
        readonly ZipLatestObserver<T1> observer1;
        readonly ZipLatestObserver<T2> observer2;
        readonly ZipLatestObserver<T3> observer3;
        readonly ZipLatestObserver<T4> observer4;
        
        readonly object gate = new object();

        public _ZipLatest(
            Observer<TResult> observer,
            Observable<T1> source1,
            Observable<T2> source2,
            Observable<T3> source3,
            Observable<T4> source4,
            Func<T1, T2, T3, T4, TResult> resultSelector)
        {
            this.observer = observer;
            this.source1 = source1;
            this.source2 = source2;
            this.source3 = source3;
            this.source4 = source4;
            this.resultSelector = resultSelector;
            this.observer1 = new ZipLatestObserver<T1>(this);
            this.observer2 = new ZipLatestObserver<T2>(this);
            this.observer3 = new ZipLatestObserver<T3>(this);
            this.observer4 = new ZipLatestObserver<T4>(this);
        }

        public IDisposable Run()
        {
            try
            {
                source1.Subscribe(observer1);
                source2.Subscribe(observer2);
                source3.Subscribe(observer3);
                source4.Subscribe(observer4);
            }
            catch
            {
                Dispose();
                throw;
            }
            return this;
        }

        public void TryPublishOnNext()
        {
            if (observer1.HasValue && observer2.HasValue && observer3.HasValue && observer4.HasValue)
            {
                var result = resultSelector(observer1.GetValue(), observer2.GetValue(), observer3.GetValue(), observer4.GetValue());
                observer.OnNext(result);
                
                if (observer1.IsCompleted || observer2.IsCompleted || observer3.IsCompleted || observer4.IsCompleted)
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void TryPublishOnCompleted(Result result, bool empty)
        {
            if (result.IsFailure)
            {
                observer.OnCompleted(result);
                Dispose();
            }
            else
            {
                if (empty || (observer1.IsCompleted && observer2.IsCompleted && observer3.IsCompleted && observer4.IsCompleted))
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void Dispose()
        {
            observer1.Dispose();
            observer2.Dispose();
            observer3.Dispose();
            observer4.Dispose();
        }

        sealed class ZipLatestObserver<T>(_ZipLatest parent) : Observer<T>
        {
            T? value;
            public bool HasValue { get; private set; }
            public bool IsCompleted { get; private set; }

            public T GetValue()
            {
                var v = this.value;
                this.value = default;
                this.HasValue = false;
                return v!;
            }

            protected override void OnNextCore(T value)
            {
                lock (parent.gate)
                {
                    this.value = value;
                    this.HasValue = true;
                    parent.TryPublishOnNext();
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                parent.observer.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                lock (parent.gate)
                {
                    IsCompleted = true;
                    parent.TryPublishOnCompleted(result, !HasValue);
                }
            }
        }
    }
}

internal sealed class ZipLatest<T1, T2, T3, T4, T5, TResult>(
    Observable<T1> source1,
    Observable<T2> source2,
    Observable<T3> source3,
    Observable<T4> source4,
    Observable<T5> source5,
    Func<T1, T2, T3, T4, T5, TResult> resultSelector) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        return new _ZipLatest(observer, source1, source2, source3, source4, source5, resultSelector).Run();
    }

    sealed class _ZipLatest : IDisposable
    {
        readonly Observer<TResult> observer;
        readonly Observable<T1> source1;
        readonly Observable<T2> source2;
        readonly Observable<T3> source3;
        readonly Observable<T4> source4;
        readonly Observable<T5> source5;
        readonly Func<T1, T2, T3, T4, T5, TResult> resultSelector;
        readonly ZipLatestObserver<T1> observer1;
        readonly ZipLatestObserver<T2> observer2;
        readonly ZipLatestObserver<T3> observer3;
        readonly ZipLatestObserver<T4> observer4;
        readonly ZipLatestObserver<T5> observer5;
        
        readonly object gate = new object();

        public _ZipLatest(
            Observer<TResult> observer,
            Observable<T1> source1,
            Observable<T2> source2,
            Observable<T3> source3,
            Observable<T4> source4,
            Observable<T5> source5,
            Func<T1, T2, T3, T4, T5, TResult> resultSelector)
        {
            this.observer = observer;
            this.source1 = source1;
            this.source2 = source2;
            this.source3 = source3;
            this.source4 = source4;
            this.source5 = source5;
            this.resultSelector = resultSelector;
            this.observer1 = new ZipLatestObserver<T1>(this);
            this.observer2 = new ZipLatestObserver<T2>(this);
            this.observer3 = new ZipLatestObserver<T3>(this);
            this.observer4 = new ZipLatestObserver<T4>(this);
            this.observer5 = new ZipLatestObserver<T5>(this);
        }

        public IDisposable Run()
        {
            try
            {
                source1.Subscribe(observer1);
                source2.Subscribe(observer2);
                source3.Subscribe(observer3);
                source4.Subscribe(observer4);
                source5.Subscribe(observer5);
            }
            catch
            {
                Dispose();
                throw;
            }
            return this;
        }

        public void TryPublishOnNext()
        {
            if (observer1.HasValue && observer2.HasValue && observer3.HasValue && observer4.HasValue && observer5.HasValue)
            {
                var result = resultSelector(observer1.GetValue(), observer2.GetValue(), observer3.GetValue(), observer4.GetValue(), observer5.GetValue());
                observer.OnNext(result);
                
                if (observer1.IsCompleted || observer2.IsCompleted || observer3.IsCompleted || observer4.IsCompleted || observer5.IsCompleted)
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void TryPublishOnCompleted(Result result, bool empty)
        {
            if (result.IsFailure)
            {
                observer.OnCompleted(result);
                Dispose();
            }
            else
            {
                if (empty || (observer1.IsCompleted && observer2.IsCompleted && observer3.IsCompleted && observer4.IsCompleted && observer5.IsCompleted))
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void Dispose()
        {
            observer1.Dispose();
            observer2.Dispose();
            observer3.Dispose();
            observer4.Dispose();
            observer5.Dispose();
        }

        sealed class ZipLatestObserver<T>(_ZipLatest parent) : Observer<T>
        {
            T? value;
            public bool HasValue { get; private set; }
            public bool IsCompleted { get; private set; }

            public T GetValue()
            {
                var v = this.value;
                this.value = default;
                this.HasValue = false;
                return v!;
            }

            protected override void OnNextCore(T value)
            {
                lock (parent.gate)
                {
                    this.value = value;
                    this.HasValue = true;
                    parent.TryPublishOnNext();
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                parent.observer.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                lock (parent.gate)
                {
                    IsCompleted = true;
                    parent.TryPublishOnCompleted(result, !HasValue);
                }
            }
        }
    }
}

internal sealed class ZipLatest<T1, T2, T3, T4, T5, T6, TResult>(
    Observable<T1> source1,
    Observable<T2> source2,
    Observable<T3> source3,
    Observable<T4> source4,
    Observable<T5> source5,
    Observable<T6> source6,
    Func<T1, T2, T3, T4, T5, T6, TResult> resultSelector) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        return new _ZipLatest(observer, source1, source2, source3, source4, source5, source6, resultSelector).Run();
    }

    sealed class _ZipLatest : IDisposable
    {
        readonly Observer<TResult> observer;
        readonly Observable<T1> source1;
        readonly Observable<T2> source2;
        readonly Observable<T3> source3;
        readonly Observable<T4> source4;
        readonly Observable<T5> source5;
        readonly Observable<T6> source6;
        readonly Func<T1, T2, T3, T4, T5, T6, TResult> resultSelector;
        readonly ZipLatestObserver<T1> observer1;
        readonly ZipLatestObserver<T2> observer2;
        readonly ZipLatestObserver<T3> observer3;
        readonly ZipLatestObserver<T4> observer4;
        readonly ZipLatestObserver<T5> observer5;
        readonly ZipLatestObserver<T6> observer6;
        
        readonly object gate = new object();

        public _ZipLatest(
            Observer<TResult> observer,
            Observable<T1> source1,
            Observable<T2> source2,
            Observable<T3> source3,
            Observable<T4> source4,
            Observable<T5> source5,
            Observable<T6> source6,
            Func<T1, T2, T3, T4, T5, T6, TResult> resultSelector)
        {
            this.observer = observer;
            this.source1 = source1;
            this.source2 = source2;
            this.source3 = source3;
            this.source4 = source4;
            this.source5 = source5;
            this.source6 = source6;
            this.resultSelector = resultSelector;
            this.observer1 = new ZipLatestObserver<T1>(this);
            this.observer2 = new ZipLatestObserver<T2>(this);
            this.observer3 = new ZipLatestObserver<T3>(this);
            this.observer4 = new ZipLatestObserver<T4>(this);
            this.observer5 = new ZipLatestObserver<T5>(this);
            this.observer6 = new ZipLatestObserver<T6>(this);
        }

        public IDisposable Run()
        {
            try
            {
                source1.Subscribe(observer1);
                source2.Subscribe(observer2);
                source3.Subscribe(observer3);
                source4.Subscribe(observer4);
                source5.Subscribe(observer5);
                source6.Subscribe(observer6);
            }
            catch
            {
                Dispose();
                throw;
            }
            return this;
        }

        public void TryPublishOnNext()
        {
            if (observer1.HasValue && observer2.HasValue && observer3.HasValue && observer4.HasValue && observer5.HasValue && observer6.HasValue)
            {
                var result = resultSelector(observer1.GetValue(), observer2.GetValue(), observer3.GetValue(), observer4.GetValue(), observer5.GetValue(), observer6.GetValue());
                observer.OnNext(result);
                
                if (observer1.IsCompleted || observer2.IsCompleted || observer3.IsCompleted || observer4.IsCompleted || observer5.IsCompleted || observer6.IsCompleted)
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void TryPublishOnCompleted(Result result, bool empty)
        {
            if (result.IsFailure)
            {
                observer.OnCompleted(result);
                Dispose();
            }
            else
            {
                if (empty || (observer1.IsCompleted && observer2.IsCompleted && observer3.IsCompleted && observer4.IsCompleted && observer5.IsCompleted && observer6.IsCompleted))
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void Dispose()
        {
            observer1.Dispose();
            observer2.Dispose();
            observer3.Dispose();
            observer4.Dispose();
            observer5.Dispose();
            observer6.Dispose();
        }

        sealed class ZipLatestObserver<T>(_ZipLatest parent) : Observer<T>
        {
            T? value;
            public bool HasValue { get; private set; }
            public bool IsCompleted { get; private set; }

            public T GetValue()
            {
                var v = this.value;
                this.value = default;
                this.HasValue = false;
                return v!;
            }

            protected override void OnNextCore(T value)
            {
                lock (parent.gate)
                {
                    this.value = value;
                    this.HasValue = true;
                    parent.TryPublishOnNext();
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                parent.observer.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                lock (parent.gate)
                {
                    IsCompleted = true;
                    parent.TryPublishOnCompleted(result, !HasValue);
                }
            }
        }
    }
}

internal sealed class ZipLatest<T1, T2, T3, T4, T5, T6, T7, TResult>(
    Observable<T1> source1,
    Observable<T2> source2,
    Observable<T3> source3,
    Observable<T4> source4,
    Observable<T5> source5,
    Observable<T6> source6,
    Observable<T7> source7,
    Func<T1, T2, T3, T4, T5, T6, T7, TResult> resultSelector) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        return new _ZipLatest(observer, source1, source2, source3, source4, source5, source6, source7, resultSelector).Run();
    }

    sealed class _ZipLatest : IDisposable
    {
        readonly Observer<TResult> observer;
        readonly Observable<T1> source1;
        readonly Observable<T2> source2;
        readonly Observable<T3> source3;
        readonly Observable<T4> source4;
        readonly Observable<T5> source5;
        readonly Observable<T6> source6;
        readonly Observable<T7> source7;
        readonly Func<T1, T2, T3, T4, T5, T6, T7, TResult> resultSelector;
        readonly ZipLatestObserver<T1> observer1;
        readonly ZipLatestObserver<T2> observer2;
        readonly ZipLatestObserver<T3> observer3;
        readonly ZipLatestObserver<T4> observer4;
        readonly ZipLatestObserver<T5> observer5;
        readonly ZipLatestObserver<T6> observer6;
        readonly ZipLatestObserver<T7> observer7;
        
        readonly object gate = new object();

        public _ZipLatest(
            Observer<TResult> observer,
            Observable<T1> source1,
            Observable<T2> source2,
            Observable<T3> source3,
            Observable<T4> source4,
            Observable<T5> source5,
            Observable<T6> source6,
            Observable<T7> source7,
            Func<T1, T2, T3, T4, T5, T6, T7, TResult> resultSelector)
        {
            this.observer = observer;
            this.source1 = source1;
            this.source2 = source2;
            this.source3 = source3;
            this.source4 = source4;
            this.source5 = source5;
            this.source6 = source6;
            this.source7 = source7;
            this.resultSelector = resultSelector;
            this.observer1 = new ZipLatestObserver<T1>(this);
            this.observer2 = new ZipLatestObserver<T2>(this);
            this.observer3 = new ZipLatestObserver<T3>(this);
            this.observer4 = new ZipLatestObserver<T4>(this);
            this.observer5 = new ZipLatestObserver<T5>(this);
            this.observer6 = new ZipLatestObserver<T6>(this);
            this.observer7 = new ZipLatestObserver<T7>(this);
        }

        public IDisposable Run()
        {
            try
            {
                source1.Subscribe(observer1);
                source2.Subscribe(observer2);
                source3.Subscribe(observer3);
                source4.Subscribe(observer4);
                source5.Subscribe(observer5);
                source6.Subscribe(observer6);
                source7.Subscribe(observer7);
            }
            catch
            {
                Dispose();
                throw;
            }
            return this;
        }

        public void TryPublishOnNext()
        {
            if (observer1.HasValue && observer2.HasValue && observer3.HasValue && observer4.HasValue && observer5.HasValue && observer6.HasValue && observer7.HasValue)
            {
                var result = resultSelector(observer1.GetValue(), observer2.GetValue(), observer3.GetValue(), observer4.GetValue(), observer5.GetValue(), observer6.GetValue(), observer7.GetValue());
                observer.OnNext(result);
                
                if (observer1.IsCompleted || observer2.IsCompleted || observer3.IsCompleted || observer4.IsCompleted || observer5.IsCompleted || observer6.IsCompleted || observer7.IsCompleted)
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void TryPublishOnCompleted(Result result, bool empty)
        {
            if (result.IsFailure)
            {
                observer.OnCompleted(result);
                Dispose();
            }
            else
            {
                if (empty || (observer1.IsCompleted && observer2.IsCompleted && observer3.IsCompleted && observer4.IsCompleted && observer5.IsCompleted && observer6.IsCompleted && observer7.IsCompleted))
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void Dispose()
        {
            observer1.Dispose();
            observer2.Dispose();
            observer3.Dispose();
            observer4.Dispose();
            observer5.Dispose();
            observer6.Dispose();
            observer7.Dispose();
        }

        sealed class ZipLatestObserver<T>(_ZipLatest parent) : Observer<T>
        {
            T? value;
            public bool HasValue { get; private set; }
            public bool IsCompleted { get; private set; }

            public T GetValue()
            {
                var v = this.value;
                this.value = default;
                this.HasValue = false;
                return v!;
            }

            protected override void OnNextCore(T value)
            {
                lock (parent.gate)
                {
                    this.value = value;
                    this.HasValue = true;
                    parent.TryPublishOnNext();
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                parent.observer.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                lock (parent.gate)
                {
                    IsCompleted = true;
                    parent.TryPublishOnCompleted(result, !HasValue);
                }
            }
        }
    }
}

internal sealed class ZipLatest<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(
    Observable<T1> source1,
    Observable<T2> source2,
    Observable<T3> source3,
    Observable<T4> source4,
    Observable<T5> source5,
    Observable<T6> source6,
    Observable<T7> source7,
    Observable<T8> source8,
    Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> resultSelector) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        return new _ZipLatest(observer, source1, source2, source3, source4, source5, source6, source7, source8, resultSelector).Run();
    }

    sealed class _ZipLatest : IDisposable
    {
        readonly Observer<TResult> observer;
        readonly Observable<T1> source1;
        readonly Observable<T2> source2;
        readonly Observable<T3> source3;
        readonly Observable<T4> source4;
        readonly Observable<T5> source5;
        readonly Observable<T6> source6;
        readonly Observable<T7> source7;
        readonly Observable<T8> source8;
        readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> resultSelector;
        readonly ZipLatestObserver<T1> observer1;
        readonly ZipLatestObserver<T2> observer2;
        readonly ZipLatestObserver<T3> observer3;
        readonly ZipLatestObserver<T4> observer4;
        readonly ZipLatestObserver<T5> observer5;
        readonly ZipLatestObserver<T6> observer6;
        readonly ZipLatestObserver<T7> observer7;
        readonly ZipLatestObserver<T8> observer8;
        
        readonly object gate = new object();

        public _ZipLatest(
            Observer<TResult> observer,
            Observable<T1> source1,
            Observable<T2> source2,
            Observable<T3> source3,
            Observable<T4> source4,
            Observable<T5> source5,
            Observable<T6> source6,
            Observable<T7> source7,
            Observable<T8> source8,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> resultSelector)
        {
            this.observer = observer;
            this.source1 = source1;
            this.source2 = source2;
            this.source3 = source3;
            this.source4 = source4;
            this.source5 = source5;
            this.source6 = source6;
            this.source7 = source7;
            this.source8 = source8;
            this.resultSelector = resultSelector;
            this.observer1 = new ZipLatestObserver<T1>(this);
            this.observer2 = new ZipLatestObserver<T2>(this);
            this.observer3 = new ZipLatestObserver<T3>(this);
            this.observer4 = new ZipLatestObserver<T4>(this);
            this.observer5 = new ZipLatestObserver<T5>(this);
            this.observer6 = new ZipLatestObserver<T6>(this);
            this.observer7 = new ZipLatestObserver<T7>(this);
            this.observer8 = new ZipLatestObserver<T8>(this);
        }

        public IDisposable Run()
        {
            try
            {
                source1.Subscribe(observer1);
                source2.Subscribe(observer2);
                source3.Subscribe(observer3);
                source4.Subscribe(observer4);
                source5.Subscribe(observer5);
                source6.Subscribe(observer6);
                source7.Subscribe(observer7);
                source8.Subscribe(observer8);
            }
            catch
            {
                Dispose();
                throw;
            }
            return this;
        }

        public void TryPublishOnNext()
        {
            if (observer1.HasValue && observer2.HasValue && observer3.HasValue && observer4.HasValue && observer5.HasValue && observer6.HasValue && observer7.HasValue && observer8.HasValue)
            {
                var result = resultSelector(observer1.GetValue(), observer2.GetValue(), observer3.GetValue(), observer4.GetValue(), observer5.GetValue(), observer6.GetValue(), observer7.GetValue(), observer8.GetValue());
                observer.OnNext(result);
                
                if (observer1.IsCompleted || observer2.IsCompleted || observer3.IsCompleted || observer4.IsCompleted || observer5.IsCompleted || observer6.IsCompleted || observer7.IsCompleted || observer8.IsCompleted)
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void TryPublishOnCompleted(Result result, bool empty)
        {
            if (result.IsFailure)
            {
                observer.OnCompleted(result);
                Dispose();
            }
            else
            {
                if (empty || (observer1.IsCompleted && observer2.IsCompleted && observer3.IsCompleted && observer4.IsCompleted && observer5.IsCompleted && observer6.IsCompleted && observer7.IsCompleted && observer8.IsCompleted))
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void Dispose()
        {
            observer1.Dispose();
            observer2.Dispose();
            observer3.Dispose();
            observer4.Dispose();
            observer5.Dispose();
            observer6.Dispose();
            observer7.Dispose();
            observer8.Dispose();
        }

        sealed class ZipLatestObserver<T>(_ZipLatest parent) : Observer<T>
        {
            T? value;
            public bool HasValue { get; private set; }
            public bool IsCompleted { get; private set; }

            public T GetValue()
            {
                var v = this.value;
                this.value = default;
                this.HasValue = false;
                return v!;
            }

            protected override void OnNextCore(T value)
            {
                lock (parent.gate)
                {
                    this.value = value;
                    this.HasValue = true;
                    parent.TryPublishOnNext();
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                parent.observer.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                lock (parent.gate)
                {
                    IsCompleted = true;
                    parent.TryPublishOnCompleted(result, !HasValue);
                }
            }
        }
    }
}

internal sealed class ZipLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(
    Observable<T1> source1,
    Observable<T2> source2,
    Observable<T3> source3,
    Observable<T4> source4,
    Observable<T5> source5,
    Observable<T6> source6,
    Observable<T7> source7,
    Observable<T8> source8,
    Observable<T9> source9,
    Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> resultSelector) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        return new _ZipLatest(observer, source1, source2, source3, source4, source5, source6, source7, source8, source9, resultSelector).Run();
    }

    sealed class _ZipLatest : IDisposable
    {
        readonly Observer<TResult> observer;
        readonly Observable<T1> source1;
        readonly Observable<T2> source2;
        readonly Observable<T3> source3;
        readonly Observable<T4> source4;
        readonly Observable<T5> source5;
        readonly Observable<T6> source6;
        readonly Observable<T7> source7;
        readonly Observable<T8> source8;
        readonly Observable<T9> source9;
        readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> resultSelector;
        readonly ZipLatestObserver<T1> observer1;
        readonly ZipLatestObserver<T2> observer2;
        readonly ZipLatestObserver<T3> observer3;
        readonly ZipLatestObserver<T4> observer4;
        readonly ZipLatestObserver<T5> observer5;
        readonly ZipLatestObserver<T6> observer6;
        readonly ZipLatestObserver<T7> observer7;
        readonly ZipLatestObserver<T8> observer8;
        readonly ZipLatestObserver<T9> observer9;
        
        readonly object gate = new object();

        public _ZipLatest(
            Observer<TResult> observer,
            Observable<T1> source1,
            Observable<T2> source2,
            Observable<T3> source3,
            Observable<T4> source4,
            Observable<T5> source5,
            Observable<T6> source6,
            Observable<T7> source7,
            Observable<T8> source8,
            Observable<T9> source9,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> resultSelector)
        {
            this.observer = observer;
            this.source1 = source1;
            this.source2 = source2;
            this.source3 = source3;
            this.source4 = source4;
            this.source5 = source5;
            this.source6 = source6;
            this.source7 = source7;
            this.source8 = source8;
            this.source9 = source9;
            this.resultSelector = resultSelector;
            this.observer1 = new ZipLatestObserver<T1>(this);
            this.observer2 = new ZipLatestObserver<T2>(this);
            this.observer3 = new ZipLatestObserver<T3>(this);
            this.observer4 = new ZipLatestObserver<T4>(this);
            this.observer5 = new ZipLatestObserver<T5>(this);
            this.observer6 = new ZipLatestObserver<T6>(this);
            this.observer7 = new ZipLatestObserver<T7>(this);
            this.observer8 = new ZipLatestObserver<T8>(this);
            this.observer9 = new ZipLatestObserver<T9>(this);
        }

        public IDisposable Run()
        {
            try
            {
                source1.Subscribe(observer1);
                source2.Subscribe(observer2);
                source3.Subscribe(observer3);
                source4.Subscribe(observer4);
                source5.Subscribe(observer5);
                source6.Subscribe(observer6);
                source7.Subscribe(observer7);
                source8.Subscribe(observer8);
                source9.Subscribe(observer9);
            }
            catch
            {
                Dispose();
                throw;
            }
            return this;
        }

        public void TryPublishOnNext()
        {
            if (observer1.HasValue && observer2.HasValue && observer3.HasValue && observer4.HasValue && observer5.HasValue && observer6.HasValue && observer7.HasValue && observer8.HasValue && observer9.HasValue)
            {
                var result = resultSelector(observer1.GetValue(), observer2.GetValue(), observer3.GetValue(), observer4.GetValue(), observer5.GetValue(), observer6.GetValue(), observer7.GetValue(), observer8.GetValue(), observer9.GetValue());
                observer.OnNext(result);
                
                if (observer1.IsCompleted || observer2.IsCompleted || observer3.IsCompleted || observer4.IsCompleted || observer5.IsCompleted || observer6.IsCompleted || observer7.IsCompleted || observer8.IsCompleted || observer9.IsCompleted)
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void TryPublishOnCompleted(Result result, bool empty)
        {
            if (result.IsFailure)
            {
                observer.OnCompleted(result);
                Dispose();
            }
            else
            {
                if (empty || (observer1.IsCompleted && observer2.IsCompleted && observer3.IsCompleted && observer4.IsCompleted && observer5.IsCompleted && observer6.IsCompleted && observer7.IsCompleted && observer8.IsCompleted && observer9.IsCompleted))
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void Dispose()
        {
            observer1.Dispose();
            observer2.Dispose();
            observer3.Dispose();
            observer4.Dispose();
            observer5.Dispose();
            observer6.Dispose();
            observer7.Dispose();
            observer8.Dispose();
            observer9.Dispose();
        }

        sealed class ZipLatestObserver<T>(_ZipLatest parent) : Observer<T>
        {
            T? value;
            public bool HasValue { get; private set; }
            public bool IsCompleted { get; private set; }

            public T GetValue()
            {
                var v = this.value;
                this.value = default;
                this.HasValue = false;
                return v!;
            }

            protected override void OnNextCore(T value)
            {
                lock (parent.gate)
                {
                    this.value = value;
                    this.HasValue = true;
                    parent.TryPublishOnNext();
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                parent.observer.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                lock (parent.gate)
                {
                    IsCompleted = true;
                    parent.TryPublishOnCompleted(result, !HasValue);
                }
            }
        }
    }
}

internal sealed class ZipLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(
    Observable<T1> source1,
    Observable<T2> source2,
    Observable<T3> source3,
    Observable<T4> source4,
    Observable<T5> source5,
    Observable<T6> source6,
    Observable<T7> source7,
    Observable<T8> source8,
    Observable<T9> source9,
    Observable<T10> source10,
    Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> resultSelector) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        return new _ZipLatest(observer, source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, resultSelector).Run();
    }

    sealed class _ZipLatest : IDisposable
    {
        readonly Observer<TResult> observer;
        readonly Observable<T1> source1;
        readonly Observable<T2> source2;
        readonly Observable<T3> source3;
        readonly Observable<T4> source4;
        readonly Observable<T5> source5;
        readonly Observable<T6> source6;
        readonly Observable<T7> source7;
        readonly Observable<T8> source8;
        readonly Observable<T9> source9;
        readonly Observable<T10> source10;
        readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> resultSelector;
        readonly ZipLatestObserver<T1> observer1;
        readonly ZipLatestObserver<T2> observer2;
        readonly ZipLatestObserver<T3> observer3;
        readonly ZipLatestObserver<T4> observer4;
        readonly ZipLatestObserver<T5> observer5;
        readonly ZipLatestObserver<T6> observer6;
        readonly ZipLatestObserver<T7> observer7;
        readonly ZipLatestObserver<T8> observer8;
        readonly ZipLatestObserver<T9> observer9;
        readonly ZipLatestObserver<T10> observer10;
        
        readonly object gate = new object();

        public _ZipLatest(
            Observer<TResult> observer,
            Observable<T1> source1,
            Observable<T2> source2,
            Observable<T3> source3,
            Observable<T4> source4,
            Observable<T5> source5,
            Observable<T6> source6,
            Observable<T7> source7,
            Observable<T8> source8,
            Observable<T9> source9,
            Observable<T10> source10,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> resultSelector)
        {
            this.observer = observer;
            this.source1 = source1;
            this.source2 = source2;
            this.source3 = source3;
            this.source4 = source4;
            this.source5 = source5;
            this.source6 = source6;
            this.source7 = source7;
            this.source8 = source8;
            this.source9 = source9;
            this.source10 = source10;
            this.resultSelector = resultSelector;
            this.observer1 = new ZipLatestObserver<T1>(this);
            this.observer2 = new ZipLatestObserver<T2>(this);
            this.observer3 = new ZipLatestObserver<T3>(this);
            this.observer4 = new ZipLatestObserver<T4>(this);
            this.observer5 = new ZipLatestObserver<T5>(this);
            this.observer6 = new ZipLatestObserver<T6>(this);
            this.observer7 = new ZipLatestObserver<T7>(this);
            this.observer8 = new ZipLatestObserver<T8>(this);
            this.observer9 = new ZipLatestObserver<T9>(this);
            this.observer10 = new ZipLatestObserver<T10>(this);
        }

        public IDisposable Run()
        {
            try
            {
                source1.Subscribe(observer1);
                source2.Subscribe(observer2);
                source3.Subscribe(observer3);
                source4.Subscribe(observer4);
                source5.Subscribe(observer5);
                source6.Subscribe(observer6);
                source7.Subscribe(observer7);
                source8.Subscribe(observer8);
                source9.Subscribe(observer9);
                source10.Subscribe(observer10);
            }
            catch
            {
                Dispose();
                throw;
            }
            return this;
        }

        public void TryPublishOnNext()
        {
            if (observer1.HasValue && observer2.HasValue && observer3.HasValue && observer4.HasValue && observer5.HasValue && observer6.HasValue && observer7.HasValue && observer8.HasValue && observer9.HasValue && observer10.HasValue)
            {
                var result = resultSelector(observer1.GetValue(), observer2.GetValue(), observer3.GetValue(), observer4.GetValue(), observer5.GetValue(), observer6.GetValue(), observer7.GetValue(), observer8.GetValue(), observer9.GetValue(), observer10.GetValue());
                observer.OnNext(result);
                
                if (observer1.IsCompleted || observer2.IsCompleted || observer3.IsCompleted || observer4.IsCompleted || observer5.IsCompleted || observer6.IsCompleted || observer7.IsCompleted || observer8.IsCompleted || observer9.IsCompleted || observer10.IsCompleted)
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void TryPublishOnCompleted(Result result, bool empty)
        {
            if (result.IsFailure)
            {
                observer.OnCompleted(result);
                Dispose();
            }
            else
            {
                if (empty || (observer1.IsCompleted && observer2.IsCompleted && observer3.IsCompleted && observer4.IsCompleted && observer5.IsCompleted && observer6.IsCompleted && observer7.IsCompleted && observer8.IsCompleted && observer9.IsCompleted && observer10.IsCompleted))
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void Dispose()
        {
            observer1.Dispose();
            observer2.Dispose();
            observer3.Dispose();
            observer4.Dispose();
            observer5.Dispose();
            observer6.Dispose();
            observer7.Dispose();
            observer8.Dispose();
            observer9.Dispose();
            observer10.Dispose();
        }

        sealed class ZipLatestObserver<T>(_ZipLatest parent) : Observer<T>
        {
            T? value;
            public bool HasValue { get; private set; }
            public bool IsCompleted { get; private set; }

            public T GetValue()
            {
                var v = this.value;
                this.value = default;
                this.HasValue = false;
                return v!;
            }

            protected override void OnNextCore(T value)
            {
                lock (parent.gate)
                {
                    this.value = value;
                    this.HasValue = true;
                    parent.TryPublishOnNext();
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                parent.observer.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                lock (parent.gate)
                {
                    IsCompleted = true;
                    parent.TryPublishOnCompleted(result, !HasValue);
                }
            }
        }
    }
}

internal sealed class ZipLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(
    Observable<T1> source1,
    Observable<T2> source2,
    Observable<T3> source3,
    Observable<T4> source4,
    Observable<T5> source5,
    Observable<T6> source6,
    Observable<T7> source7,
    Observable<T8> source8,
    Observable<T9> source9,
    Observable<T10> source10,
    Observable<T11> source11,
    Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> resultSelector) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        return new _ZipLatest(observer, source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, resultSelector).Run();
    }

    sealed class _ZipLatest : IDisposable
    {
        readonly Observer<TResult> observer;
        readonly Observable<T1> source1;
        readonly Observable<T2> source2;
        readonly Observable<T3> source3;
        readonly Observable<T4> source4;
        readonly Observable<T5> source5;
        readonly Observable<T6> source6;
        readonly Observable<T7> source7;
        readonly Observable<T8> source8;
        readonly Observable<T9> source9;
        readonly Observable<T10> source10;
        readonly Observable<T11> source11;
        readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> resultSelector;
        readonly ZipLatestObserver<T1> observer1;
        readonly ZipLatestObserver<T2> observer2;
        readonly ZipLatestObserver<T3> observer3;
        readonly ZipLatestObserver<T4> observer4;
        readonly ZipLatestObserver<T5> observer5;
        readonly ZipLatestObserver<T6> observer6;
        readonly ZipLatestObserver<T7> observer7;
        readonly ZipLatestObserver<T8> observer8;
        readonly ZipLatestObserver<T9> observer9;
        readonly ZipLatestObserver<T10> observer10;
        readonly ZipLatestObserver<T11> observer11;
        
        readonly object gate = new object();

        public _ZipLatest(
            Observer<TResult> observer,
            Observable<T1> source1,
            Observable<T2> source2,
            Observable<T3> source3,
            Observable<T4> source4,
            Observable<T5> source5,
            Observable<T6> source6,
            Observable<T7> source7,
            Observable<T8> source8,
            Observable<T9> source9,
            Observable<T10> source10,
            Observable<T11> source11,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> resultSelector)
        {
            this.observer = observer;
            this.source1 = source1;
            this.source2 = source2;
            this.source3 = source3;
            this.source4 = source4;
            this.source5 = source5;
            this.source6 = source6;
            this.source7 = source7;
            this.source8 = source8;
            this.source9 = source9;
            this.source10 = source10;
            this.source11 = source11;
            this.resultSelector = resultSelector;
            this.observer1 = new ZipLatestObserver<T1>(this);
            this.observer2 = new ZipLatestObserver<T2>(this);
            this.observer3 = new ZipLatestObserver<T3>(this);
            this.observer4 = new ZipLatestObserver<T4>(this);
            this.observer5 = new ZipLatestObserver<T5>(this);
            this.observer6 = new ZipLatestObserver<T6>(this);
            this.observer7 = new ZipLatestObserver<T7>(this);
            this.observer8 = new ZipLatestObserver<T8>(this);
            this.observer9 = new ZipLatestObserver<T9>(this);
            this.observer10 = new ZipLatestObserver<T10>(this);
            this.observer11 = new ZipLatestObserver<T11>(this);
        }

        public IDisposable Run()
        {
            try
            {
                source1.Subscribe(observer1);
                source2.Subscribe(observer2);
                source3.Subscribe(observer3);
                source4.Subscribe(observer4);
                source5.Subscribe(observer5);
                source6.Subscribe(observer6);
                source7.Subscribe(observer7);
                source8.Subscribe(observer8);
                source9.Subscribe(observer9);
                source10.Subscribe(observer10);
                source11.Subscribe(observer11);
            }
            catch
            {
                Dispose();
                throw;
            }
            return this;
        }

        public void TryPublishOnNext()
        {
            if (observer1.HasValue && observer2.HasValue && observer3.HasValue && observer4.HasValue && observer5.HasValue && observer6.HasValue && observer7.HasValue && observer8.HasValue && observer9.HasValue && observer10.HasValue && observer11.HasValue)
            {
                var result = resultSelector(observer1.GetValue(), observer2.GetValue(), observer3.GetValue(), observer4.GetValue(), observer5.GetValue(), observer6.GetValue(), observer7.GetValue(), observer8.GetValue(), observer9.GetValue(), observer10.GetValue(), observer11.GetValue());
                observer.OnNext(result);
                
                if (observer1.IsCompleted || observer2.IsCompleted || observer3.IsCompleted || observer4.IsCompleted || observer5.IsCompleted || observer6.IsCompleted || observer7.IsCompleted || observer8.IsCompleted || observer9.IsCompleted || observer10.IsCompleted || observer11.IsCompleted)
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void TryPublishOnCompleted(Result result, bool empty)
        {
            if (result.IsFailure)
            {
                observer.OnCompleted(result);
                Dispose();
            }
            else
            {
                if (empty || (observer1.IsCompleted && observer2.IsCompleted && observer3.IsCompleted && observer4.IsCompleted && observer5.IsCompleted && observer6.IsCompleted && observer7.IsCompleted && observer8.IsCompleted && observer9.IsCompleted && observer10.IsCompleted && observer11.IsCompleted))
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void Dispose()
        {
            observer1.Dispose();
            observer2.Dispose();
            observer3.Dispose();
            observer4.Dispose();
            observer5.Dispose();
            observer6.Dispose();
            observer7.Dispose();
            observer8.Dispose();
            observer9.Dispose();
            observer10.Dispose();
            observer11.Dispose();
        }

        sealed class ZipLatestObserver<T>(_ZipLatest parent) : Observer<T>
        {
            T? value;
            public bool HasValue { get; private set; }
            public bool IsCompleted { get; private set; }

            public T GetValue()
            {
                var v = this.value;
                this.value = default;
                this.HasValue = false;
                return v!;
            }

            protected override void OnNextCore(T value)
            {
                lock (parent.gate)
                {
                    this.value = value;
                    this.HasValue = true;
                    parent.TryPublishOnNext();
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                parent.observer.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                lock (parent.gate)
                {
                    IsCompleted = true;
                    parent.TryPublishOnCompleted(result, !HasValue);
                }
            }
        }
    }
}

internal sealed class ZipLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(
    Observable<T1> source1,
    Observable<T2> source2,
    Observable<T3> source3,
    Observable<T4> source4,
    Observable<T5> source5,
    Observable<T6> source6,
    Observable<T7> source7,
    Observable<T8> source8,
    Observable<T9> source9,
    Observable<T10> source10,
    Observable<T11> source11,
    Observable<T12> source12,
    Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> resultSelector) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        return new _ZipLatest(observer, source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, resultSelector).Run();
    }

    sealed class _ZipLatest : IDisposable
    {
        readonly Observer<TResult> observer;
        readonly Observable<T1> source1;
        readonly Observable<T2> source2;
        readonly Observable<T3> source3;
        readonly Observable<T4> source4;
        readonly Observable<T5> source5;
        readonly Observable<T6> source6;
        readonly Observable<T7> source7;
        readonly Observable<T8> source8;
        readonly Observable<T9> source9;
        readonly Observable<T10> source10;
        readonly Observable<T11> source11;
        readonly Observable<T12> source12;
        readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> resultSelector;
        readonly ZipLatestObserver<T1> observer1;
        readonly ZipLatestObserver<T2> observer2;
        readonly ZipLatestObserver<T3> observer3;
        readonly ZipLatestObserver<T4> observer4;
        readonly ZipLatestObserver<T5> observer5;
        readonly ZipLatestObserver<T6> observer6;
        readonly ZipLatestObserver<T7> observer7;
        readonly ZipLatestObserver<T8> observer8;
        readonly ZipLatestObserver<T9> observer9;
        readonly ZipLatestObserver<T10> observer10;
        readonly ZipLatestObserver<T11> observer11;
        readonly ZipLatestObserver<T12> observer12;
        
        readonly object gate = new object();

        public _ZipLatest(
            Observer<TResult> observer,
            Observable<T1> source1,
            Observable<T2> source2,
            Observable<T3> source3,
            Observable<T4> source4,
            Observable<T5> source5,
            Observable<T6> source6,
            Observable<T7> source7,
            Observable<T8> source8,
            Observable<T9> source9,
            Observable<T10> source10,
            Observable<T11> source11,
            Observable<T12> source12,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> resultSelector)
        {
            this.observer = observer;
            this.source1 = source1;
            this.source2 = source2;
            this.source3 = source3;
            this.source4 = source4;
            this.source5 = source5;
            this.source6 = source6;
            this.source7 = source7;
            this.source8 = source8;
            this.source9 = source9;
            this.source10 = source10;
            this.source11 = source11;
            this.source12 = source12;
            this.resultSelector = resultSelector;
            this.observer1 = new ZipLatestObserver<T1>(this);
            this.observer2 = new ZipLatestObserver<T2>(this);
            this.observer3 = new ZipLatestObserver<T3>(this);
            this.observer4 = new ZipLatestObserver<T4>(this);
            this.observer5 = new ZipLatestObserver<T5>(this);
            this.observer6 = new ZipLatestObserver<T6>(this);
            this.observer7 = new ZipLatestObserver<T7>(this);
            this.observer8 = new ZipLatestObserver<T8>(this);
            this.observer9 = new ZipLatestObserver<T9>(this);
            this.observer10 = new ZipLatestObserver<T10>(this);
            this.observer11 = new ZipLatestObserver<T11>(this);
            this.observer12 = new ZipLatestObserver<T12>(this);
        }

        public IDisposable Run()
        {
            try
            {
                source1.Subscribe(observer1);
                source2.Subscribe(observer2);
                source3.Subscribe(observer3);
                source4.Subscribe(observer4);
                source5.Subscribe(observer5);
                source6.Subscribe(observer6);
                source7.Subscribe(observer7);
                source8.Subscribe(observer8);
                source9.Subscribe(observer9);
                source10.Subscribe(observer10);
                source11.Subscribe(observer11);
                source12.Subscribe(observer12);
            }
            catch
            {
                Dispose();
                throw;
            }
            return this;
        }

        public void TryPublishOnNext()
        {
            if (observer1.HasValue && observer2.HasValue && observer3.HasValue && observer4.HasValue && observer5.HasValue && observer6.HasValue && observer7.HasValue && observer8.HasValue && observer9.HasValue && observer10.HasValue && observer11.HasValue && observer12.HasValue)
            {
                var result = resultSelector(observer1.GetValue(), observer2.GetValue(), observer3.GetValue(), observer4.GetValue(), observer5.GetValue(), observer6.GetValue(), observer7.GetValue(), observer8.GetValue(), observer9.GetValue(), observer10.GetValue(), observer11.GetValue(), observer12.GetValue());
                observer.OnNext(result);
                
                if (observer1.IsCompleted || observer2.IsCompleted || observer3.IsCompleted || observer4.IsCompleted || observer5.IsCompleted || observer6.IsCompleted || observer7.IsCompleted || observer8.IsCompleted || observer9.IsCompleted || observer10.IsCompleted || observer11.IsCompleted || observer12.IsCompleted)
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void TryPublishOnCompleted(Result result, bool empty)
        {
            if (result.IsFailure)
            {
                observer.OnCompleted(result);
                Dispose();
            }
            else
            {
                if (empty || (observer1.IsCompleted && observer2.IsCompleted && observer3.IsCompleted && observer4.IsCompleted && observer5.IsCompleted && observer6.IsCompleted && observer7.IsCompleted && observer8.IsCompleted && observer9.IsCompleted && observer10.IsCompleted && observer11.IsCompleted && observer12.IsCompleted))
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void Dispose()
        {
            observer1.Dispose();
            observer2.Dispose();
            observer3.Dispose();
            observer4.Dispose();
            observer5.Dispose();
            observer6.Dispose();
            observer7.Dispose();
            observer8.Dispose();
            observer9.Dispose();
            observer10.Dispose();
            observer11.Dispose();
            observer12.Dispose();
        }

        sealed class ZipLatestObserver<T>(_ZipLatest parent) : Observer<T>
        {
            T? value;
            public bool HasValue { get; private set; }
            public bool IsCompleted { get; private set; }

            public T GetValue()
            {
                var v = this.value;
                this.value = default;
                this.HasValue = false;
                return v!;
            }

            protected override void OnNextCore(T value)
            {
                lock (parent.gate)
                {
                    this.value = value;
                    this.HasValue = true;
                    parent.TryPublishOnNext();
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                parent.observer.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                lock (parent.gate)
                {
                    IsCompleted = true;
                    parent.TryPublishOnCompleted(result, !HasValue);
                }
            }
        }
    }
}

internal sealed class ZipLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(
    Observable<T1> source1,
    Observable<T2> source2,
    Observable<T3> source3,
    Observable<T4> source4,
    Observable<T5> source5,
    Observable<T6> source6,
    Observable<T7> source7,
    Observable<T8> source8,
    Observable<T9> source9,
    Observable<T10> source10,
    Observable<T11> source11,
    Observable<T12> source12,
    Observable<T13> source13,
    Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> resultSelector) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        return new _ZipLatest(observer, source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, source13, resultSelector).Run();
    }

    sealed class _ZipLatest : IDisposable
    {
        readonly Observer<TResult> observer;
        readonly Observable<T1> source1;
        readonly Observable<T2> source2;
        readonly Observable<T3> source3;
        readonly Observable<T4> source4;
        readonly Observable<T5> source5;
        readonly Observable<T6> source6;
        readonly Observable<T7> source7;
        readonly Observable<T8> source8;
        readonly Observable<T9> source9;
        readonly Observable<T10> source10;
        readonly Observable<T11> source11;
        readonly Observable<T12> source12;
        readonly Observable<T13> source13;
        readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> resultSelector;
        readonly ZipLatestObserver<T1> observer1;
        readonly ZipLatestObserver<T2> observer2;
        readonly ZipLatestObserver<T3> observer3;
        readonly ZipLatestObserver<T4> observer4;
        readonly ZipLatestObserver<T5> observer5;
        readonly ZipLatestObserver<T6> observer6;
        readonly ZipLatestObserver<T7> observer7;
        readonly ZipLatestObserver<T8> observer8;
        readonly ZipLatestObserver<T9> observer9;
        readonly ZipLatestObserver<T10> observer10;
        readonly ZipLatestObserver<T11> observer11;
        readonly ZipLatestObserver<T12> observer12;
        readonly ZipLatestObserver<T13> observer13;
        
        readonly object gate = new object();

        public _ZipLatest(
            Observer<TResult> observer,
            Observable<T1> source1,
            Observable<T2> source2,
            Observable<T3> source3,
            Observable<T4> source4,
            Observable<T5> source5,
            Observable<T6> source6,
            Observable<T7> source7,
            Observable<T8> source8,
            Observable<T9> source9,
            Observable<T10> source10,
            Observable<T11> source11,
            Observable<T12> source12,
            Observable<T13> source13,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> resultSelector)
        {
            this.observer = observer;
            this.source1 = source1;
            this.source2 = source2;
            this.source3 = source3;
            this.source4 = source4;
            this.source5 = source5;
            this.source6 = source6;
            this.source7 = source7;
            this.source8 = source8;
            this.source9 = source9;
            this.source10 = source10;
            this.source11 = source11;
            this.source12 = source12;
            this.source13 = source13;
            this.resultSelector = resultSelector;
            this.observer1 = new ZipLatestObserver<T1>(this);
            this.observer2 = new ZipLatestObserver<T2>(this);
            this.observer3 = new ZipLatestObserver<T3>(this);
            this.observer4 = new ZipLatestObserver<T4>(this);
            this.observer5 = new ZipLatestObserver<T5>(this);
            this.observer6 = new ZipLatestObserver<T6>(this);
            this.observer7 = new ZipLatestObserver<T7>(this);
            this.observer8 = new ZipLatestObserver<T8>(this);
            this.observer9 = new ZipLatestObserver<T9>(this);
            this.observer10 = new ZipLatestObserver<T10>(this);
            this.observer11 = new ZipLatestObserver<T11>(this);
            this.observer12 = new ZipLatestObserver<T12>(this);
            this.observer13 = new ZipLatestObserver<T13>(this);
        }

        public IDisposable Run()
        {
            try
            {
                source1.Subscribe(observer1);
                source2.Subscribe(observer2);
                source3.Subscribe(observer3);
                source4.Subscribe(observer4);
                source5.Subscribe(observer5);
                source6.Subscribe(observer6);
                source7.Subscribe(observer7);
                source8.Subscribe(observer8);
                source9.Subscribe(observer9);
                source10.Subscribe(observer10);
                source11.Subscribe(observer11);
                source12.Subscribe(observer12);
                source13.Subscribe(observer13);
            }
            catch
            {
                Dispose();
                throw;
            }
            return this;
        }

        public void TryPublishOnNext()
        {
            if (observer1.HasValue && observer2.HasValue && observer3.HasValue && observer4.HasValue && observer5.HasValue && observer6.HasValue && observer7.HasValue && observer8.HasValue && observer9.HasValue && observer10.HasValue && observer11.HasValue && observer12.HasValue && observer13.HasValue)
            {
                var result = resultSelector(observer1.GetValue(), observer2.GetValue(), observer3.GetValue(), observer4.GetValue(), observer5.GetValue(), observer6.GetValue(), observer7.GetValue(), observer8.GetValue(), observer9.GetValue(), observer10.GetValue(), observer11.GetValue(), observer12.GetValue(), observer13.GetValue());
                observer.OnNext(result);
                
                if (observer1.IsCompleted || observer2.IsCompleted || observer3.IsCompleted || observer4.IsCompleted || observer5.IsCompleted || observer6.IsCompleted || observer7.IsCompleted || observer8.IsCompleted || observer9.IsCompleted || observer10.IsCompleted || observer11.IsCompleted || observer12.IsCompleted || observer13.IsCompleted)
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void TryPublishOnCompleted(Result result, bool empty)
        {
            if (result.IsFailure)
            {
                observer.OnCompleted(result);
                Dispose();
            }
            else
            {
                if (empty || (observer1.IsCompleted && observer2.IsCompleted && observer3.IsCompleted && observer4.IsCompleted && observer5.IsCompleted && observer6.IsCompleted && observer7.IsCompleted && observer8.IsCompleted && observer9.IsCompleted && observer10.IsCompleted && observer11.IsCompleted && observer12.IsCompleted && observer13.IsCompleted))
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void Dispose()
        {
            observer1.Dispose();
            observer2.Dispose();
            observer3.Dispose();
            observer4.Dispose();
            observer5.Dispose();
            observer6.Dispose();
            observer7.Dispose();
            observer8.Dispose();
            observer9.Dispose();
            observer10.Dispose();
            observer11.Dispose();
            observer12.Dispose();
            observer13.Dispose();
        }

        sealed class ZipLatestObserver<T>(_ZipLatest parent) : Observer<T>
        {
            T? value;
            public bool HasValue { get; private set; }
            public bool IsCompleted { get; private set; }

            public T GetValue()
            {
                var v = this.value;
                this.value = default;
                this.HasValue = false;
                return v!;
            }

            protected override void OnNextCore(T value)
            {
                lock (parent.gate)
                {
                    this.value = value;
                    this.HasValue = true;
                    parent.TryPublishOnNext();
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                parent.observer.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                lock (parent.gate)
                {
                    IsCompleted = true;
                    parent.TryPublishOnCompleted(result, !HasValue);
                }
            }
        }
    }
}

internal sealed class ZipLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(
    Observable<T1> source1,
    Observable<T2> source2,
    Observable<T3> source3,
    Observable<T4> source4,
    Observable<T5> source5,
    Observable<T6> source6,
    Observable<T7> source7,
    Observable<T8> source8,
    Observable<T9> source9,
    Observable<T10> source10,
    Observable<T11> source11,
    Observable<T12> source12,
    Observable<T13> source13,
    Observable<T14> source14,
    Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> resultSelector) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        return new _ZipLatest(observer, source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, source13, source14, resultSelector).Run();
    }

    sealed class _ZipLatest : IDisposable
    {
        readonly Observer<TResult> observer;
        readonly Observable<T1> source1;
        readonly Observable<T2> source2;
        readonly Observable<T3> source3;
        readonly Observable<T4> source4;
        readonly Observable<T5> source5;
        readonly Observable<T6> source6;
        readonly Observable<T7> source7;
        readonly Observable<T8> source8;
        readonly Observable<T9> source9;
        readonly Observable<T10> source10;
        readonly Observable<T11> source11;
        readonly Observable<T12> source12;
        readonly Observable<T13> source13;
        readonly Observable<T14> source14;
        readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> resultSelector;
        readonly ZipLatestObserver<T1> observer1;
        readonly ZipLatestObserver<T2> observer2;
        readonly ZipLatestObserver<T3> observer3;
        readonly ZipLatestObserver<T4> observer4;
        readonly ZipLatestObserver<T5> observer5;
        readonly ZipLatestObserver<T6> observer6;
        readonly ZipLatestObserver<T7> observer7;
        readonly ZipLatestObserver<T8> observer8;
        readonly ZipLatestObserver<T9> observer9;
        readonly ZipLatestObserver<T10> observer10;
        readonly ZipLatestObserver<T11> observer11;
        readonly ZipLatestObserver<T12> observer12;
        readonly ZipLatestObserver<T13> observer13;
        readonly ZipLatestObserver<T14> observer14;
        
        readonly object gate = new object();

        public _ZipLatest(
            Observer<TResult> observer,
            Observable<T1> source1,
            Observable<T2> source2,
            Observable<T3> source3,
            Observable<T4> source4,
            Observable<T5> source5,
            Observable<T6> source6,
            Observable<T7> source7,
            Observable<T8> source8,
            Observable<T9> source9,
            Observable<T10> source10,
            Observable<T11> source11,
            Observable<T12> source12,
            Observable<T13> source13,
            Observable<T14> source14,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> resultSelector)
        {
            this.observer = observer;
            this.source1 = source1;
            this.source2 = source2;
            this.source3 = source3;
            this.source4 = source4;
            this.source5 = source5;
            this.source6 = source6;
            this.source7 = source7;
            this.source8 = source8;
            this.source9 = source9;
            this.source10 = source10;
            this.source11 = source11;
            this.source12 = source12;
            this.source13 = source13;
            this.source14 = source14;
            this.resultSelector = resultSelector;
            this.observer1 = new ZipLatestObserver<T1>(this);
            this.observer2 = new ZipLatestObserver<T2>(this);
            this.observer3 = new ZipLatestObserver<T3>(this);
            this.observer4 = new ZipLatestObserver<T4>(this);
            this.observer5 = new ZipLatestObserver<T5>(this);
            this.observer6 = new ZipLatestObserver<T6>(this);
            this.observer7 = new ZipLatestObserver<T7>(this);
            this.observer8 = new ZipLatestObserver<T8>(this);
            this.observer9 = new ZipLatestObserver<T9>(this);
            this.observer10 = new ZipLatestObserver<T10>(this);
            this.observer11 = new ZipLatestObserver<T11>(this);
            this.observer12 = new ZipLatestObserver<T12>(this);
            this.observer13 = new ZipLatestObserver<T13>(this);
            this.observer14 = new ZipLatestObserver<T14>(this);
        }

        public IDisposable Run()
        {
            try
            {
                source1.Subscribe(observer1);
                source2.Subscribe(observer2);
                source3.Subscribe(observer3);
                source4.Subscribe(observer4);
                source5.Subscribe(observer5);
                source6.Subscribe(observer6);
                source7.Subscribe(observer7);
                source8.Subscribe(observer8);
                source9.Subscribe(observer9);
                source10.Subscribe(observer10);
                source11.Subscribe(observer11);
                source12.Subscribe(observer12);
                source13.Subscribe(observer13);
                source14.Subscribe(observer14);
            }
            catch
            {
                Dispose();
                throw;
            }
            return this;
        }

        public void TryPublishOnNext()
        {
            if (observer1.HasValue && observer2.HasValue && observer3.HasValue && observer4.HasValue && observer5.HasValue && observer6.HasValue && observer7.HasValue && observer8.HasValue && observer9.HasValue && observer10.HasValue && observer11.HasValue && observer12.HasValue && observer13.HasValue && observer14.HasValue)
            {
                var result = resultSelector(observer1.GetValue(), observer2.GetValue(), observer3.GetValue(), observer4.GetValue(), observer5.GetValue(), observer6.GetValue(), observer7.GetValue(), observer8.GetValue(), observer9.GetValue(), observer10.GetValue(), observer11.GetValue(), observer12.GetValue(), observer13.GetValue(), observer14.GetValue());
                observer.OnNext(result);
                
                if (observer1.IsCompleted || observer2.IsCompleted || observer3.IsCompleted || observer4.IsCompleted || observer5.IsCompleted || observer6.IsCompleted || observer7.IsCompleted || observer8.IsCompleted || observer9.IsCompleted || observer10.IsCompleted || observer11.IsCompleted || observer12.IsCompleted || observer13.IsCompleted || observer14.IsCompleted)
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void TryPublishOnCompleted(Result result, bool empty)
        {
            if (result.IsFailure)
            {
                observer.OnCompleted(result);
                Dispose();
            }
            else
            {
                if (empty || (observer1.IsCompleted && observer2.IsCompleted && observer3.IsCompleted && observer4.IsCompleted && observer5.IsCompleted && observer6.IsCompleted && observer7.IsCompleted && observer8.IsCompleted && observer9.IsCompleted && observer10.IsCompleted && observer11.IsCompleted && observer12.IsCompleted && observer13.IsCompleted && observer14.IsCompleted))
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void Dispose()
        {
            observer1.Dispose();
            observer2.Dispose();
            observer3.Dispose();
            observer4.Dispose();
            observer5.Dispose();
            observer6.Dispose();
            observer7.Dispose();
            observer8.Dispose();
            observer9.Dispose();
            observer10.Dispose();
            observer11.Dispose();
            observer12.Dispose();
            observer13.Dispose();
            observer14.Dispose();
        }

        sealed class ZipLatestObserver<T>(_ZipLatest parent) : Observer<T>
        {
            T? value;
            public bool HasValue { get; private set; }
            public bool IsCompleted { get; private set; }

            public T GetValue()
            {
                var v = this.value;
                this.value = default;
                this.HasValue = false;
                return v!;
            }

            protected override void OnNextCore(T value)
            {
                lock (parent.gate)
                {
                    this.value = value;
                    this.HasValue = true;
                    parent.TryPublishOnNext();
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                parent.observer.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                lock (parent.gate)
                {
                    IsCompleted = true;
                    parent.TryPublishOnCompleted(result, !HasValue);
                }
            }
        }
    }
}

internal sealed class ZipLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(
    Observable<T1> source1,
    Observable<T2> source2,
    Observable<T3> source3,
    Observable<T4> source4,
    Observable<T5> source5,
    Observable<T6> source6,
    Observable<T7> source7,
    Observable<T8> source8,
    Observable<T9> source9,
    Observable<T10> source10,
    Observable<T11> source11,
    Observable<T12> source12,
    Observable<T13> source13,
    Observable<T14> source14,
    Observable<T15> source15,
    Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> resultSelector) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        return new _ZipLatest(observer, source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, source13, source14, source15, resultSelector).Run();
    }

    sealed class _ZipLatest : IDisposable
    {
        readonly Observer<TResult> observer;
        readonly Observable<T1> source1;
        readonly Observable<T2> source2;
        readonly Observable<T3> source3;
        readonly Observable<T4> source4;
        readonly Observable<T5> source5;
        readonly Observable<T6> source6;
        readonly Observable<T7> source7;
        readonly Observable<T8> source8;
        readonly Observable<T9> source9;
        readonly Observable<T10> source10;
        readonly Observable<T11> source11;
        readonly Observable<T12> source12;
        readonly Observable<T13> source13;
        readonly Observable<T14> source14;
        readonly Observable<T15> source15;
        readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> resultSelector;
        readonly ZipLatestObserver<T1> observer1;
        readonly ZipLatestObserver<T2> observer2;
        readonly ZipLatestObserver<T3> observer3;
        readonly ZipLatestObserver<T4> observer4;
        readonly ZipLatestObserver<T5> observer5;
        readonly ZipLatestObserver<T6> observer6;
        readonly ZipLatestObserver<T7> observer7;
        readonly ZipLatestObserver<T8> observer8;
        readonly ZipLatestObserver<T9> observer9;
        readonly ZipLatestObserver<T10> observer10;
        readonly ZipLatestObserver<T11> observer11;
        readonly ZipLatestObserver<T12> observer12;
        readonly ZipLatestObserver<T13> observer13;
        readonly ZipLatestObserver<T14> observer14;
        readonly ZipLatestObserver<T15> observer15;
        
        readonly object gate = new object();

        public _ZipLatest(
            Observer<TResult> observer,
            Observable<T1> source1,
            Observable<T2> source2,
            Observable<T3> source3,
            Observable<T4> source4,
            Observable<T5> source5,
            Observable<T6> source6,
            Observable<T7> source7,
            Observable<T8> source8,
            Observable<T9> source9,
            Observable<T10> source10,
            Observable<T11> source11,
            Observable<T12> source12,
            Observable<T13> source13,
            Observable<T14> source14,
            Observable<T15> source15,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> resultSelector)
        {
            this.observer = observer;
            this.source1 = source1;
            this.source2 = source2;
            this.source3 = source3;
            this.source4 = source4;
            this.source5 = source5;
            this.source6 = source6;
            this.source7 = source7;
            this.source8 = source8;
            this.source9 = source9;
            this.source10 = source10;
            this.source11 = source11;
            this.source12 = source12;
            this.source13 = source13;
            this.source14 = source14;
            this.source15 = source15;
            this.resultSelector = resultSelector;
            this.observer1 = new ZipLatestObserver<T1>(this);
            this.observer2 = new ZipLatestObserver<T2>(this);
            this.observer3 = new ZipLatestObserver<T3>(this);
            this.observer4 = new ZipLatestObserver<T4>(this);
            this.observer5 = new ZipLatestObserver<T5>(this);
            this.observer6 = new ZipLatestObserver<T6>(this);
            this.observer7 = new ZipLatestObserver<T7>(this);
            this.observer8 = new ZipLatestObserver<T8>(this);
            this.observer9 = new ZipLatestObserver<T9>(this);
            this.observer10 = new ZipLatestObserver<T10>(this);
            this.observer11 = new ZipLatestObserver<T11>(this);
            this.observer12 = new ZipLatestObserver<T12>(this);
            this.observer13 = new ZipLatestObserver<T13>(this);
            this.observer14 = new ZipLatestObserver<T14>(this);
            this.observer15 = new ZipLatestObserver<T15>(this);
        }

        public IDisposable Run()
        {
            try
            {
                source1.Subscribe(observer1);
                source2.Subscribe(observer2);
                source3.Subscribe(observer3);
                source4.Subscribe(observer4);
                source5.Subscribe(observer5);
                source6.Subscribe(observer6);
                source7.Subscribe(observer7);
                source8.Subscribe(observer8);
                source9.Subscribe(observer9);
                source10.Subscribe(observer10);
                source11.Subscribe(observer11);
                source12.Subscribe(observer12);
                source13.Subscribe(observer13);
                source14.Subscribe(observer14);
                source15.Subscribe(observer15);
            }
            catch
            {
                Dispose();
                throw;
            }
            return this;
        }

        public void TryPublishOnNext()
        {
            if (observer1.HasValue && observer2.HasValue && observer3.HasValue && observer4.HasValue && observer5.HasValue && observer6.HasValue && observer7.HasValue && observer8.HasValue && observer9.HasValue && observer10.HasValue && observer11.HasValue && observer12.HasValue && observer13.HasValue && observer14.HasValue && observer15.HasValue)
            {
                var result = resultSelector(observer1.GetValue(), observer2.GetValue(), observer3.GetValue(), observer4.GetValue(), observer5.GetValue(), observer6.GetValue(), observer7.GetValue(), observer8.GetValue(), observer9.GetValue(), observer10.GetValue(), observer11.GetValue(), observer12.GetValue(), observer13.GetValue(), observer14.GetValue(), observer15.GetValue());
                observer.OnNext(result);
                
                if (observer1.IsCompleted || observer2.IsCompleted || observer3.IsCompleted || observer4.IsCompleted || observer5.IsCompleted || observer6.IsCompleted || observer7.IsCompleted || observer8.IsCompleted || observer9.IsCompleted || observer10.IsCompleted || observer11.IsCompleted || observer12.IsCompleted || observer13.IsCompleted || observer14.IsCompleted || observer15.IsCompleted)
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void TryPublishOnCompleted(Result result, bool empty)
        {
            if (result.IsFailure)
            {
                observer.OnCompleted(result);
                Dispose();
            }
            else
            {
                if (empty || (observer1.IsCompleted && observer2.IsCompleted && observer3.IsCompleted && observer4.IsCompleted && observer5.IsCompleted && observer6.IsCompleted && observer7.IsCompleted && observer8.IsCompleted && observer9.IsCompleted && observer10.IsCompleted && observer11.IsCompleted && observer12.IsCompleted && observer13.IsCompleted && observer14.IsCompleted && observer15.IsCompleted))
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void Dispose()
        {
            observer1.Dispose();
            observer2.Dispose();
            observer3.Dispose();
            observer4.Dispose();
            observer5.Dispose();
            observer6.Dispose();
            observer7.Dispose();
            observer8.Dispose();
            observer9.Dispose();
            observer10.Dispose();
            observer11.Dispose();
            observer12.Dispose();
            observer13.Dispose();
            observer14.Dispose();
            observer15.Dispose();
        }

        sealed class ZipLatestObserver<T>(_ZipLatest parent) : Observer<T>
        {
            T? value;
            public bool HasValue { get; private set; }
            public bool IsCompleted { get; private set; }

            public T GetValue()
            {
                var v = this.value;
                this.value = default;
                this.HasValue = false;
                return v!;
            }

            protected override void OnNextCore(T value)
            {
                lock (parent.gate)
                {
                    this.value = value;
                    this.HasValue = true;
                    parent.TryPublishOnNext();
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                parent.observer.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                lock (parent.gate)
                {
                    IsCompleted = true;
                    parent.TryPublishOnCompleted(result, !HasValue);
                }
            }
        }
    }
}

