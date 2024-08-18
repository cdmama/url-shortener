using Microsoft.AspNetCore.WebUtilities;

namespace UrlShortener.Services
{
    public interface IUrlShortenerService
    {
        string GetShortCode(string url);
        string GetFullUrl(string url);
        string GetFullUrlFromShortCode(string shortcode);
    }

    public class UrlShortenerService(IUrlDatabase urlDatabase) : IUrlShortenerService
    {
        public string GetShortCode(string url)
        {
            if (!IsValidUrl(url))
                throw new FormatException("Invalid url");

            if (!urlDatabase.Urls.Contains(url))
                urlDatabase.Urls.Add(url);

            var index = urlDatabase.Urls.IndexOf(url);
            var shortCode = WebEncoders.Base64UrlEncode(BitConverter.GetBytes(index));

            return shortCode;
        }

        public string GetFullUrl(string url)
        {
            if (!IsValidShortUrl(url, out var uri))
                throw new FormatException("Invalid url");
            
            var shortcode = uri.Segments[1];

            var fullUrl = GetFullUrlFromShortCode(shortcode);

            return fullUrl;
        }

        public string GetFullUrlFromShortCode(string shortcode)
        {
            if (shortcode.Length > 6)
                throw new FormatException("Invalid shortcode");

            var index = BitConverter.ToInt32(WebEncoders.Base64UrlDecode(shortcode));

            if (index < 0 || index >= urlDatabase.Urls.Count)
                throw new ArgumentOutOfRangeException(shortcode);

            var fullUrl = urlDatabase.Urls[index];

            return fullUrl;
        }

        private static bool IsValidUrl(string url)
        {
            return Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }

        private static bool IsValidShortUrl(string url, out Uri uri)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                return false;

            if (uri.Host != "localhost")
                return false;

            // there should be 2 segments: 1 = /, 2 = <shortcode>
            return uri.Segments.Length == 2;          
        }
    }
}
