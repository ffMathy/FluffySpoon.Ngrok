using FluffySpoon.AspNet.Ngrok.New.Models;

namespace FluffySpoon.AspNet.Ngrok.New;

public interface INgrokApiClient
{
    Task<bool> HasTunnelAsync(string address, CancellationToken cancellationToken);
    Task<Tunnel[]?> TryGetTunnelListAsync(CancellationToken cancellationToken);

    Task<Tunnel?> CreateTunnelAsync(
        string projectName, 
        string address,
        CancellationToken cancellationToken);
}