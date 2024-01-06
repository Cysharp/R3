using System.Windows.Threading;

namespace R3.WPF;

public static class WpfProviderInitializer
{
    public static void SetDefaultProviders()
    {
        ObservableSystem.DefaultTimeProvider = new WpfDispatcherTimerProvider();
        ObservableSystem.DefaultFrameProvider = new WpfRenderingFrameProvider();
    }

    public static void SetDefaultProviders(DispatcherPriority priority)
    {
        ObservableSystem.DefaultTimeProvider = new WpfDispatcherTimerProvider(priority);
        ObservableSystem.DefaultFrameProvider = new WpfRenderingFrameProvider();
    }

    public static void SetDefaultProviders(DispatcherPriority priority, Dispatcher dispatcher)
    {
        ObservableSystem.DefaultTimeProvider = new WpfDispatcherTimerProvider(priority, dispatcher);
        ObservableSystem.DefaultFrameProvider = new WpfRenderingFrameProvider();
    }
}
