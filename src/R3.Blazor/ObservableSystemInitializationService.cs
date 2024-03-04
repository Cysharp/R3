namespace R3;

public static class BlazorR3Extensions
{
    public static IServiceCollection AddBlazorR3(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<TimeProvider>(_ => new SynchronizationContextTimeProvider());
        services.AddHostedService<ObservableSystemInitializationService>();

        return services;
    }

    public static IServiceCollection AddBlazorR3(this IServiceCollection services, Func<IServiceProvider, TimeProvider> timeProviderFactory)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<TimeProvider>(timeProviderFactory);
        services.AddHostedService<ObservableSystemInitializationService>();

        return services;
    }
}

public sealed class ObservableSystemInitializationService(IHttpContextAccessor accessor) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        ObservableSystem.RegisterServiceProvider(() => accessor.HttpContext!.RequestServices);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
