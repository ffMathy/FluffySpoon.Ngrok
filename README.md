# FluffySpoon.Ngrok
A NuGet package used to start Ngrok programmatically and fetch the tunnel URL. Useful to enable for local development when a public URL is needed.

# Examples
## Console application

Add `AddNgrok` to your service registration

```csharp
var services = new ServiceCollection();
services.AddNgrok();

var serviceProvider = services.BuildServiceProvider();
var ngrokService = serviceProvider.GetService<INgrokService>();

//this downloads the Ngrok executable and starts it in the background.
await ngrokService.InitializeAsync();

//this opens a tunnel for the given URL
var tunnel = await ngrokService.StartAsync(new Uri("http://localhost:80"));
Console.WriteLine("Ngrok tunnel URL for localhost:80 is: " + tunnel.PublicUrl);

//the active tunnel can also be accessed using ngrokService.ActiveTunnel.

//we may stop the tunnel as well.
await ngrokService.StopAsync();
```

## ASP .NET Core application
For this example, the `FluffySpoon.Ngrok.AspNet` package has to be installed.

```csharp
var builder = WebApplication.CreateBuilder();

//this is the line that is needed to automatically start the tunnel with your ASP .NET Core application.
builder.Services.AddNgrokHostedService();

builder.Services.AddControllersWithViews();

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();
```

## Getting the tunnel URL
To get the tunnel URL in an ASP .NET Core application, you can just inject a `INgrokService` into your controller or class.

```csharp
public class HomeController : Controller
{
    private readonly INgrokService _ngrokService;

    public HomeController(INgrokService ngrokService)
    {
        _ngrokService = ngrokService;
    }

    public IActionResult Index()
    {
        var tunnel = await _ngrokService.ActiveTunnel;
        Console.WriteLine("Tunnel URL is: " + tunnel.PublicUrl);
        
        return View();
    }
}
```

## Waiting for the tunnel to be ready
On the `INgrokService`, you can call a method to wait for the tunnel to be ready.

```csharp
await ngrokService.WaitUntilReadyAsync();
```

## Registering lifetime hooks
These are useful if you want to debug things like webhooks etc locally.

```csharp
class SomeLifetimeHook : INgrokLifetimeHook
{    
    public Task OnCreatedAsync(TunnelResponse tunnel, CancellationToken cancellationToken)
    {
        //TODO: do something when a tunnel has been created. for instance, here you could register a webhook for "tunnel.PublicUrl".
        return Task.CompletedTask;
    }

    public Task OnDestroyedAsync(TunnelResponse tunnel, CancellationToken cancellationToken)
    {
        //TODO: do something when a tunnel has been destroyed. for instance, here you could unregister a webhook for "tunnel.PublicUrl".
        return Task.CompletedTask;
    }
}
```

And you can register a lifetime hook as such:

```csharp
services.AddNgrokLifetimeHook<MyHook>();
```
