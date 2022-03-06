using FluffySpoon.AspNet.Ngrok.New.Models;
using Microsoft.Extensions.Hosting;

namespace FluffySpoon.AspNet.Ngrok.New;

public interface INgrokHostedService : IHostedService
{
    Tunnel? ActiveTunnel { get; }
    
    Task WaitUntilReadyAsync(CancellationToken cancellationToken = default);
}