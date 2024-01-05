using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace R3.Internal;

internal static class ThrowHelper
{
    internal static void ThrowArgumentNullIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null)
        {
            ThrowArgumentNullException(paramName);
        }
    }

    internal static void ThrowObjectDisposedIf([DoesNotReturnIf(true)] bool condition, Type type)
    {
        if (condition)
        {
            ThrowObjectDisposedException(type);
        }
    }

    [DoesNotReturn]
    internal static void ThrowArgumentNullException(string? paramName) => throw new ArgumentNullException(paramName);

    [DoesNotReturn]
    internal static void ThrowObjectDisposedException(Type? type) => throw new ObjectDisposedException(type?.FullName);
}
