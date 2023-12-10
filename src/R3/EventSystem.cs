using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using static Microsoft.Extensions.Logging.LogLevel;

namespace R3;

public class EventSystem
{
    public static ILogger<EventSystem> Logger { get; set; } = NullLogger<EventSystem>.Instance;

    EventSystem()
    {
    }
}



internal static partial class SystemLoggerExtensions
{
    [LoggerMessage(Trace, "Add subscription tracking TrackingId: {TrackingId}.")]
    public static partial void AddTracking(this ILogger<EventSystem> logger, int trackingId);

    [LoggerMessage(Trace, "Remove subscription TrackingId: {TrackingId}.")]
    public static partial void RemoveTracking(this ILogger<EventSystem> logger, int trackingId);

}
