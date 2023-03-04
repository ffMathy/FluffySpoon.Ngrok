using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;

namespace FluffySpoon.Ngrok;

public class NgrokOptions
{
    public bool ShowNgrokWindow { get; set; }
    public string? AuthToken { get; set; }
}

public static class RegistrationExtensions 
{
    public static void AddNgrokLifetimeHook(
        this IServiceCollection services, 
        INgrokLifetimeHook hook)
    {
        services.AddSingleton(hook);
    }
    
    public static void AddNgrokLifetimeHook<THook>(
        this IServiceCollection services) where THook : class, INgrokLifetimeHook
    {
        services.AddSingleton<INgrokLifetimeHook, THook>();
    }
    
    public static void AddNgrok(
        this IServiceCollection services,
        NgrokOptions? options = null)
    {
        services.AddLogging();

        services.AddTransient<INgrokDownloader, NgrokDownloader>();
        services.AddSingleton<INgrokApiClient, NgrokApiClient>();
        
        services.AddSingleton(options ?? new NgrokOptions());
        
        services.AddSingleton<INgrokProcess, NgrokProcess>();
        services.AddSingleton<INgrokService, NgrokService>();

        services.AddHttpClient<INgrokDownloader, NgrokDownloader>(httpClient =>
        {
            httpClient.BaseAddress = new Uri("https://bin.equinox.io");
        });

        services.AddHttpClient<INgrokApiClient, NgrokApiClient>(httpClient =>
        {
            httpClient.BaseAddress = new Uri("http://localhost:4040/api/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });
    }
}