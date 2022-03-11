using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluffySpoon.AspNet.Ngrok.Sample;
using FluffySpoon.Ngrok;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NgrokApi;

namespace FluffySpoon.AspNet.Ngrok.Tests;

class Hook : INgrokLifetimeHook
{
    public Tunnel Tunnel { get; private set; }
    
    public bool IsDestroyed { get; private set; }
    
    public Task OnCreatedAsync(Tunnel tunnel, CancellationToken cancellationToken)
    {
        Tunnel = tunnel;
        return Task.CompletedTask;
    }

    public Task OnDestroyedAsync(Tunnel tunnel, CancellationToken cancellationToken)
    {
        IsDestroyed = true;
        return Task.CompletedTask;
    }
}

[TestClass]
public class WebHostBuilderTest
{
    [TestMethod]
    public async Task CanCreateHostAndReachItViaNgrok()
    {
        await using var host = Startup.Create();
        await host.StartAsync();

        using var httpClient = new HttpClient();
        await AssertIsUrlReachableAsync(httpClient, "http://localhost:14568/");

        var ngrokService = host.Services.GetRequiredService<INgrokService>();
        await ngrokService.WaitUntilReadyAsync();
        
        var tunnel = ngrokService.ActiveTunnels.SingleOrDefault();
        Assert.IsNotNull(tunnel);

        await AssertIsUrlReachableAsync(httpClient, tunnel.PublicUrl);

        await host.StopAsync();
    }
    
    [TestMethod]
    public async Task TunnelCallbacksAreMadeProperly()
    {
        var hook = new Hook();
        await using var host = Startup.Create(x => x
            .AddTransient<INgrokLifetimeHook>(_ => hook));
        
        Assert.IsFalse(hook.IsDestroyed);
        await host.StartAsync();

        using var httpClient = new HttpClient();
        await AssertIsUrlReachableAsync(httpClient, "http://localhost:14568/");

        var ngrokService = host.Services.GetRequiredService<INgrokService>();
        await ngrokService.WaitUntilReadyAsync();
        
        var tunnel = ngrokService.ActiveTunnels.SingleOrDefault();
        Assert.IsNotNull(tunnel);
        
        Assert.IsNotNull(hook.Tunnel);

        await AssertIsUrlReachableAsync(httpClient, tunnel.PublicUrl);
        Assert.IsFalse(hook.IsDestroyed);

        await host.StopAsync();
        Assert.IsTrue(hook.IsDestroyed);
    }

    private static async Task AssertIsUrlReachableAsync(HttpClient httpClient, string url)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(30))
        {
            try
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                return;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex);
            }

            await Task.Delay(5000);
        }

        Assert.Fail("Timeout for URL " + url);
    }
}