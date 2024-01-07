#if NETSTANDARD2_0 || NETSTANDARD2_1

namespace System.Threading;

internal static class CancellationTokenExtensions
{
    public static CancellationTokenRegistration UnsafeRegister(this CancellationToken cancellationToken, Action<object?> callback, object? state)
    {
        return cancellationToken.Register(callback, state, useSynchronizationContext: false);
    }
}

#endif
