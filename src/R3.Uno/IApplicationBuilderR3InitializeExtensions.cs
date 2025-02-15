using R3;
using Uno.Extensions.Hosting;

namespace Uno; // Uno namespace

public static class IApplicationBuilderR3InitializeExtensions
{
    public static IApplicationBuilder UseR3(this IApplicationBuilder builder)
    {
        UnoProviderInitializer.SetDefaultObservableSystem();
        return builder;
    }

    public static IApplicationBuilder UseR3(this IApplicationBuilder builder, Action<Exception> unhandledExceptionHandler)
    {
        UnoProviderInitializer.SetDefaultObservableSystem(unhandledExceptionHandler);
        return builder;
    }
}
