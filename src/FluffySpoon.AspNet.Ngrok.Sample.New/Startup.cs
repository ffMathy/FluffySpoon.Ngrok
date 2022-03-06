using FluffySpoon.AspNet.Ngrok.New;

namespace FluffySpoon.AspNet.Ngrok.Sample.New;

public class Startup
{
    public static WebApplication Create()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddNgrok();
        builder.Services.AddControllersWithViews();

        var app = builder.Build();
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        return app;
    }
}