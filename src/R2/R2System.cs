using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using static Microsoft.Extensions.Logging.LogLevel;

namespace R2;

public class R2System
{
    public static ILogger<R2System> Logger { get; set; } = NullLogger<R2System>.Instance;

    R2System()
    {
    }
}



internal static partial class SystemLoggerExtensions
{
    [LoggerMessage(Trace, "Add subscription tracking TrackingId: {TrackingId}.")]
    public static partial void AddTracking(this ILogger<R2System> logger, int trackingId);

    [LoggerMessage(Trace, "Remove subscription TrackingId: {TrackingId}.")]
    public static partial void RemoveTracking(this ILogger<R2System> logger, int trackingId);

}
