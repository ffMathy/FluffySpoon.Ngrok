using System.Text.Json.Serialization;

namespace FluffySpoon.AspNet.Ngrok.New.Models;

public class Details
{
    [JsonPropertyName("err")]
    public string ErrorMessage { get; set; }
}