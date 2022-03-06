using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluffySpoon.AspNet.Ngrok.Models;
using Microsoft.Extensions.Hosting;

namespace FluffySpoon.AspNet.Ngrok
{
    public interface INgrokHostedService : IHostedService
    {
        Task<IReadOnlyCollection<Tunnel>> GetTunnelsAsync();

        event Action<IEnumerable<Tunnel>> Ready;
    }
}