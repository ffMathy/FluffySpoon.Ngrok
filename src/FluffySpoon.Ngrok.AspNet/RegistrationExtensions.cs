using FluffySpoon.Ngrok;
using Microsoft.Extensions.DependencyInjection;

namespace FluffySpoon.AspNet.Ngrok;

public static class AspNetCoreExtensions 
{
    public static void AddNgrokHostedService(
        this IServiceCollection services)
    {
        services.AddNgrok();

        services.AddSingleton<INgrokHostedService, NgrokHostedService>();
        services.AddHostedService(x => x.GetRequiredService<INgrokHostedService>());
    }
}