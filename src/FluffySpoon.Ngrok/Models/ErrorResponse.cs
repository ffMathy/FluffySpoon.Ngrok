using Newtonsoft.Json;

namespace FluffySpoon.Ngrok.Models;

public class ErrorResponse
{
	[JsonProperty("error_code")]
	public int ErrorCode { get; set; }

	[JsonProperty("status_code")]
	public int StatusCode { get; set; }

	[JsonProperty("msg")]
	public string Message { get; set; }

	public Details Details { get; set; }
}