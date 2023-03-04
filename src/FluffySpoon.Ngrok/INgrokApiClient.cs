using FluffySpoon.Ngrok.Models;

namespace FluffySpoon.Ngrok;

public interface INgrokApiClient
{
    Task<TunnelResponse> CreateTunnelAsync(
        string projectName, 
        Uri address,
        CancellationToken cancellationToken);

    Task<TunnelResponse[]> GetTunnelsAsync(CancellationToken cancellationToken);
    Task<bool> IsNgrokReady(CancellationToken cancellationToken);
}