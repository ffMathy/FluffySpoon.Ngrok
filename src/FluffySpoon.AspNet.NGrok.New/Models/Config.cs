using System.Text.Json.Serialization;

namespace FluffySpoon.AspNet.Ngrok.New.Models;

public class Config
{
    [JsonPropertyName("addr")]
    public string Address { get; set; }
    
    public bool Inspect { get; set; }
}