using Microsoft.Extensions.Hosting;

namespace FluffySpoon.AspNet.Ngrok.New;

public interface INgrokHostedService : IHostedService
{
    Task WaitUntilReadyAsync();
}