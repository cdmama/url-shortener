namespace UrlShortener.Models
{
    public class DecodeRequest
    {
        public required string ShortUrl { get; set; }
    }
    public class DecodeResponse
    {
        public required string Url { get; set; }
    }
}
