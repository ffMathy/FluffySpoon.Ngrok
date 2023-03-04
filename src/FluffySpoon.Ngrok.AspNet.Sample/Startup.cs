using Microsoft.Extensions.FileProviders;

namespace FluffySpoon.Ngrok.AspNet.Sample;

public class Startup
{
    public static WebApplication Create(Action<IServiceCollection>? configureServices = null)
    {
        var builder = WebApplication.CreateBuilder();
        
        builder.Services.AddNgrokHostedService(new NgrokOptions()
        {
            ShowNgrokWindow = true
        });
        builder.Services
            .AddControllersWithViews()
            .AddApplicationPart(typeof(Startup).Assembly);
        
        configureServices?.Invoke(builder.Services);

        var app = builder.Build();

        app.Urls.Clear();
        app.Urls.Add("http://localhost:14568");
        
        app.UseStaticFiles(new StaticFileOptions()
        {
            FileProvider = new EmbeddedFileProvider(
                typeof(Startup).Assembly,
                "FluffySpoon.Ngrok.AspNet.Sample")
        });

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        return app;
    }
}