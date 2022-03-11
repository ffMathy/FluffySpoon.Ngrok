using System.Text.Json.Serialization;

namespace FluffySpoon.Ngrok.Models;

public class CreateTunnelApiRequest
{
	public string Name { get; set; } = null!;

	[JsonPropertyName("addr")] 
	public string Address { get; set; } = null!;

	[JsonPropertyName("proto")] 
	public string Protocol { get; set; } = null!;

	public string Subdomain { get; set; } = null!;

	public string HostHeader { get; set; } = null!;
}