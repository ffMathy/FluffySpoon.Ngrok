using FluffySpoon.Ngrok.Models;
using NgrokApi;

namespace FluffySpoon.Ngrok;

public interface INgrokApiClient
{
    Task<Tunnel> CreateTunnelAsync(
        string projectName, 
        Uri address,
        CancellationToken cancellationToken);

    Task<Tunnel[]> GetTunnelsAsync(CancellationToken cancellationToken);
}