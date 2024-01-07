using Avalonia.Threading;
using R3;

namespace Avalonia; // Avalonia namespace

public static class AppBuilderR3InitializeExtensions
{
    public static AppBuilder UseR3(this AppBuilder builder)
    {
        // need to delay setup, initialize provider(dispatcher) need to determine platform
        return builder.AfterSetup(_ => AvaloniaProviderInitializer.SetDefaultObservableSystem());
    }

    public static AppBuilder UseR3(this AppBuilder builder, Action<Exception> unhandledExceptionHandler)
    {
        return builder.AfterSetup(_ => AvaloniaProviderInitializer.SetDefaultObservableSystem(unhandledExceptionHandler));
    }

    public static AppBuilder UseR3(this AppBuilder builder, DispatcherPriority priority, Action<Exception> unhandledExceptionHandler)
    {
        return builder.AfterSetup(_ => AvaloniaProviderInitializer.SetDefaultObservableSystem(unhandledExceptionHandler, priority));
    }

    public static AppBuilder UseR3(this AppBuilder builder, int framesPerSecond, Action<Exception> unhandledExceptionHandler)
    {
        return builder.AfterSetup(_ => AvaloniaProviderInitializer.SetDefaultObservableSystem(unhandledExceptionHandler, framesPerSecond));
    }

    public static AppBuilder UseR3(this AppBuilder builder, DispatcherPriority priority, int framesPerSecond, Action<Exception> unhandledExceptionHandler)
    {
        return builder.AfterSetup(_ => AvaloniaProviderInitializer.SetDefaultObservableSystem(unhandledExceptionHandler, priority, framesPerSecond));
    }
}
