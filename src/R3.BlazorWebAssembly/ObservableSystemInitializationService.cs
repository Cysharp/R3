using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace R3;

public static class BlazorWebAssemblyR3Extensions
{
    public static IServiceCollection AddBlazorWebAssemblyR3(this IServiceCollection services)
    {
        return AddBlazorWebAssemblyR3(services, _ => new SynchronizationContextTimeProvider(() => SynchronizationContext.Current), null!);
    }

    public static IServiceCollection AddBlazorWebAssemblyR3(this IServiceCollection services, Action<Exception> unhandledExceptionHandler)
    {
        return AddBlazorWebAssemblyR3(services, _ => new SynchronizationContextTimeProvider(() => SynchronizationContext.Current), unhandledExceptionHandler);
    }

    public static IServiceCollection AddBlazorWebAssemblyR3(this IServiceCollection services, Func<IServiceProvider, TimeProvider> timeProviderFactory)
    {
        return AddBlazorWebAssemblyR3(services, timeProviderFactory, null!);
    }

    public static IServiceCollection AddBlazorWebAssemblyR3(this IServiceCollection services, Func<IServiceProvider, TimeProvider> timeProviderFactory, Action<Exception> unhandledExceptionHandler)
    {
        services.TryAddSingleton<TimeProvider>(timeProviderFactory);
        services.AddHostedService(sp => new ObservableSystemInitializationService(sp.GetRequiredService<IServiceProvider>(), unhandledExceptionHandler));

        return services;
    }
}

public sealed class ObservableSystemInitializationService(IServiceProvider serviceProvider, Action<Exception>? unhandledExceptionHandler) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        ObservableSystem.RegisterServiceProvider(() => serviceProvider);
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
