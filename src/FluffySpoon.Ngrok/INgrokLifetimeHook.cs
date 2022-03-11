using FluffySpoon.Ngrok.Models;
using NgrokApi;

namespace FluffySpoon.Ngrok;

public interface INgrokLifetimeHook
{
    Task OnCreatedAsync(Tunnel tunnel, CancellationToken cancellationToken);
    Task OnDestroyedAsync(Tunnel tunnel, CancellationToken cancellationToken);
}