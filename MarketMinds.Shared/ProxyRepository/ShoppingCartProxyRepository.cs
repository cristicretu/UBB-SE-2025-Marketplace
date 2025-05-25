using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;

namespace MarketMinds.Shared.ProxyRepository
{
    public class ShoppingCartProxyRepository : IShoppingCartRepository
    {
        private const string ApiBaseRoute = "api/shoppingcart";
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions jsonOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShoppingCartProxyRepository"/> class with a base API URL.
        /// </summary>
        /// <param name="baseApiUrl">The base URL of the API.</param>
        public ShoppingCartProxyRepository(string baseApiUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new System.Uri(baseApiUrl);

            // Configure JSON options
            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            // Add a converter to handle Product deserialization
            jsonOptions.Converters.Add(new JsonStringEnumConverter());
            jsonOptions.Converters.Add(new ProductJsonConverter());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShoppingCartProxyRepository"/> class with configuration.
        /// </summary>
        /// <param name="configuration">The configuration containing API settings.</param>
        public ShoppingCartProxyRepository(IConfiguration configuration)
        {
            this.httpClient = new HttpClient();
            var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5001";
            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }
            this.httpClient.BaseAddress = new System.Uri(baseUrl + "api/");

            // Configure JSON options
            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            // Add a converter to handle Product deserialization
            jsonOptions.Converters.Add(new JsonStringEnumConverter());
            jsonOptions.Converters.Add(new ProductJsonConverter());
        }

        public async Task AddProductToCartAsync(int buyerId, int productId, int quantity)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/items?productId={productId}&quantity={quantity}";
            var response = await this.httpClient.PostAsync(requestUri, null);
            
            await this.ThrowOnError(nameof(AddProductToCartAsync), response);
        }

        public async Task ClearCartAsync(int buyerId)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/items";
            var response = await this.httpClient.DeleteAsync(requestUri);
            await this.ThrowOnError(nameof(ClearCartAsync), response);
        }

        public async Task<int> GetCartItemCountAsync(int buyerId)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/items/count";
            var response = await this.httpClient.GetAsync(requestUri);
            await this.ThrowOnError(nameof(GetCartItemCountAsync), response);
            return await response.Content.ReadFromJsonAsync<int>();
        }

        public async Task<List<Product>> GetCartItemsAsync(int buyerId)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/items";
            var response = await this.httpClient.GetAsync(requestUri);
            await this.ThrowOnError(nameof(GetCartItemsAsync), response);

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<Product>>(json, jsonOptions);
            return result ?? new List<Product>();
        }

        public async Task<int> GetProductQuantityAsync(int buyerId, int productId)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/items/{productId}/quantity";
            var response = await this.httpClient.GetAsync(requestUri);
            await this.ThrowOnError(nameof(GetProductQuantityAsync), response);
            return await response.Content.ReadFromJsonAsync<int>();
        }

        public async Task<bool> IsProductInCartAsync(int buyerId, int productId)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/items/{productId}/exists";
            var response = await this.httpClient.GetAsync(requestUri);
            await this.ThrowOnError(nameof(IsProductInCartAsync), response);
            return await response.Content.ReadFromJsonAsync<bool>();
        }

        public async Task RemoveProductFromCartAsync(int buyerId, int productId)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/items/{productId}";
            var response = await this.httpClient.DeleteAsync(requestUri);
            await this.ThrowOnError(nameof(RemoveProductFromCartAsync), response);
        }

        public async Task UpdateProductQuantityAsync(int buyerId, int productId, int quantity)
        {
            var requestUri = $"{ApiBaseRoute}/{buyerId}/items/{productId}?quantity={quantity}";
            var response = await this.httpClient.PutAsync(requestUri, null);
            await this.ThrowOnError(nameof(UpdateProductQuantityAsync), response);
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

    /// <summary>
    /// JSON converter for handling Product deserialization
    /// </summary>
    public class ProductJsonConverter : JsonConverter<Product>
    {
        public override Product Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Create a BuyProduct instance since that's what we expect from the server
            return JsonSerializer.Deserialize<BuyProduct>(ref reader, options) ??
                throw new JsonException("Failed to deserialize BuyProduct");
        }

        public override void Write(Utf8JsonWriter writer, Product value, JsonSerializerOptions options)
        {
            // Serialize as BuyProduct since that's what we're using
            if (value is BuyProduct buyProduct)
            {
                JsonSerializer.Serialize(writer, buyProduct, options);
            }
            else
            {
                throw new JsonException($"Cannot serialize {value.GetType()} as Product");
            }
        }
    }
}
