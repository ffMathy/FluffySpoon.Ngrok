using System.Text.Json.Serialization;

namespace FluffySpoon.Ngrok.Models;

public class TunnelListResponse
{
    public TunnelResponse[] Tunnels { get; set; }
}

public class TunnelResponse
{
    public TunnelConfig Config { get; set; }

    public string Name { get; set; }

    // <summary>
    // URL of the ephemeral tunnel's public endpoint
    // </summary>
    [JsonPropertyName("public_url")] public string PublicUrl { get; set; }
    // <summary>
    // tunnel protocol for ephemeral tunnels. one of <c>http</c>, <c>https</c>,
    // <c>tcp</c> or <c>tls</c>
    // </summary>
    [JsonPropertyName("proto")] public string Proto { get; set; }
}

public class TunnelConfig
{
    [JsonPropertyName("addr")] public string Address { get; set; }
}