using FluffySpoon.AspNet.Ngrok.New.Models;

namespace FluffySpoon.AspNet.Ngrok.New;

public interface INgrokApiClient
{
    Task<Tunnel> CreateTunnelAsync(
        string projectName, 
        string address,
        CancellationToken cancellationToken);
}