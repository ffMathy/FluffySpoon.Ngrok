using FluffySpoon.AspNet.Ngrok.New.Models;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;

namespace FluffySpoon.AspNet.Ngrok.New;

public class NgrokHostedService : INgrokHostedService
{
    private readonly IServer _server;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly INgrokDownloader _downloader;
    private readonly INgrokProcess _process;
    private readonly INgrokApiClient _apiClient;
    private readonly IEnumerable<INgrokLifetimeHook> _hooks;

    private bool _isStarted;
    
    public NgrokHostedService(
        IServer server,
        IHostApplicationLifetime lifetime,
        INgrokDownloader downloader,
        INgrokProcess process,
        INgrokApiClient apiClient,
        IEnumerable<INgrokLifetimeHook> hooks)
    {
        _server = server;
        _lifetime = lifetime;
        _downloader = downloader;
        _process = process;
        _apiClient = apiClient;
        _hooks = hooks;
    }
    
    public async Task WaitUntilReadyAsync()
    {
        while (!_isStarted)
            await Task.Delay(25);
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var combinedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _lifetime.ApplicationStopping).Token;
        
        NgrokProcess.KillAll();
        
        await _downloader.DownloadExecutableAsync(combinedCancellationToken);
        _process.Start();

        Tunnel? tunnel = null;
        
        _lifetime.ApplicationStarted.Register(async () =>
        {
            var feature = _server.Features.Get<IServerAddressesFeature>();
            if (feature == null)
                throw new InvalidOperationException("Ngrok requires the IServerAddressesFeature to be accessible.");

            var address = feature.Addresses
                .Select(x => new Uri(x))
                .OrderByDescending(x => x.Scheme == "http" ?
                    1 :
                    0)
                .First()
                .ToString();
            tunnel = await _apiClient.CreateTunnelAsync(
                AppDomain.CurrentDomain.FriendlyName,
                address,
                cancellationToken);
            if (tunnel != null)
            {
                await Task.WhenAll(_hooks
                    .Select(x => x.OnCreatedAsync(tunnel, cancellationToken)));
            }

            _isStarted = true;
        });
        
        _lifetime.ApplicationStopping.Register(async () =>
        {
            if (tunnel != null)
            {
                await Task.WhenAll(_hooks
                    .Select(x => x.OnDestroyedAsync(tunnel, cancellationToken)));
            }

            _process.Stop();
        });
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _process.Stop();
    }
}