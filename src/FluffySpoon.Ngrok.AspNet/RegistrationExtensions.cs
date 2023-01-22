using Microsoft.Extensions.DependencyInjection;

namespace FluffySpoon.Ngrok.AspNet;

public static class RegistrationExtensions 
{
    public static void AddNgrokHostedService(
        this IServiceCollection services,
        NgrokOptions? options = null)
    {
        services.AddNgrok(options);

        services.AddSingleton<INgrokHostedService, NgrokHostedService>();
        services.AddHostedService(x => x.GetRequiredService<INgrokHostedService>());
    }
}