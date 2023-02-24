using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FluffySpoon.Ngrok.AspNet;

public class NgrokHostedService : INgrokHostedService
{
    private readonly IServer _server;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly INgrokService _service;
    private readonly ILogger<NgrokHostedService> _logger;

    private Task? _stopTask;
    
    public NgrokHostedService(
        IServer server,
        IHostApplicationLifetime lifetime,
        INgrokService service,
        ILogger<NgrokHostedService> logger)
    {
        _server = server;
        _lifetime = lifetime;
        _service = service;
        _logger = logger;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _service.InitializeAsync(cancellationToken);
        
        var combinedCancellationToken = CancellationTokenSource
            .CreateLinkedTokenSource(cancellationToken, _lifetime.ApplicationStopping)
            .Token;

        _lifetime.ApplicationStarted.Register(() =>
        {
            _logger.LogDebug("Application has started - will start Ngrok");
            
            var feature = _server.Features.Get<IServerAddressesFeature>();
            if (feature == null)
                throw new InvalidOperationException("Ngrok requires the IServerAddressesFeature to be accessible.");

            var address = feature.Addresses
                .Select(x => new Uri(x))
                .OrderByDescending(x => x.Scheme == "http" ? 1 : 0)
                .First();
            _service.StartAsync(address, combinedCancellationToken);
        });
        
        _lifetime.ApplicationStopped.Register(() =>
        {
            _logger.LogDebug("Application has stopped - will stop Ngrok");
            StopProcessAsync(combinedCancellationToken);
        });
    }

    private Task StopProcessAsync(CancellationToken combinedCancellationToken)
    {
        if (_stopTask != null)
            return _stopTask;
        
        return _stopTask = _service.StopAsync(combinedCancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return StopProcessAsync(cancellationToken);
    }
}