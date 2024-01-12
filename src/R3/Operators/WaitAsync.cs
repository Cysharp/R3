namespace R3;

public static partial class ObservableExtensions
{
    public static Task WaitAsync<T>(this Observable<T> source, CancellationToken cancellationToken = default)
    {
        var method = new WaitAsync<T>(cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }
}

internal sealed class WaitAsync<T>(CancellationToken cancellationToken) : TaskObserverBase<T, Unit>(cancellationToken)
{
    protected override void OnNextCore(T value)
    {
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        TrySetException(error);
    }

    protected override void OnCompletedCore(Result result)
    {
        if (result.IsFailure)
        {
            TrySetException(result.Exception);
            return;
        }
        TrySetResult(Unit.Default);
    }
}
