namespace R3.Internal;

internal static class Stubs
{
    internal static readonly Action<Result> HandleError = static x =>
    {
        if (x.IsFailure)
        {
            EventSystem.GetUnhandledExceptionHandler().Invoke(x.Exception);
        }
    };
}

internal static class Stubs<T>
{
    internal static readonly Func<T, T> ReturnSelf = static x => x;
}
