namespace R3.Internal;

internal static class FrameCountExtensions
{
    // 0 is invalid, 1 is valid.
    public static int Normalize(this int frameCount)
    {
        return frameCount > 0 ? frameCount : 1;
    }
}
