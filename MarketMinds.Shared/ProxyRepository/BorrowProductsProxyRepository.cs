using System.Net.Http;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;

namespace MarketMinds.Shared.ProxyRepository
{
    public class BorrowProductsProxyRepository
    {
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;

        public BorrowProductsProxyRepository(IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null");
            }
            httpClient = new HttpClient();
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (string.IsNullOrEmpty(apiBaseUrl))
            {
                throw new InvalidOperationException("API base URL is null or empty");
            }
            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }
            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");
        }

        public void CreateListing(Product product)
        {
            BorrowProduct borrowProduct = product as BorrowProduct;
            
            var sellerId = borrowProduct.Seller?.Id ?? borrowProduct.SellerId;
            var conditionId = borrowProduct.Condition?.Id ?? borrowProduct.ConditionId;
            var categoryId = borrowProduct.Category?.Id ?? borrowProduct.CategoryId;
            
            var productToSend = new
            {
                borrowProduct.Title,
                borrowProduct.Description,
                SellerId = sellerId,
                ConditionId = conditionId,
                CategoryId = categoryId,
                borrowProduct.DailyRate,
                StartDate = borrowProduct.StartDate,
                EndDate = borrowProduct.EndDate,
                TimeLimit = borrowProduct.TimeLimit,
                borrowProduct.IsBorrowed,
                Images = borrowProduct.Images == null || !borrowProduct.Images.Any()
                       ? (borrowProduct.NonMappedImages != null && borrowProduct.NonMappedImages.Any()
                          ? borrowProduct.NonMappedImages.Select(img => new { Url = img.Url ?? string.Empty }).Cast<object>().ToList()
                          : new List<object>())
                       : borrowProduct.Images.Select(img => new { img.Url }).Cast<object>().ToList()
            };

            var serializerOptions = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            };
            
            var content = System.Net.Http.Json.JsonContent.Create(productToSend, null, serializerOptions);
            var response = httpClient.PostAsync("borrowproducts", content).Result;
            response.EnsureSuccessStatusCode();
        }

        public void AddImageToProduct(int productId, BorrowProductImage image)
        {
            var imageToSend = new
            {
                Url = image.Url,
                ProductId = productId
            };

            var serializerOptions = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            };
            
            var content = System.Net.Http.Json.JsonContent.Create(imageToSend, null, serializerOptions);
            var response = httpClient.PostAsync($"borrowproducts/{productId}/images", content).Result;
            response.EnsureSuccessStatusCode();
        }

        public void DeleteListing(Product product)
        {
            var response = httpClient.DeleteAsync($"borrowproducts/{product.Id}").Result;
            response.EnsureSuccessStatusCode();
        }

        public List<Product> GetProducts()
        {
            var serializerOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            serializerOptions.Converters.Add(new UserJsonConverter());

            var response = httpClient.GetAsync("borrowproducts").Result;
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;
        
            var products = System.Text.Json.JsonSerializer.Deserialize<List<BorrowProduct>>(json, serializerOptions);
            return products?.Cast<Product>().ToList() ?? new List<Product>();
        }

        public Product GetProductById(int id)
        {
            var serializerOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            serializerOptions.Converters.Add(new UserJsonConverter());

            var response = httpClient.GetAsync($"borrowproducts/{id}").Result;
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;
         
            var product = System.Text.Json.JsonSerializer.Deserialize<BorrowProduct>(json, serializerOptions);
            return product;
        }
    }
}