using FluffySpoon.Ngrok.Models;

namespace FluffySpoon.Ngrok;

public interface INgrokLifetimeHook
{
    Task OnCreatedAsync(TunnelResponse tunnel, CancellationToken cancellationToken);
    Task OnDestroyedAsync(TunnelResponse tunnel, CancellationToken cancellationToken);
}