using System.Windows;
using System.Windows.Threading;

namespace R3;

public static class WpfProviderInitializer
{
    public static void SetDefaultObservableSystem(Action<Exception> unhandledExceptionHandler)
    {
        ObservableSystem.RegisterUnhandledExceptionHandler(unhandledExceptionHandler);
        ObservableSystem.DefaultTimeProvider = new WpfDispatcherTimerProvider();
        ObservableSystem.DefaultFrameProvider = new WpfRenderingFrameProvider();
    }

    public static void SetDefaultObservableSystem(Action<Exception> unhandledExceptionHandler, DispatcherPriority priority)
    {
        ObservableSystem.RegisterUnhandledExceptionHandler(unhandledExceptionHandler);
        ObservableSystem.DefaultTimeProvider = new WpfDispatcherTimerProvider(priority);
        ObservableSystem.DefaultFrameProvider = new WpfRenderingFrameProvider();
    }

    public static void SetDefaultObservableSystem(Action<Exception> unhandledExceptionHandler, DispatcherPriority priority, Dispatcher dispatcher)
    {
        ObservableSystem.RegisterUnhandledExceptionHandler(unhandledExceptionHandler);
        ObservableSystem.DefaultTimeProvider = new WpfDispatcherTimerProvider(priority, dispatcher);
        ObservableSystem.DefaultFrameProvider = new WpfRenderingFrameProvider();
    }
}
