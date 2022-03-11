using FluffySpoon.Ngrok.Models;

namespace FluffySpoon.Ngrok;

public interface INgrokLifetimeHook
{
    Task OnCreatedAsync(Tunnel tunnel, CancellationToken cancellationToken);
    Task OnDestroyedAsync(Tunnel tunnel, CancellationToken cancellationToken);
}