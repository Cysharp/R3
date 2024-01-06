namespace R3;

public class ObservableSystem
{
    public static TimeProvider DefaultTimeProvider { get; set; } = TimeProvider.System;
    public static FrameProvider DefaultFrameProvider { get; set; } = new NotSupportedFrameProvider();

    static Action<Exception> unhandledException = DefaultUnhandledExceptionHandler;

    // Prevent +=, use Set and Get method.
    public static void RegisterUnhandledExceptionHandler(Action<Exception> unhandledExceptionHandler)
    {
        unhandledException = unhandledExceptionHandler;
    }

    public static Action<Exception> GetUnhandledExceptionHandler()
    {
        return unhandledException;
    }

    static void DefaultUnhandledExceptionHandler(Exception exception)
    {
        Console.WriteLine("R3 UnhandleException: " + exception.ToString());
    }
}

internal sealed class NotSupportedFrameProvider : FrameProvider
{
    public override long GetFrameCount()
    {
        throw new NotSupportedException("ObservableSystem.DefaultFrameProvider is not set. Please set ObservableSystem.DefaultFrameProvider to a valid FrameProvider(ThreadSleepFrameProvider, etc...).");
    }

    public override void Register(IFrameRunnerWorkItem callback)
    {
        throw new NotSupportedException("ObservableSystem.DefaultFrameProvider is not set. Please set ObservableSystem.DefaultFrameProvider to a valid FrameProvider(ThreadSleepFrameProvider, etc...).");
    }
}
