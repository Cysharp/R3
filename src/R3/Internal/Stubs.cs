namespace R3.Internal;

internal static class Stubs
{
    internal static readonly Action<Result> HandleResult = static x =>
    {
        if (x.IsFailure)
        {
            ObservableSystem.GetUnhandledExceptionHandler().Invoke(x.Exception);
        }
    };
}

internal static class Stubs<T>
{
    internal static readonly Func<T, T> ReturnSelf = static x => x;


    // TState

    internal static readonly Action<Exception, T> HandleException = static (x, _) =>
    {
        ObservableSystem.GetUnhandledExceptionHandler().Invoke(x);
    };


    internal static readonly Action<Result, T> HandleResult = static (x, _) =>
    {
        if (x.IsFailure)
        {
            ObservableSystem.GetUnhandledExceptionHandler().Invoke(x.Exception);
        }
    };
}
