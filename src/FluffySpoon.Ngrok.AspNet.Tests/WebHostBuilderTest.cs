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
[Ignore]
public class WebHostBuilderTest
{
    [TestMethod]
    public async Task CanCreateHostAndReachItViaNgrok()
    {
        var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        
        await using var host = Startup.Create();
        await host.StartAsync(timeoutToken);

        using var httpClient = new HttpClient();
        await AssertIsUrlReachableAsync(httpClient, "http://localhost:14568/api");

        var ngrokService = host.Services.GetRequiredService<INgrokService>();
        await ngrokService.WaitUntilReadyAsync(timeoutToken);
        
        var tunnel = ngrokService.ActiveTunnels.SingleOrDefault();
        Assert.IsNotNull(tunnel);

        await AssertIsUrlReachableAsync(httpClient, tunnel.PublicUrl);;

        await host.StopAsync(timeoutToken);
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
        await AssertIsUrlReachableAsync(httpClient, "http://localhost:14568/api");

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
    
    [TestMethod]
    public async Task CanFetchHtmlFileWithNgrokToken()
    {
        var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        
        await using var host = Startup.Create(null);
        await host.StartAsync(timeoutToken);

        using var httpClient = new HttpClient();
        await AssertIsUrlReachableAsync(httpClient, "http://localhost:14568/wwwroot/html-file.html");

        var ngrokService = host.Services.GetRequiredService<INgrokService>();
        await ngrokService.WaitUntilReadyAsync(timeoutToken);
        
        var tunnel = ngrokService.ActiveTunnels.SingleOrDefault();
        Assert.IsNotNull(tunnel);

        await AssertIsUrlReachableAsync(httpClient, $"{tunnel.PublicUrl}/wwwroot/html-file.html");

        await host.StopAsync(timeoutToken);
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

                var responseHtml = await response.Content.ReadAsStringAsync();
                Assert.IsFalse(responseHtml.Contains("ERR_NGROK_"), "Ngrok error: " + responseHtml);

                return;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"{url} ({ex.StatusCode}): {ex}");
            }

            await Task.Delay(1000);
        }

        Assert.Fail("Timeout for URL " + url);
    }
}