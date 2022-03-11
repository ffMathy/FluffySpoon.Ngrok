using FluffySpoon.Ngrok.Models;
using NgrokApi;

namespace FluffySpoon.Ngrok;

public interface INgrokApiClient
{
    Task<TunnelResponse> CreateTunnelAsync(
        string projectName, 
        Uri address,
        CancellationToken cancellationToken);

    Task<TunnelResponse[]> GetTunnelsAsync(CancellationToken cancellationToken);
}