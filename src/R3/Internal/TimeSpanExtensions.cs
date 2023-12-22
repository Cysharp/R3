namespace R3.Internal;

internal static class TimeSpanExtensions
{
    public static TimeSpan Normalize(this TimeSpan timeSpan)
    {
        return timeSpan >= TimeSpan.Zero ? timeSpan : TimeSpan.Zero;
    }
}
