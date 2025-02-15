using System.Windows;
using System.Windows.Threading;

namespace R3;

public static class WpfProviderInitializer
{
    public static void SetDefaultObservableSystem(Action<Exception> unhandledExceptionHandler)
    {
        ObservableSystem.RegisterUnhandledExceptionHandler(unhandledExceptionHandler);
        ObservableSystem.DefaultTimeProvider = WpfDispatcherTimeProvider.Default;
        ObservableSystem.DefaultFrameProvider = WpfRenderingFrameProvider.Default;
    }

    public static void SetDefaultObservableSystem(Action<Exception> unhandledExceptionHandler, DispatcherPriority priority)
    {
        ObservableSystem.RegisterUnhandledExceptionHandler(unhandledExceptionHandler);
        ObservableSystem.DefaultTimeProvider = new WpfDispatcherTimeProvider(priority);
        ObservableSystem.DefaultFrameProvider = WpfRenderingFrameProvider.Default;
    }

    public static void SetDefaultObservableSystem(Action<Exception> unhandledExceptionHandler, DispatcherPriority priority, Dispatcher dispatcher)
    {
        ObservableSystem.RegisterUnhandledExceptionHandler(unhandledExceptionHandler);
        ObservableSystem.DefaultTimeProvider = new WpfDispatcherTimeProvider(priority, dispatcher);
        ObservableSystem.DefaultFrameProvider = WpfRenderingFrameProvider.Default;
    }
}
