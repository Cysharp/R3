using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

namespace R3.Maui;

public static class MauiAppBuilderExtensions
{
    public static MauiAppBuilder UseR3(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<IMauiInitializeService, R3MauiInitializeService>();
        builder.Services.AddSingleton<IR3MauiExceptionHandler, R3MauiDefaultExceptionHandler>();
        return builder;
    }

    public static MauiAppBuilder UseR3(this MauiAppBuilder builder, Action<Exception> unhandledExceptionHandler)
    {
        builder.UseR3();
        builder.Services.AddSingleton<IR3MauiExceptionHandler>(new R3MauiAnonymousExceptionHandler(unhandledExceptionHandler));
        return builder;
    }
}
