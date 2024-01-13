using System.Net.Http.Headers;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FluffySpoon.Ngrok;

public class NgrokOptions
{
    public bool ShowNgrokWindow { get; set; }
    public string AuthToken { get; set; }
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
    
    public static void AddNgrok(this IServiceCollection services, IConfiguration configuration)
    {
        AddNgrokInternal(services);
        services.Configure<NgrokOptions>(configuration);
    }
    
    public static void AddNgrok(this IServiceCollection services, Action<NgrokOptions> configureOptions)
    {
        var optionsBuilder = AddNgrokInternal(services);
        optionsBuilder.Configure(configureOptions);
    }
    
    public static void AddNgrok(this IServiceCollection services)
    {
        AddNgrokInternal(services);
    }

    private static OptionsBuilder<NgrokOptions> AddNgrokInternal(IServiceCollection services)
    {
        services.AddLogging();

        services.AddTransient<INgrokDownloader, NgrokDownloader>();
        services.AddSingleton<INgrokApiClient, NgrokApiClient>();

        var optionsBuilder = services.AddOptions<NgrokOptions>();

        services.AddSingleton<INgrokProcess, NgrokProcess>();
        services.AddSingleton<INgrokService, NgrokService>();

        services.AddSingleton<IFlurlClientCache>(sp => new FlurlClientCache());

        services.AddHttpClient<INgrokDownloader, NgrokDownloader>(httpClient =>
        {
            httpClient.BaseAddress = new Uri("https://bin.equinox.io");
        });
        
        return optionsBuilder;
    }
}