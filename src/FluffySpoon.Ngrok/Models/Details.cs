using Newtonsoft.Json;

namespace FluffySpoon.Ngrok.Models;

public class Details
{
    [JsonProperty("err")]
    public string ErrorMessage { get; set; }
}