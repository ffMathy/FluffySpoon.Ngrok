using System.Text.Json.Serialization;

namespace FluffySpoon.Ngrok.Models;

public class Config
{
    [JsonPropertyName("addr")]
    public string Address { get; set; }
    
    public bool Inspect { get; set; }
}