using System;

namespace R3;

public static class GodotObservableExtensions
{
    public static Observable<double> Delta(this Observable<Unit> source)
    {
        return Delta(source, GodotFrameProvider.Process);
    }

    public static Observable<double> Delta(this Observable<Unit> source, GodotFrameProvider frameProvider)
    {
        return new Delta(source, frameProvider);
    }

    public static Observable<(double Delta, T Item)> Delta<T>(this Observable<T> source)
    {
        return Delta(source, GodotFrameProvider.Process);
    }

    public static Observable<(double Delta, T Item)> Delta<T>(this Observable<T> source, GodotFrameProvider frameProvider)
    {
        return new Delta<T>(source, frameProvider);
    }
}

internal sealed class Delta(Observable<Unit> source, GodotFrameProvider frameProvider) : Observable<double>
{
    protected override IDisposable SubscribeCore(Observer<double> observer)
    {
        return source.Subscribe(new _Delta(observer, frameProvider));
    }

    sealed class _Delta(Observer<double> observer, GodotFrameProvider frameProvider) : Observer<Unit>
    {
        protected override void OnNextCore(Unit value)
        {
            observer.OnNext(frameProvider.Delta.Value);
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

internal sealed class Delta<T>(Observable<T> source, GodotFrameProvider frameProvider) : Observable<(double Delta, T Item)>
{
    protected override IDisposable SubscribeCore(Observer<(double Delta, T Item)> observer)
    {
        return source.Subscribe(new _Delta(observer, frameProvider));
    }

    sealed class _Delta(Observer<(double Delta, T Item)> observer, GodotFrameProvider frameProvider) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            observer.OnNext((frameProvider.Delta.Value, value));
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
