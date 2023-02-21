using Newtonsoft.Json;

namespace FluffySpoon.Ngrok.Models;

public class CreateTunnelApiRequest
{
	[JsonProperty("name")]
	public string Name { get; set; } = null!;

	[JsonProperty("addr")] 
	public string Address { get; set; } = null!;

	[JsonProperty("proto")] 
	public string Protocol { get; set; } = null!;
}