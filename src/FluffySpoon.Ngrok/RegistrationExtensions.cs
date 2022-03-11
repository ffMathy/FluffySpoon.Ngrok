using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;

namespace FluffySpoon.Ngrok;

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

        services.AddTransient<INgrokDownloader, NgrokDownloader>();
        services.AddTransient<INgrokApiClient, NgrokApiClient>();
        
        services.AddSingleton<INgrokProcess, NgrokProcess>();
        services.AddSingleton<INgrokService, NgrokService>();

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