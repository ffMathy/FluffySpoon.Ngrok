using FluffySpoon.Ngrok.Models;

namespace FluffySpoon.Ngrok;

public class NgrokService : INgrokService
{
    private readonly INgrokDownloader _downloader;
    private readonly INgrokProcess _process;
    private readonly INgrokApiClient _apiClient;
    private readonly IEnumerable<INgrokLifetimeHook> _hooks;

    private bool _isInitialized = false;
    
    public Tunnel? ActiveTunnel { get; private set; }
    
    public NgrokService(
        INgrokDownloader downloader,
        INgrokProcess process,
        INgrokApiClient apiClient,
        IEnumerable<INgrokLifetimeHook> hooks)
    {
        _downloader = downloader;
        _process = process;
        _apiClient = apiClient;
        _hooks = hooks;
    }
    
    public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
    {
        while (ActiveTunnel == null && !cancellationToken.IsCancellationRequested)
            await Task.Delay(25, cancellationToken);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
            return;
        
        _isInitialized = true;
        
        await _downloader.DownloadExecutableAsync(cancellationToken);
        _process.Start();
    }

    public async Task<Tunnel> StartAsync(
        string host,
        CancellationToken cancellationToken)
    {
        await InitializeAsync(cancellationToken);
        
        var tunnel = await _apiClient.CreateTunnelAsync(
            AppDomain.CurrentDomain.FriendlyName,
            host,
            cancellationToken);

        ActiveTunnel = tunnel;
        
        await Task.WhenAll(_hooks
            .Select(x => x.OnCreatedAsync(tunnel, cancellationToken)));

        return tunnel;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (ActiveTunnel != null)
        {
            await Task.WhenAll(_hooks
                .Select(x => x.OnDestroyedAsync(ActiveTunnel, cancellationToken)));
        }

        _process.Stop();
        ActiveTunnel = null;
    }
}