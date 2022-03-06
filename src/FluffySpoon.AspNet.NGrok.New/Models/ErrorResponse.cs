using System.Text.Json.Serialization;

namespace FluffySpoon.AspNet.Ngrok.New.Models;

public class ErrorResponse
{
	[JsonPropertyName("error_code")]
	public int ErrorCode { get; set; }

	[JsonPropertyName("status_code")]
	public int StatusCode { get; set; }

	[JsonPropertyName("msg")]
	public string Message { get; set; }

	public Details Details { get; set; }
}