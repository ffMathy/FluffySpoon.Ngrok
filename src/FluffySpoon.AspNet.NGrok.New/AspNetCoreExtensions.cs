using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;

namespace FluffySpoon.AspNet.Ngrok.New;

public static class AspNetCoreExtensions 
{
    public static IServiceCollection AddNgrok(this IServiceCollection services)
    {
        services.AddHostedService<NgrokHostedService>();

        services.AddHttpClient<NgrokDownloader>(x => 
            x.BaseAddress = new Uri("https://bin.equinox.io"));

        services.AddHttpClient<NgrokApiClient>(x =>
        {
            x.BaseAddress = new Uri("http://localhost:4040");
            x.DefaultRequestHeaders.Accept.Clear();
            x.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });
        
        return services;
    }
}