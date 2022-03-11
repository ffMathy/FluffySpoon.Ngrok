using FluffySpoon.Ngrok.Models;

namespace FluffySpoon.Ngrok;

public interface INgrokApiClient
{
    Task<Tunnel> CreateTunnelAsync(
        string projectName, 
        string address,
        CancellationToken cancellationToken);
}