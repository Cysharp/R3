using Microsoft.Extensions.DependencyInjection.Extensions;

namespace R3;

public static class BlazorR3Extensions
{
    public static IServiceCollection AddBlazorR3(this IServiceCollection services)
    {
        return AddBlazorR3(services, _ => new SynchronizationContextTimeProvider(() => SynchronizationContext.Current), null!);
    }

    public static IServiceCollection AddBlazorR3(this IServiceCollection services, Action<Exception> unhandledExceptionHandler)
    {
        return AddBlazorR3(services, _ => new SynchronizationContextTimeProvider(() => SynchronizationContext.Current), unhandledExceptionHandler);
    }

    public static IServiceCollection AddBlazorR3(this IServiceCollection services, Func<IServiceProvider, TimeProvider> timeProviderFactory)
    {
        return AddBlazorR3(services, timeProviderFactory, null!);
    }

    public static IServiceCollection AddBlazorR3(this IServiceCollection services, Func<IServiceProvider, TimeProvider> timeProviderFactory, Action<Exception> unhandledExceptionHandler)
    {
        services.AddHttpContextAccessor();
        services.TryAddSingleton<TimeProvider>(timeProviderFactory);
        services.AddHostedService(sp => new ObservableSystemInitializationService(sp.GetRequiredService<IHttpContextAccessor>(), unhandledExceptionHandler));

        return services;
    }
}

public sealed class ObservableSystemInitializationService(IHttpContextAccessor accessor, Action<Exception>? unhandledExceptionHandler) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        ObservableSystem.RegisterServiceProvider(() => accessor.HttpContext!.RequestServices);
        if (unhandledExceptionHandler != null)
        {
            ObservableSystem.RegisterUnhandledExceptionHandler(unhandledExceptionHandler);
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
