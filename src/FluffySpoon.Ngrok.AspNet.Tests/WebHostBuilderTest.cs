using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluffySpoon.Ngrok.AspNet.Sample;
using FluffySpoon.Ngrok.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FluffySpoon.Ngrok.AspNet.Tests;

class Hook : INgrokLifetimeHook
{
    public TunnelResponse Tunnel { get; private set; }
    
    public bool IsDestroyed { get; private set; }
    
    public Task OnCreatedAsync(TunnelResponse tunnel, CancellationToken cancellationToken)
    {
        Tunnel = tunnel;
        return Task.CompletedTask;
    }

    public Task OnDestroyedAsync(TunnelResponse tunnel, CancellationToken cancellationToken)
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
        var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        
        await using var host = Startup.Create();
        await host.StartAsync(timeoutToken);

        using var httpClient = new HttpClient();
        await AssertIsUrlReachableAsync(httpClient, "http://localhost:14568/");

        var ngrokService = host.Services.GetRequiredService<INgrokService>();
        await ngrokService.WaitUntilReadyAsync(timeoutToken);
        
        var tunnel = ngrokService.ActiveTunnels.SingleOrDefault();
        Assert.IsNotNull(tunnel);

        await AssertIsUrlReachableAsync(httpClient, tunnel.PublicUrl);
    }
    
    [TestMethod]
    public async Task TunnelCallbacksAreMadeProperly()
    {
        var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        
        var hook = new Hook();
        await using var host = Startup.Create(x => x
            .AddTransient<INgrokLifetimeHook>(_ => hook));
        
        Assert.IsFalse(hook.IsDestroyed);
        await host.StartAsync(timeoutToken);

        using var httpClient = new HttpClient();
        await AssertIsUrlReachableAsync(httpClient, "http://localhost:14568/");

        var ngrokService = host.Services.GetRequiredService<INgrokService>();
        await ngrokService.WaitUntilReadyAsync(timeoutToken);
        
        var tunnel = ngrokService.ActiveTunnels.SingleOrDefault();
        Assert.IsNotNull(tunnel);
        
        Assert.IsNotNull(hook.Tunnel);

        await AssertIsUrlReachableAsync(httpClient, tunnel.PublicUrl);
        Assert.IsFalse(hook.IsDestroyed);

        await host.StopAsync(timeoutToken);
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

            await Task.Delay(1000);
        }

        Assert.Fail("Timeout for URL " + url);
    }
}