using Avalonia.Threading;
using R3.Avalonia;

namespace Avalonia; // Avalonia namespace

public static class AppBuilderR3InitializeExtensions
{
    public static AppBuilder UseR3(this AppBuilder builder)
    {
        // need to delay setup, initialize provider(dispatcher) need to determine platform
        return builder.AfterSetup(_ => AvaloniaProviderInitializer.SetDefaultProviders());
    }

    public static AppBuilder UseR3(this AppBuilder builder, DispatcherPriority priority)
    {
        return builder.AfterSetup(_ => AvaloniaProviderInitializer.SetDefaultProviders(priority));
    }

    public static AppBuilder UseR3(this AppBuilder builder, int framesPerSecond)
    {
        return builder.AfterSetup(_ => AvaloniaProviderInitializer.SetDefaultProviders(framesPerSecond));
    }

    public static AppBuilder UseR3(this AppBuilder builder, DispatcherPriority priority, int framesPerSecond)
    {
        return builder.AfterSetup(_ => AvaloniaProviderInitializer.SetDefaultProviders(priority, framesPerSecond));
    }
}
