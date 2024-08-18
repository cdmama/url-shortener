namespace UrlShortener.Services
{
    public interface IUrlDatabase
    {
        List<string> Urls { get; set; }
    }

    public class UrlDatabase : IUrlDatabase
    {
        public UrlDatabase() { }

        public UrlDatabase(List<string> urls)
        {
            Urls = urls;
        }

        public List<string> Urls { get; set; } = [];
    }
}