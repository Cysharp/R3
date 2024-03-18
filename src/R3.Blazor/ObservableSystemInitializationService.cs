using Microsoft.Extensions.DependencyInjection.Extensions;

namespace R3;

public static class BlazorR3Extensions
{
    public static TimeSpan DefaultPeriod = TimeSpan.FromMilliseconds(16.7d);

    public static IServiceCollection AddBlazorR3(this IServiceCollection services, TimeSpan? dueTime = null, TimeSpan? period = null)
    {
        return AddBlazorR3(services, _ => new SynchronizationContextTimeProvider(), _ => new TimerFrameProvider(dueTime ?? TimeSpan.Zero, period ?? DefaultPeriod), null!);
    }

    public static IServiceCollection AddBlazorR3(this IServiceCollection services, Action<Exception> unhandledExceptionHandler, TimeSpan? dueTime = null,  TimeSpan? period = null)
    {
        return AddBlazorR3(services, _ => new SynchronizationContextTimeProvider(), _ => new TimerFrameProvider(dueTime ?? TimeSpan.Zero, period ?? DefaultPeriod), unhandledExceptionHandler);
    }

    public static IServiceCollection AddBlazorR3(this IServiceCollection services, Func<IServiceProvider, TimeProvider> timeProviderFactory, Func<IServiceProvider, FrameProvider> frameProviderFactory)
    {
        return AddBlazorR3(services, timeProviderFactory, frameProviderFactory, null!);
    }

    public static IServiceCollection AddBlazorR3(this IServiceCollection services, Func<IServiceProvider, TimeProvider> timeProviderFactory, Func<IServiceProvider, FrameProvider> frameProviderFactory, Action<Exception> unhandledExceptionHandler)
    {
        services.AddHttpContextAccessor();
        services.TryAddScoped<TimeProvider>(timeProviderFactory);
        services.TryAddScoped<FrameProvider>(frameProviderFactory);
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
