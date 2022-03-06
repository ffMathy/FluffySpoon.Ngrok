using System.Net.Http.Headers;
using FluffySpoon.AspNet.Ngrok.New.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FluffySpoon.AspNet.Ngrok.New;

public interface INgrokLifetimeHook
{
    Task OnCreatedAsync(Tunnel tunnel, CancellationToken cancellationToken);
    Task OnDestroyedAsync(Tunnel tunnel, CancellationToken cancellationToken);
}

public static class AspNetCoreExtensions 
{
    public static void AddNgrokLifetimeHook(
        this IServiceCollection services, 
        INgrokLifetimeHook hook)
    {
        services.AddSingleton(hook);
    }
    
    public static void AddNgrok(
        this IServiceCollection services)
    {
        services.AddLogging();

        services.AddHostedService<NgrokHostedService>();

        services.AddTransient<INgrokDownloader, NgrokDownloader>();
        services.AddTransient<INgrokApiClient, NgrokApiClient>();
        
        services.AddSingleton<INgrokProcess, NgrokProcess>();

        services.AddHttpClient<INgrokDownloader, NgrokDownloader>(httpClient =>
        {
            httpClient.BaseAddress = new Uri("https://bin.equinox.io");
        });

        services.AddHttpClient<INgrokApiClient, NgrokApiClient>(httpClient =>
        {
            httpClient.BaseAddress = new Uri("http://localhost:4040");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });
    }
}