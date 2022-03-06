namespace FluffySpoon.AspNet.Ngrok.Models
{
    public class Http
    {
        public int Count { get; set; }
        public decimal Rate1 { get; set; }
        public decimal Rate5 { get; set; }
        public decimal Rate15 { get; set; }
        public decimal P50 { get; set; }
        public decimal P90 { get; set; }
        public decimal P95 { get; set; }
        public decimal P99 { get; set; }
    }
}