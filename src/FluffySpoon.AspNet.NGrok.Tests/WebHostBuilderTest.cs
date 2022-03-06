using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluffySpoon.AspNet.Ngrok.New;
using FluffySpoon.AspNet.Ngrok.New.Models;
using FluffySpoon.AspNet.Ngrok.Sample.New;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        var host = Startup.Create();
        await host.StartAsync();

        using var httpClient = new HttpClient();
        await AssertIsUrlReachableAsync(httpClient, "http://localhost:14568/");

        var ngrokService = host.Services.GetRequiredService<INgrokHostedService>();
        await ngrokService.WaitUntilReadyAsync();
        
        var tunnel = ngrokService.ActiveTunnel;

        Assert.IsNotNull(tunnel);

        await AssertIsUrlReachableAsync(httpClient, tunnel.PublicUrl);
    }

    private static async Task AssertIsUrlReachableAsync(HttpClient httpClient, string url)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(60))
        {
            try
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                return;
            }
            catch (HttpRequestException)
            {
            }

            await Task.Delay(1000);
        }

        Assert.Fail("Timeout for URL " + url);
    }
}