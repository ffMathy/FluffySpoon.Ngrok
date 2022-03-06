using Newtonsoft.Json;

namespace FluffySpoon.AspNet.Ngrok.Models
{
    public class Details
    {
        [JsonProperty("err")]
        public string ErrorMessage { get; set; }
    }
}