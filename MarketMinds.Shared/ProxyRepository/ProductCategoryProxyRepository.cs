using Microsoft.Extensions.Configuration;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;

namespace MarketMinds.Shared.ProxyRepository
{
    public class ProductCategoryProxyRepository : IProductCategoryRepository
    {
        private readonly HttpClient httpClient;

        public ProductCategoryProxyRepository(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            var apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }
            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");
        }

        public string GetAllProductCategoriesRaw()
        {
            var response = httpClient.GetAsync("ProductCategory").Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

        public string CreateProductCategoryRaw(string name, string description)
        {
            var requestContent = new StringContent(
                $"{{\"DisplayTitle\":\"{name}\",\"Description\":\"{description}\"}}",
                System.Text.Encoding.UTF8,
                "application/json");
            var response = httpClient.PostAsync("ProductCategory", requestContent).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

        public void DeleteProductCategoryRaw(string name)
        {
            var response = httpClient.DeleteAsync($"ProductCategory/{name}").Result;
            response.EnsureSuccessStatusCode();
        }

        // Legacy implementations that delegate to service layer
        public List<Category> GetAllProductCategories()
        {
            throw new NotImplementedException("This method should be called from the service layer");
        }

        public Category CreateProductCategory(string name, string description)
        {
            throw new NotImplementedException("This method should be called from the service layer");
        }

        public void DeleteProductCategory(string name)
        {
            throw new NotImplementedException("This method should be called from the service layer");
        }
    }
}