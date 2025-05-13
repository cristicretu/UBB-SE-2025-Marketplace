using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using Microsoft.Extensions.Configuration;
using MarketMinds.Shared.IRepository;

namespace MarketMinds.Shared.ProxyRepository
{
    public class BasketProxyRepository : IBasketRepository
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions jsonOptions;

        // Constructor with configuration
        public BasketProxyRepository(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            var apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }
            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/basket/");

            // Configure JSON options that match the server's configuration
            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            // Set longer timeout for HTTP requests
            httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public Basket GetBasketByUserId(int userId)
        {
            string fullUrl = $"{httpClient.BaseAddress.AbsoluteUri.Replace("api/basket/", "")}api/basket/user/{userId}";
            var response = httpClient.GetAsync(fullUrl).Result;
            response.EnsureSuccessStatusCode();

            var responseJson = response.Content.ReadAsStringAsync().Result;
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            options.Converters.Add(new MarketMinds.Shared.Services.UserJsonConverter());
            var basket = JsonSerializer.Deserialize<Basket>(responseJson, options) ?? new Basket { Id = userId, Items = new List<BasketItem>() };

            // Make sure we have an Items collection
            if (basket.Items == null)
            {
                basket.Items = new List<BasketItem>();
            }

            return basket;
        }

        public void RemoveItemByProductId(int basketId, int productId)
        {
            var response = httpClient.DeleteAsync($"{basketId}/product/{productId}").Result;
            response.EnsureSuccessStatusCode();
        }

        public List<BasketItem> GetBasketItems(int basketId)
        {
            var response = httpClient.GetAsync($"{basketId}/items").Result;
            response.EnsureSuccessStatusCode();

            var responseJson = response.Content.ReadAsStringAsync().Result;
            var items = JsonSerializer.Deserialize<List<BasketItem>>(responseJson, jsonOptions) ?? new List<BasketItem>();
            return items;
        }

        public void AddItemToBasket(int basketId, int productId, int quantity)
        {
            var response = httpClient.PostAsJsonAsync($"{basketId}/product/{productId}", quantity).Result;
            response.EnsureSuccessStatusCode();
        }

        public void UpdateItemQuantityByProductId(int basketId, int productId, int quantity)
        {
            var response = httpClient.PutAsJsonAsync($"{basketId}/product/{productId}", quantity).Result;
            response.EnsureSuccessStatusCode();
        }

        public void ClearBasket(int basketId)
        {
            var response = httpClient.DeleteAsync($"{basketId}/clear").Result;
            response.EnsureSuccessStatusCode();
        }

        // Support methods needed by BasketService
        internal HttpResponseMessage AddProductToBasketRaw(int userId, int productId, int quantity)
        {
            var response = httpClient.PostAsJsonAsync($"user/{userId}/product/{productId}", quantity).Result;
            response.EnsureSuccessStatusCode();
            return response;
        }

        internal string GetBasketByUserRaw(int userId)
        {
            string fullUrl = $"{httpClient.BaseAddress.AbsoluteUri.Replace("api/basket/", "")}api/basket/user/{userId}";
            var response = httpClient.GetAsync(fullUrl).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

        internal HttpResponseMessage RemoveProductFromBasketRaw(int userId, int productId)
        {
            var response = httpClient.DeleteAsync($"user/{userId}/product/{productId}").Result;
            response.EnsureSuccessStatusCode();
            return response;
        }

        internal HttpResponseMessage UpdateProductQuantityRaw(int userId, int productId, int quantity)
        {
            var response = httpClient.PutAsJsonAsync($"user/{userId}/product/{productId}", quantity).Result;
            response.EnsureSuccessStatusCode();
            return response;
        }

        internal HttpResponseMessage ClearBasketRaw(int userId)
        {
            var response = httpClient.DeleteAsync($"user/{userId}/clear").Result;
            response.EnsureSuccessStatusCode();
            return response;
        }

        internal string ValidateBasketBeforeCheckOutRaw(int basketId)
        {
            var response = httpClient.GetAsync($"{basketId}/validate").Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

        internal HttpResponseMessage ApplyPromoCodeRaw(int basketId, string code)
        {
            var response = httpClient.PostAsJsonAsync($"{basketId}/promocode", code).Result;
            return response;
        }

        internal string GetPromoCodeDiscountRaw(string code)
        {
            string normalizedCode = code.Trim().ToUpper();
            var response = httpClient.PostAsJsonAsync($"1/promocode", normalizedCode).Result;

            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStringAsync().Result;
            }

            return "{}";
        }

        internal string CalculateBasketTotalsRaw(int basketId, string promoCode)
        {
            string endpoint = $"{basketId}/totals";
            if (!string.IsNullOrEmpty(promoCode))
            {
                endpoint += $"?promoCode={promoCode}";
            }

            var response = httpClient.GetAsync(endpoint).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

        internal HttpResponseMessage DecreaseProductQuantityRaw(int userId, int productId)
        {
            var response = httpClient.PutAsync($"user/{userId}/product/{productId}/decrease", null).Result;
            response.EnsureSuccessStatusCode();
            return response;
        }

        internal HttpResponseMessage IncreaseProductQuantityRaw(int userId, int productId)
        {
            var response = httpClient.PutAsync($"user/{userId}/product/{productId}/increase", null).Result;
            response.EnsureSuccessStatusCode();
            return response;
        }

        internal async Task<HttpResponseMessage> CheckoutBasketRaw(int userId, int basketId, object requestData)
        {
            using var httpClientAccount = new HttpClient();
            httpClientAccount.BaseAddress = new Uri(httpClient.BaseAddress.AbsoluteUri.Replace("api/basket/", ""));
            httpClientAccount.Timeout = TimeSpan.FromSeconds(30);

            return await httpClientAccount.PostAsJsonAsync($"api/account/{userId}/orders", requestData);
        }
    }
}