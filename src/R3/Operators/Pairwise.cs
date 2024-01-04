namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<(T Previous, T Current)> Pairwise<T>(this Observable<T> source)
    {
        return new Pairwise<T>(source);
    }
}

internal sealed class Pairwise<T>(Observable<T> source) : Observable<(T Previous, T Current)>
{
    protected override IDisposable SubscribeCore(Observer<(T Previous, T Current)> observer)
    {
        return source.Subscribe(new _Pairwise(observer));
    }

    sealed class _Pairwise(Observer<(T Previous, T Current)> observer) : Observer<T>
    {
        T? previous;
        bool isFirst = true;

        protected override void OnNextCore(T value)
        {
            if (isFirst)
            {
                isFirst = false;
                previous = value;
                return;
            }

            var pair = (previous!, value);
            previous = value;
            observer.OnNext(pair);
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
