using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;

namespace FluffySpoon.AspNet.Ngrok.New;

public class NgrokHostedService : IHostedService
{
    private readonly IServer _server;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly INgrokDownloader _downloader;
    private readonly INgrokProcess _process;

    public NgrokHostedService(
        IServer server,
        IHostApplicationLifetime lifetime,
        INgrokDownloader downloader,
        INgrokProcess process)
    {
        _server = server;
        _lifetime = lifetime;
        _downloader = downloader;
        _process = process;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var combinedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _lifetime.ApplicationStopping).Token;
        
        NgrokProcess.KillAll();
        
        await _downloader.DownloadExecutableAsync(combinedCancellationToken);
        _process.Start();

        _lifetime.ApplicationStopping.Register(_process.Stop);
        _lifetime.ApplicationStarted.Register(() =>
        {
            var feature = _server.Features.Get<IServerAddressesFeature>();
            if (feature == null)
                throw new InvalidOperationException("Ngrok requires the IServerAddressesFeature to be accessible.");
        });
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _process.Stop();
    }
}