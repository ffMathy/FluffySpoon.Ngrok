using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FluffySpoon.Ngrok.AspNet;

public static class RegistrationExtensions
{
    private static void AddHostedServiceInternal(IServiceCollection services)
    {
        services.AddSingleton<INgrokHostedService, NgrokHostedService>();
        services.AddHostedService(x => x.GetRequiredService<INgrokHostedService>());
    }

    public static void AddNgrokHostedService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddNgrok(configuration);
        AddHostedServiceInternal(services);
    }

    public static void AddNgrokHostedService(this IServiceCollection services, Action<NgrokOptions> configureOptions)
    {
        services.AddNgrok(configureOptions);
        AddHostedServiceInternal(services);
    }

    public static void AddNgrokHostedService(this IServiceCollection services)
    {
        services.AddNgrok();
        AddHostedServiceInternal(services);
    }
}