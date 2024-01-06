using Avalonia.Threading;

namespace R3.Avalonia;

public static class AvaloniaProviderInitializer
{
    public static void SetDefaultProviders()
    {
        ObservableSystem.DefaultTimeProvider = new AvaloniaDispatcherTimerProvider();
        ObservableSystem.DefaultFrameProvider = new AvaloniaDispatcherFrameProvider();
    }

    public static void SetDefaultProviders(DispatcherPriority priority)
    {
        ObservableSystem.DefaultTimeProvider = new AvaloniaDispatcherTimerProvider(priority);
        ObservableSystem.DefaultFrameProvider = new AvaloniaDispatcherFrameProvider(priority);
    }

    public static void SetDefaultProviders(int framesPerSecond)
    {
        ObservableSystem.DefaultTimeProvider = new AvaloniaDispatcherTimerProvider();
        ObservableSystem.DefaultFrameProvider = new AvaloniaDispatcherFrameProvider(framesPerSecond);
    }

    public static void SetDefaultProviders(DispatcherPriority priority, int framesPerSecond)
    {
        ObservableSystem.DefaultTimeProvider = new AvaloniaDispatcherTimerProvider(priority);
        ObservableSystem.DefaultFrameProvider = new AvaloniaDispatcherFrameProvider(framesPerSecond, priority);
    }
}
