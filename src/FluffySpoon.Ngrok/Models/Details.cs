using System.Text.Json.Serialization;

namespace FluffySpoon.Ngrok.Models;

public class Details
{
    [JsonPropertyName("err")]
    public string ErrorMessage { get; set; }
}