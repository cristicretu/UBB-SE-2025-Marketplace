using System.Net.Http.Json;
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
            
            Console.WriteLine($"TRACE: Repository.CreateListing received EndTime: {auctionProduct.EndTime}");
            
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
                throw new ArgumentException("SellerId must be specified and greater than zero", nameof(auctionProduct.SellerId));
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
                startAuctionDate = auctionProduct.StartTime,
                endAuctionDate = auctionProduct.EndTime,
                startingPrice = auctionProduct.StartPrice,
                currentPrice = auctionProduct.CurrentPrice,
                Images = imagesList
            };
            
            Console.WriteLine($"TRACE: About to send API request with endAuctionDate: {productToSend.endAuctionDate}");

            try
            {
                var response = httpClient.PostAsJsonAsync("auctionproducts", productToSend).Result;
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = response.Content.ReadAsStringAsync().Result;
                    throw new HttpRequestException($"Failed to create auction product. Status: {(int)response.StatusCode} {response.ReasonPhrase}. Error: {errorContent}");
                }
                
                var responseContent = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine($"TRACE: API response received: {responseContent}");
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
            
            Console.WriteLine($"TRACE: ProxyRepository.PlaceBid - Creating bid: ProductId={auction.Id}, BidderId={bidder.Id}, Amount={bidAmount}");
            
            var bidToSend = new
            {
                ProductId = auction.Id,
                BidderId = bidder.Id,
                Amount = bidAmount,
                Timestamp = DateTime.Now
            };
            
            try
            {
                Console.WriteLine($"TRACE: ProxyRepository.PlaceBid - Sending POST to auctionproducts/{auction.Id}/bids");
                var response = httpClient.PostAsJsonAsync($"auctionproducts/{auction.Id}/bids", bidToSend).Result;
                
                Console.WriteLine($"TRACE: ProxyRepository.PlaceBid - Response status: {(int)response.StatusCode} {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine($"TRACE: ProxyRepository.PlaceBid - Error response content: {errorContent}");
                    
                    // Check for specific user role error message
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest && 
                        errorContent.Contains("Your account doesn't have permission to place bids. Only buyer accounts can place bids."))
                    {
                        Console.WriteLine($"TRACE: ProxyRepository.PlaceBid - Detected buyer role error");
                        throw new InvalidOperationException("ERROR_NOT_BUYER: Your account doesn't have permission to place bids. Only buyer accounts can place bids.");
                    }
                    
                    var errorMessage = !string.IsNullOrWhiteSpace(errorContent) ? errorContent : "Unknown server error";
                    throw new Exception($"Server rejected bid: {errorMessage} (Status code: {(int)response.StatusCode})");
                }
                
                Console.WriteLine($"TRACE: ProxyRepository.PlaceBid - Bid placed successfully");
            }
            catch (InvalidOperationException)
            {
                // Pass through the user role error
                throw;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"TRACE: ProxyRepository.PlaceBid - Exception: {exception.GetType().Name}: {exception.Message}");
                if (exception.InnerException != null)
                {
                    Console.WriteLine($"TRACE: ProxyRepository.PlaceBid - Inner exception: {exception.InnerException.GetType().Name}: {exception.InnerException.Message}");
                }
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

            try
            {
                var serializerOptions = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                serializerOptions.Converters.Add(new AuctionProductJsonConverter());
                serializerOptions.Converters.Add(new UserJsonConverter());
                serializerOptions.Converters.Add(new CategoryJsonConverter());
                serializerOptions.Converters.Add(new ConditionJsonConverter());
                serializerOptions.Converters.Add(new SellerJsonConverter());
                serializerOptions.Converters.Add(new BidJsonConverter());

                var response = httpClient.GetAsync("auctionproducts").Result;
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = response.Content.ReadAsStringAsync().Result;
                    throw new HttpRequestException($"API returned error: {(int)response.StatusCode} {response.ReasonPhrase}. Details: {errorContent}");
                }
                
                var json = response.Content.ReadAsStringAsync().Result;
                
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<AuctionProduct>();
                }
                
                try
                {
                    var products = System.Text.Json.JsonSerializer.Deserialize<List<AuctionProduct>>(json, serializerOptions);
                    return products ?? new List<AuctionProduct>();
                }
                catch (System.Text.Json.JsonException jsonEx)
                {
                    throw new InvalidOperationException($"Failed to deserialize API response: {jsonEx.Message}", jsonEx);
                }
            }
            catch (HttpRequestException httpEx)
            {
                throw new InvalidOperationException($"Failed to communicate with API: {httpEx.Message}", httpEx);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error: {ex.Message}", ex);
            }
        }

        public AuctionProduct GetProductById(int id)
        {
            if (httpClient == null || httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HTTP client is not properly initialized");
            }

            try
            {
                var serializerOptions = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                serializerOptions.Converters.Add(new AuctionProductJsonConverter());
                serializerOptions.Converters.Add(new UserJsonConverter());
                serializerOptions.Converters.Add(new CategoryJsonConverter());
                serializerOptions.Converters.Add(new ConditionJsonConverter());
                serializerOptions.Converters.Add(new SellerJsonConverter());
                serializerOptions.Converters.Add(new BidJsonConverter());

                var response = httpClient.GetAsync($"auctionproducts/{id}").Result;
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = response.Content.ReadAsStringAsync().Result;
                    throw new HttpRequestException($"API returned error: {(int)response.StatusCode} {response.ReasonPhrase}. Details: {errorContent}");
                }
                
                var json = response.Content.ReadAsStringAsync().Result;
                
                if (string.IsNullOrWhiteSpace(json))
                {
                    throw new KeyNotFoundException($"No data returned for auction product with ID {id}");
                }
                
                try
                {
                    var product = System.Text.Json.JsonSerializer.Deserialize<AuctionProduct>(json, serializerOptions);
                    if (product == null)
                    {
                        throw new KeyNotFoundException($"Deserialized null product for ID {id}");
                    }
                    return product;
                }
                catch (System.Text.Json.JsonException jsonEx)
                {
                    throw new Exception($"Failed to deserialize auction product: {jsonEx.Message}. JSON: {json.Substring(0, Math.Min(100, json.Length))}...", jsonEx);
                }
            }
            catch (HttpRequestException httpEx)
            {
                throw new Exception($"HTTP request error: {httpEx.Message}", httpEx);
            }
            catch (KeyNotFoundException)
            {
                throw; // Rethrow key not found exception
            }
            catch (Exception ex)
            {
                if (ex is System.Text.Json.JsonException || ex.Message.Contains("deserialize"))
                {
                    throw; // Re-throw deserialization exceptions already handled
                }
                throw new Exception($"Error retrieving auction product with ID {id}: {ex.Message}", ex);
            }
        }
    }
}
