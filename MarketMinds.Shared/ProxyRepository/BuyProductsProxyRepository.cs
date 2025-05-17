using System.Net.Http.Json;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Models.DTOs;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Shared.ProxyRepository
{
    public class BuyProductsProxyRepository
    {
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;

        public BuyProductsProxyRepository(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }
            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");
        }

        // Raw data access methods
        public string GetProducts()
        {
            if (httpClient == null || httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HTTP client is not properly initialized");
            }

            var response = httpClient.GetAsync("buyproducts").Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

        public string CreateListing(object productToSend)
        {
            var response = httpClient.PostAsJsonAsync("buyproducts", productToSend).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

        public void DeleteListing(int id)
        {
            var response = httpClient.DeleteAsync($"buyproducts/{id}").Result;
            response.EnsureSuccessStatusCode();
        }

        public string GetProductById(int id)
        {
            if (httpClient == null || httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HTTP client is not properly initialized");
            }

            var response = httpClient.GetAsync($"buyproducts/{id}").Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }




        // merge-nicusor

        /// <inheritdoc />
        public async Task<string> GetSellerNameAsync(int? sellerId)
        {
            if (!sellerId.HasValue)
            {
                return null;
            }

            var response = await this.httpClient.GetAsync($"{httpClient.BaseAddress}/seller/{sellerId.Value}/name");
            await this.ThrowOnError(nameof(GetSellerNameAsync), response);
            var sellerName = await response.Content.ReadAsStringAsync();
            return sellerName;
        }

        /// <inheritdoc />
        public async Task<List<Product>> GetBorrowableProductsAsync()
        {
            var response = await this.httpClient.GetAsync($"{httpClient.BaseAddress}/borrowable");
            await this.ThrowOnError(nameof(GetBorrowableProductsAsync), response);
            var products = await response.Content.ReadFromJsonAsync<List<Product>>();
            return products ?? new List<Product>();
        }

        /// <inheritdoc />
        public async Task UpdateProductAsync(int id, string name, double price, int sellerId, string productType, DateTime startDate, DateTime endDate)
        {
            var requestDto = new ProductRequest
            {
                Name = name,
                Price = price,
                SellerID = sellerId,
                ProductType = productType,
                StartDate = startDate,
                EndDate = endDate,
            };

            var response = await this.httpClient.PutAsJsonAsync($"{httpClient.BaseAddress}/{id}", requestDto);
            await this.ThrowOnError(nameof(UpdateProductAsync), response);
        }


        private async Task ThrowOnError(string methodName, HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = response.ReasonPhrase;
                }
                throw new Exception($"{methodName}: {errorMessage}");
            }
        }
    }
}
