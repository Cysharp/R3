namespace R3.Internal;

internal static class Stubs<T>
{
    internal static readonly Func<T, T> ReturnSelf = static x => x;
    internal static readonly Action<T> Nop = static _ => { };
}
