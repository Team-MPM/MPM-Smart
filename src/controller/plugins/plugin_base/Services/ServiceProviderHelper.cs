using Microsoft.Extensions.DependencyInjection;

namespace PluginBase.Services;

public static class ServiceProviderHelper
{
    public static IServiceProvider? ServiceProvider { get; private set; }

    public static void Configure(IServiceProvider serviceProvider) => ServiceProvider = serviceProvider;

    public static T GetService<T>() where T : notnull
    {
        if (ServiceProvider == null)
            throw new InvalidOperationException("ServiceProvider is not configured yet.");

        return ServiceProvider.GetRequiredService<T>();
    }

    public static object GetService(Type serviceType)
    {
        if (ServiceProvider == null)
            throw new InvalidOperationException("ServiceProvider is not configured yet.");

        return ServiceProvider.GetRequiredService(serviceType);
    }
}