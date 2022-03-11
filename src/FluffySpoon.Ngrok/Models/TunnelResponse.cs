using Newtonsoft.Json;
using NgrokApi;

namespace FluffySpoon.Ngrok.Models;

public class TunnelListResponse
{
    public TunnelResponse[] Tunnels { get; set; }
}

public class TunnelResponse : Tunnel
{
    public TunnelConfig Config { get; set; }
}

public class TunnelConfig
{
    [JsonProperty("addr")]
    public string Address { get; set; }
}