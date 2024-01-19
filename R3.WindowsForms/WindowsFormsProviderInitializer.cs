using System;
using System.ComponentModel;

namespace R3.WindowsForms;

public static class WindowsFormsProviderInitializer
{
    public static void SetDefaultObservableSystem(
        Action<Exception> unhandledExceptionHandler)
    {
        SetDefaultObservableSystem(unhandledExceptionHandler, null);
    }

    public static void SetDefaultObservableSystem(
        Action<Exception> unhandledExceptionHandler,
        ISynchronizeInvoke? marshalingControl)
    {
        SetDefaultObservableSystem(unhandledExceptionHandler, marshalingControl, null);
    }

    public static void SetDefaultObservableSystem(
        Action<Exception> unhandledExceptionHandler,
        ISynchronizeInvoke? marshalingControl,
        MessageFilter? isStepFrame)
    {
        ObservableSystem.RegisterUnhandledExceptionHandler(unhandledExceptionHandler);
        ObservableSystem.DefaultFrameProvider = new WindowsFormsFrameProvider(isStepFrame);
        ObservableSystem.DefaultTimeProvider = new WindowsFormsTimerProvider(marshalingControl);
    }
}
