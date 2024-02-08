using Microsoft.Maui.Animations;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Hosting;
using R3.Maui;

namespace R3;

public class R3MauiInitializeService(
    IDispatcher dispatcher,
    ITicker ticker,
    IR3MauiExceptionHandler exceptionHandler) : IMauiInitializeService
{
    public void Initialize(IServiceProvider services)
    {
        ObservableSystem.RegisterUnhandledExceptionHandler(exceptionHandler.HandleException);
        ObservableSystem.DefaultTimeProvider = new MauiDispatcherTimerProvider(dispatcher);
        ObservableSystem.DefaultFrameProvider = new MauiTickerFrameProvider(ticker);
    }
}
