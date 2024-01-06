using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using static Microsoft.Extensions.Logging.LogLevel;

namespace R3;

public class ObservableSystem
{
    public static ILogger<ObservableSystem> Logger { get; set; } = NullLogger<ObservableSystem>.Instance;

    public static TimeProvider DefaultTimeProvider { get; set; } = TimeProvider.System;
    public static FrameProvider DefaultFrameProvider { get; set; } = new NotSupportedFrameProvider();

    static Action<Exception> unhandledException = WriteLog;

    // Prevent +=, use Set and Get method.
    public static void RegisterUnhandledExceptionHandler(Action<Exception> unhandledExceptionHandler)
    {
        unhandledException = unhandledExceptionHandler;
    }

    public static Action<Exception> GetUnhandledExceptionHandler()
    {
        return unhandledException;
    }

    static void WriteLog(Exception exception)
    {
        if (Logger == NullLogger<ObservableSystem>.Instance)
        {
            Console.WriteLine("R3 UnhandleException: " + exception.ToString());
        }
        else
        {
            Logger.UnhandledException(exception);
        }
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

internal static partial class SystemLoggerExtensions
{
    [LoggerMessage(Trace, "Add subscription tracking TrackingId: {TrackingId}.")]
    public static partial void AddTracking(this ILogger<ObservableSystem> logger, int trackingId);

    [LoggerMessage(Trace, "Remove subscription TrackingId: {TrackingId}.")]
    public static partial void RemoveTracking(this ILogger<ObservableSystem> logger, int trackingId);

    [LoggerMessage(Error, "R3 EventSystem received unhandled exception.")]
    public static partial void UnhandledException(this ILogger<ObservableSystem> logger, Exception exception);

}
