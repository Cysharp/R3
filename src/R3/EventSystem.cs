using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using static Microsoft.Extensions.Logging.LogLevel;

namespace R3;

public class EventSystem
{
    public static ILogger<EventSystem> Logger { get; set; } = NullLogger<EventSystem>.Instance;

    static Action<Exception> unhandledException = WriteLog;

    // Prevent +=, use Set and Get method.
    public static void SetUnhandledExceptionHandler(Action<Exception> unhandledExceptionHandler)
    {
        unhandledException = unhandledExceptionHandler;
    }

    public static Action<Exception> GetUnhandledExceptionHandler()
    {
        return unhandledException;
    }

    static void WriteLog(Exception exception)
    {
        if (Logger == NullLogger<EventSystem>.Instance)
        {
            Console.WriteLine("R3 UnhandleException: " + exception.ToString());
        }
        else
        {
            Logger.UnhandledException(exception);
        }
    }
}

internal static partial class SystemLoggerExtensions
{
    [LoggerMessage(Trace, "Add subscription tracking TrackingId: {TrackingId}.")]
    public static partial void AddTracking(this ILogger<EventSystem> logger, int trackingId);

    [LoggerMessage(Trace, "Remove subscription TrackingId: {TrackingId}.")]
    public static partial void RemoveTracking(this ILogger<EventSystem> logger, int trackingId);

    [LoggerMessage(Error, "R3 EventSystem received unhandled exception.")]
    public static partial void UnhandledException(this ILogger<EventSystem> logger, Exception exception);

}
