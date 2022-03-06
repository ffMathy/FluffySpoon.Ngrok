using System.Text.Json.Serialization;

namespace FluffySpoon.AspNet.Ngrok.Models
{
    public class Tunnel
    {
        public string Name { get; set; }
        public string Uri { get; set; }

        [JsonPropertyName("public_url")]
        public string PublicUrl { get; set; }

        [JsonPropertyName("proto")]
        public string Protocol { get; set; }
        
        public Config Config { get; set; }
        public Metrics Metrics { get; set; }
    }
}