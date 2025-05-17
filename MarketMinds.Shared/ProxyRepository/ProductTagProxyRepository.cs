using System.Text.Json;
using Microsoft.Extensions.Configuration;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.ProxyRepository
{
    public class ProductTagProxyRepository : IProductTagRepository
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions jsonOptions;

        public ProductTagProxyRepository(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            var apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:500";
            if (string.IsNullOrEmpty(apiBaseUrl))
            {
                throw new InvalidOperationException("API base URL is null or empty");
            }

            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }

            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");
            Console.WriteLine($"ProductTag Repository initialized with base address: {httpClient.BaseAddress}");

            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        // Raw methods for API access
        public string GetAllProductTagsRaw()
        {
            var response = httpClient.GetAsync("ProductTag").Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

        public string CreateProductTagRaw(string displayTitle)
        {
            var requestContent = new StringContent(
                $"{{\"displayTitle\":\"{displayTitle}\"}}",
                System.Text.Encoding.UTF8,
                "application/json");
            var response = httpClient.PostAsync("ProductTag", requestContent).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

        public void DeleteProductTag(string displayTitle)
        {
            var response = httpClient.DeleteAsync($"ProductTag/{displayTitle}").Result;
            response.EnsureSuccessStatusCode();
        }

        // Legacy implementations that delegate to service layer
        public List<ProductTag> GetAllProductTags()
        {
            throw new NotImplementedException("This method should be called from the service layer");
        }

        public ProductTag CreateProductTag(string displayTitle)
        {
            throw new NotImplementedException("This method should be called from the service layer");
        }
    }
}
