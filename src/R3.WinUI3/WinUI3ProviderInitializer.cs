using System;
using System.Windows;

namespace R3;

public static class WinUI3ProviderInitializer
{
    public static void SetDefaultObservableSystem(Action<Exception> unhandledExceptionHandler)
    {
        ObservableSystem.RegisterUnhandledExceptionHandler(unhandledExceptionHandler);
        ObservableSystem.DefaultTimeProvider = WinUI3DispatcherTimerProvider.Default;
        ObservableSystem.DefaultFrameProvider = WinUI3RenderingFrameProvider.Default;
    }
}
