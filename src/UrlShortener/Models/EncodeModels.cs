namespace UrlShortener.Models
{
    public class EncodeRequest
    {
        public required string Url { get; set; }
    }
    public class EncodeResponse
    {
        public required string ShortUrl { get; set; }
    }
}
