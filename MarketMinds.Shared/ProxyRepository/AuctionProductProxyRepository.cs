using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;

namespace MarketMinds.Shared.ProxyRepository
{
    public class AuctionProductsProxyRepository
    {
        private readonly HttpClient httpClient;
        private readonly string apiBaseUrl;

        public AuctionProductsProxyRepository(IConfiguration configuration)
        {
            httpClient = new HttpClient();
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            if (!apiBaseUrl.EndsWith("/"))
            {
                apiBaseUrl += "/";
            }
            httpClient.BaseAddress = new Uri(apiBaseUrl + "api/");
        }

        public void CreateListing(Product product)
        {
            if (!(product is AuctionProduct auctionProduct))
            {
                throw new ArgumentException("Product must be an AuctionProduct.", nameof(product));
            }
            
            if (string.IsNullOrWhiteSpace(auctionProduct.Title))
            {
                throw new ArgumentException("Title cannot be empty", nameof(auctionProduct.Title));
            }

            if (auctionProduct.CategoryId <= 0)
            {
                throw new ArgumentException("CategoryId must be specified", nameof(auctionProduct.CategoryId));
            }

            if (auctionProduct.ConditionId <= 0)
            {
                throw new ArgumentException("ConditionId must be specified", nameof(auctionProduct.ConditionId));
            }

            if (auctionProduct.StartPrice <= 0)
            {
                throw new ArgumentException("StartPrice must be greater than zero", nameof(auctionProduct.StartPrice));
            }

            // Ensure we have a valid SellerId
            if (auctionProduct.SellerId <= 0)
            {
                // For demo purposes, use a default SellerId if none provided
                auctionProduct.SellerId = 1;
            }
            
            // Filter out any null URLs in images
            var imagesList = new List<object>();
            if (auctionProduct.NonMappedImages != null && auctionProduct.NonMappedImages.Any())
            {
                imagesList = auctionProduct.NonMappedImages
                    .Where(img => !string.IsNullOrWhiteSpace(img.Url))
                    .Select(img => new { Url = img.Url })
                    .Cast<object>()
                    .ToList();
            }
            else if (auctionProduct.Images != null && auctionProduct.Images.Any())
            {
                imagesList = auctionProduct.Images
                    .Where(img => !string.IsNullOrWhiteSpace(img.Url))
                    .Select(img => new { Url = img.Url })
                    .Cast<object>()
                    .ToList();
            }
            
            var productToSend = new
            {
                auctionProduct.Title,
                Description = auctionProduct.Description ?? string.Empty,
                SellerId = auctionProduct.SellerId,
                ConditionId = auctionProduct.ConditionId,
                CategoryId = auctionProduct.CategoryId,
                StartTime = auctionProduct.StartTime,
                EndTime = auctionProduct.EndTime,
                StartPrice = auctionProduct.StartPrice,
                CurrentPrice = auctionProduct.CurrentPrice,
                Images = imagesList
            };

            try
            {
                var response = httpClient.PostAsJsonAsync("auctionproducts", productToSend).Result;
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = response.Content.ReadAsStringAsync().Result;
                    throw new HttpRequestException($"Failed to create auction product. Status: {(int)response.StatusCode} {response.ReasonPhrase}. Error: {errorContent}");
                }
                
                var responseContent = response.Content.ReadAsStringAsync().Result;
            }
            catch (HttpRequestException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating auction product: {ex.Message}", ex);
            }
        }

        public void PlaceBid(AuctionProduct auction, User bidder, double bidAmount)
        {
            if (httpClient == null || httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HTTP client is not properly initialized");
            }
            
            var bidToSend = new
            {
                ProductId = auction.Id,
                BidderId = bidder.Id,
                Amount = bidAmount,
                Timestamp = DateTime.Now
            };
            
            try
            {
                var response = httpClient.PostAsJsonAsync($"auctionproducts/{auction.Id}/bids", bidToSend).Result;
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = response.Content.ReadAsStringAsync().Result;
                    var errorMessage = !string.IsNullOrWhiteSpace(errorContent) ? errorContent : "Unknown server error";
                    throw new Exception($"Server rejected bid: {errorMessage} (Status code: {(int)response.StatusCode})");
                }
            }
            catch (Exception exception)
            {
                throw new Exception($"Failed to place bid: {exception.Message}", exception);
            }
        }

        public void ConcludeAuction(AuctionProduct auction)
        {
            var response = httpClient.DeleteAsync($"auctionproducts/{auction.Id}").Result;
        }

        public List<AuctionProduct> GetProducts()
        {
            if (httpClient == null || httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HTTP client is not properly initialized");
            }

            var serializerOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            serializerOptions.Converters.Add(new UserJsonConverter());

            var response = httpClient.GetAsync("auctionproducts").Result;
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;
            var products = System.Text.Json.JsonSerializer.Deserialize<List<AuctionProduct>>(json, serializerOptions);
            return products ?? new List<AuctionProduct>();
        }

        public AuctionProduct GetProductById(int id)
        {
            if (httpClient == null || httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HTTP client is not properly initialized");
            }

            var serializerOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            serializerOptions.Converters.Add(new UserJsonConverter());

            var response = httpClient.GetAsync($"auctionproducts/{id}").Result;
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;
            var product = System.Text.Json.JsonSerializer.Deserialize<AuctionProduct>(json, serializerOptions);
            return product;
        }
    }
}
