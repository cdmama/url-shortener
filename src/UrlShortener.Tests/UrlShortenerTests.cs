using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Http.Json;
using UrlShortener.Models;
using UrlShortener.Services;

namespace UrlShortener.Tests
{
    public class UrlShortenerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _application;

        public UrlShortenerTests(WebApplicationFactory<Program> application) 
        {
            _application = application;
        }

        private HttpClient CreateClient(List<string> mockData)
        {
            return _application.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.Replace(ServiceDescriptor.Singleton(typeof(IUrlDatabase), _ => new UrlDatabase(mockData)));
                });
            }).CreateClient();
        }

        [Fact]
        public async Task GET_ping()
        {
            var client = _application.CreateClient();
            var response = await client.GetAsync("/ping");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Be("pong");
        }

        #region Encode
        [Fact]
        public async Task POST_encode_with_invalid_url()
        {
            var client = _application.CreateClient();

            var response = await client.PostAsJsonAsync("/encode", new EncodeRequest()
            {
                Url = "not-a-url"
            });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task POST_encode_with_valid_url()
        {
            List<string> mockDatabase = [
                "https://www.google.com",
                "https://twitch.tv",
                "https://youtube.com",
            ];

            var client = CreateClient(mockDatabase);

            var response = await client.PostAsJsonAsync("/encode", new EncodeRequest()
            {
                Url = "https://docs.mypassglobal.com/api/index.html"
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadFromJsonAsync<EncodeResponse>();

            // should be added as index 3
            content.ShortUrl.Should().Be("http://localhost/AwAAAA");
        }

        [Fact]
        public async Task POST_encode_with_valid_url_duplicate()
        {
            List<string> mockDatabase = [
                "https://www.google.com",
                "https://twitch.tv",
                "https://docs.mypassglobal.com/api/index.html",
                "https://youtube.com"
            ];

            var client = CreateClient(mockDatabase);

            var response = await client.PostAsJsonAsync("/encode", new EncodeRequest()
            {
                Url = "https://docs.mypassglobal.com/api/index.html"
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadFromJsonAsync<EncodeResponse>();

            // should be existing as index 2
            content.ShortUrl.Should().Be("http://localhost/AgAAAA");
        }
        #endregion

        #region Decode
        [Fact]
        public async Task POST_decode_with_invalid_url()
        {
            var client = _application.CreateClient();

            var response = await client.PostAsJsonAsync("/decode", new DecodeRequest()
            {
                ShortUrl = "not-a-valid-url"
            });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task POST_decode_with_valid_url_invalid_format()
        {
            var client = _application.CreateClient();

            var response = await client.PostAsJsonAsync("/decode", new DecodeRequest()
            {
                ShortUrl = "https://www.google.com"
            });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task POST_decode_with_valid_url_not_found()
        {
            List<string> mockDatabase = [
                "https://www.google.com",
                "https://twitch.tv",
                "https://youtube.com"
            ];

            var client = CreateClient(mockDatabase);

            var response = await client.PostAsJsonAsync("/decode", new DecodeRequest()
            {
                ShortUrl = "http://localhost/AwAAAA"
            });

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task POST_decode_with_valid_url_found()
        {
            List<string> mockDatabase = [
                "https://www.google.com",
                "https://twitch.tv",
                "https://docs.mypassglobal.com/api/index.html",
                "https://youtube.com"
            ];

            var client = CreateClient(mockDatabase);

            var response = await client.PostAsJsonAsync("/decode", new DecodeRequest()
            {
                ShortUrl = "http://localhost/AgAAAA"
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadFromJsonAsync<DecodeResponse>();

            // should be existing as index 2
            content.Url.Should().Be("https://docs.mypassglobal.com/api/index.html");
        }
        #endregion

        #region Follow

        [Fact]
        public async Task GET_follow_invalid_url()
        {
            var client = _application.CreateClient();

            var response = await client.GetAsync("/not-a-valid-shortcode");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GET_follow_valid_url()
        {
            List<string> mockDatabase = [
                "https://www.google.com",
                "https://twitch.tv",
                "https://youtube.com"
           ];

            var client = CreateClient(mockDatabase);

            var response = await client.GetAsync("/AAAAAA");

            response.RequestMessage.RequestUri.Should().Be("https://www.google.com");
        }

        #endregion
    }
}