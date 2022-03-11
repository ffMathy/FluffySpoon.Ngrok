using FluffySpoon.Ngrok.Models;

namespace FluffySpoon.Ngrok;

public interface INgrokService
{
    Tunnel? ActiveTunnel { get; }
    
    Task WaitUntilReadyAsync(CancellationToken cancellationToken = default);
    
    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task<Tunnel> StartAsync(
        string host, 
        CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);
}