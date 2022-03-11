using FluffySpoon.Ngrok;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;

namespace FluffySpoon.AspNet.Ngrok;

public class NgrokHostedService : INgrokHostedService
{
    private readonly IServer _server;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly INgrokService _service;
    
    public NgrokHostedService(
        IServer server,
        IHostApplicationLifetime lifetime,
        INgrokService service)
    {
        _server = server;
        _lifetime = lifetime;
        _service = service;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _service.InitializeAsync(cancellationToken);
        
        var combinedCancellationToken = CancellationTokenSource
            .CreateLinkedTokenSource(cancellationToken, _lifetime.ApplicationStopping)
            .Token;

        _lifetime.ApplicationStarted.Register(() =>
        {
            var feature = _server.Features.Get<IServerAddressesFeature>();
            if (feature == null)
                throw new InvalidOperationException("Ngrok requires the IServerAddressesFeature to be accessible.");

            var address = feature.Addresses
                .Select(x => new Uri(x))
                .OrderByDescending(x => x.Scheme == "http" ? 1 : 0)
                .First();
            _service.StartAsync(address, combinedCancellationToken);
        });
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _service.StopAsync(cancellationToken);
    }
}