using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace R3.Maui;

public interface IR3MauiExceptionHandler
{
    void HandleException(Exception ex);
}

public class R3MauiDefaultExceptionHandler(IServiceProvider serviceProvider) : IR3MauiExceptionHandler
{
    public void HandleException(Exception ex)
    {
        System.Diagnostics.Trace.TraceError("R3 Unhandled Exception {0}", ex);

        var logger = serviceProvider.GetService<ILogger<R3MauiDefaultExceptionHandler>>();
        logger?.LogError(ex, "R3 Unhandled Exception");
    }
}

public class R3MauiAnonymousExceptionHandler(Action<Exception> handler) : IR3MauiExceptionHandler
{
    public void HandleException(Exception ex)
    {
        handler(ex);
    }
}
