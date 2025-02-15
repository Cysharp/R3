using System;
using System.ComponentModel;

namespace R3.WinForms;

public static class WinFormsProviderInitializer
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
        ObservableSystem.DefaultTimeProvider = (marshalingControl == null) ? WinFormsTimeProvider.Default : new WinFormsTimeProvider(marshalingControl);
        ObservableSystem.DefaultFrameProvider = (isStepFrame == null) ? WinFormsFrameProvider.Default : new WinFormsFrameProvider(isStepFrame);
    }
}
