using Avalonia.Logging;
using Avalonia.Threading;

namespace R3;

public static class AvaloniaProviderInitializer
{
    public static void SetDefaultObservableSystem()
    {
        SetDefaultObservableSystem(ex => Logger.Sink?.Log(LogEventLevel.Error, "R3", null, "R3 Unhandled Exception {0}", ex));
    }

    public static void SetDefaultObservableSystem(Action<Exception> unhandledExceptionHandler)
    {
        ObservableSystem.RegisterUnhandledExceptionHandler(unhandledExceptionHandler);
        ObservableSystem.DefaultTimeProvider = new AvaloniaDispatcherTimerProvider();
        ObservableSystem.DefaultFrameProvider = new AvaloniaDispatcherFrameProvider();
    }

    public static void SetDefaultObservableSystem(Action<Exception> unhandledExceptionHandler, DispatcherPriority priority)
    {
        ObservableSystem.RegisterUnhandledExceptionHandler(unhandledExceptionHandler);
        ObservableSystem.DefaultTimeProvider = new AvaloniaDispatcherTimerProvider(priority);
        ObservableSystem.DefaultFrameProvider = new AvaloniaDispatcherFrameProvider(priority);
    }

    public static void SetDefaultObservableSystem(Action<Exception> unhandledExceptionHandler, int framesPerSecond)
    {
        ObservableSystem.RegisterUnhandledExceptionHandler(unhandledExceptionHandler);
        ObservableSystem.DefaultTimeProvider = new AvaloniaDispatcherTimerProvider();
        ObservableSystem.DefaultFrameProvider = new AvaloniaDispatcherFrameProvider(framesPerSecond);
    }

    public static void SetDefaultObservableSystem(Action<Exception> unhandledExceptionHandler, DispatcherPriority priority, int framesPerSecond)
    {
        ObservableSystem.RegisterUnhandledExceptionHandler(unhandledExceptionHandler);
        ObservableSystem.DefaultTimeProvider = new AvaloniaDispatcherTimerProvider(priority);
        ObservableSystem.DefaultFrameProvider = new AvaloniaDispatcherFrameProvider(framesPerSecond, priority);
    }
}
