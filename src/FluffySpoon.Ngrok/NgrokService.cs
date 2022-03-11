using NgrokApi;

namespace FluffySpoon.Ngrok;

public class NgrokService : INgrokService
{
    private readonly INgrokDownloader _downloader;
    private readonly INgrokProcess _process;
    private readonly IEnumerable<INgrokLifetimeHook> _hooks;
    private readonly INgrokApiClient _ngrok;

    private bool _isInitialized;
    
    private readonly HashSet<Tunnel> _activeTunnels;

    public IReadOnlyCollection<Tunnel> ActiveTunnels => _activeTunnels;

    public NgrokService(
        INgrokDownloader downloader,
        INgrokProcess process,
        IEnumerable<INgrokLifetimeHook> hooks,
        INgrokApiClient ngrok)
    {
        _downloader = downloader;
        _process = process;
        _hooks = hooks;
        _ngrok = ngrok;

        _activeTunnels = new HashSet<Tunnel>();
    }
    
    public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
    {
        while (!ActiveTunnels.Any() && !cancellationToken.IsCancellationRequested)
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
        Uri host,
        CancellationToken cancellationToken)
    {
        await InitializeAsync(cancellationToken);
        
        var tunnel = await _ngrok.CreateTunnelAsync(
            AppDomain.CurrentDomain.FriendlyName,
            host,
            cancellationToken);

        _activeTunnels.Add(tunnel);
        
        await Task.WhenAll(_hooks
            .Select(x => x.OnCreatedAsync(tunnel, cancellationToken)));

        return tunnel;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAll(_activeTunnels
            .Select(tunnel => Task.WhenAll(_hooks
                .Select(hook => hook.OnDestroyedAsync(tunnel, cancellationToken)))));

        _process.Stop();
        _activeTunnels.Clear();
    }
}