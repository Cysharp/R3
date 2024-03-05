using System;

namespace R3;

public static class ObservableSystem
{
    static IServiceProvider? serviceProvider;
    static Func<IServiceProvider>? serviceProviderFactory;

    static TimeProvider defaultTimeProvider = TimeProvider.System;
    static FrameProvider defaultFrameProvider = new NotSupportedFrameProvider();
    static Action<Exception> unhandledException = DefaultUnhandledExceptionHandler;

    public static TimeProvider DefaultTimeProvider
    {
        get
        {
            var services = serviceProvider;
            if (serviceProviderFactory != null)
            {
                services = serviceProviderFactory();
            }

            if (services != null)
            {
                var provider = services.GetService(typeof(TimeProvider));
                if (provider != null)
                {
                    return (TimeProvider)provider;
                }
            }
            return defaultTimeProvider;
        }
        set
        {
            defaultTimeProvider = value;
        }
    }

    public static FrameProvider DefaultFrameProvider
    {
        get
        {
            var services = serviceProvider;
            if (serviceProviderFactory != null)
            {
                services = serviceProviderFactory();
            }

            if (services != null)
            {
                var provider = services.GetService(typeof(FrameProvider));
                if (provider != null)
                {
                    return (FrameProvider)provider;
                }
            }
            return defaultFrameProvider;
        }
        set
        {
            defaultFrameProvider = value;
        }
    }

    public static void RegisterServiceProvider(IServiceProvider? serviceProvider)
    {
        ObservableSystem.serviceProvider = serviceProvider;
        ObservableSystem.serviceProviderFactory = null;
    }

    public static void RegisterServiceProvider(Func<IServiceProvider> serviceProviderFactory)
    {
        ObservableSystem.serviceProvider = null;
        ObservableSystem.serviceProviderFactory = serviceProviderFactory;
    }

    // Prevent +=, use Set and Get method.
    public static void RegisterUnhandledExceptionHandler(Action<Exception> unhandledExceptionHandler)
    {
        unhandledException = unhandledExceptionHandler;
    }

    public static Action<Exception> GetUnhandledExceptionHandler()
    {
        return unhandledException;
    }

    static void DefaultUnhandledExceptionHandler(Exception exception)
    {
        Console.WriteLine("R3 UnhandleException: " + exception.ToString());
    }
}

internal sealed class NotSupportedFrameProvider : FrameProvider
{
    public override long GetFrameCount()
    {
        throw new NotSupportedException("ObservableSystem.DefaultFrameProvider is not set. Please set ObservableSystem.DefaultFrameProvider to a valid FrameProvider(ThreadSleepFrameProvider, etc...).");
    }

    public override void Register(IFrameRunnerWorkItem callback)
    {
        throw new NotSupportedException("ObservableSystem.DefaultFrameProvider is not set. Please set ObservableSystem.DefaultFrameProvider to a valid FrameProvider(ThreadSleepFrameProvider, etc...).");
    }
}
