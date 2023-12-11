using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Runtime.ExceptionServices;
using static Microsoft.Extensions.Logging.LogLevel;

namespace R3;

public class EventSystem
{
    public static ILogger<EventSystem> Logger { get; set; } = NullLogger<EventSystem>.Instance;

    static Action<Exception> unhandledException = Throw;

    // Prevent +=, use Set and Get method.
    public static void SetUnhandledExceptionHandler(Action<Exception> unhandledExceptionHandler)
    {
        unhandledException = unhandledExceptionHandler;
    }

    public static Action<Exception> GetUnhandledExceptionHandler()
    {
        return unhandledException;
    }

    static void Throw(Exception exception)
    {
        ExceptionDispatchInfo.Capture(exception).Throw();
    }
}



internal static partial class SystemLoggerExtensions
{
    [LoggerMessage(Trace, "Add subscription tracking TrackingId: {TrackingId}.")]
    public static partial void AddTracking(this ILogger<EventSystem> logger, int trackingId);

    [LoggerMessage(Trace, "Remove subscription TrackingId: {TrackingId}.")]
    public static partial void RemoveTracking(this ILogger<EventSystem> logger, int trackingId);

}
