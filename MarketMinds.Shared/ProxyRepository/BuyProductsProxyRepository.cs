using System.Net.Http.Json;
using Azure;
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
            apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5001";
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

        public string GetProducts(int offset, int count)
        {
            if (httpClient == null || httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HTTP client is not properly initialized");
            }

            string url = $"buyproducts?offset={offset}&count={count}";
            var response = httpClient.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

        public int GetProductCount()
        {
            if (httpClient == null || httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HTTP client is not properly initialized");
            }

            var response = httpClient.GetAsync("buyproducts/count").Result;
            response.EnsureSuccessStatusCode();
            var countString = response.Content.ReadAsStringAsync().Result;
            return int.Parse(countString);
        }

        public string GetFilteredProducts(int offset, int count, List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null)
        {
            if (httpClient == null || httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HTTP client is not properly initialized");
            }

            var queryParams = new List<string>
            {
                $"offset={offset}",
                $"count={count}"
            };

            if (conditionIds != null && conditionIds.Any())
            {
                foreach (var conditionId in conditionIds)
                {
                    queryParams.Add($"conditionIds={conditionId}");
                }
            }

            if (categoryIds != null && categoryIds.Any())
            {
                foreach (var categoryId in categoryIds)
                {
                    queryParams.Add($"categoryIds={categoryId}");
                }
            }

            if (maxPrice.HasValue)
            {
                queryParams.Add($"maxPrice={maxPrice.Value}");
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
            }

            string url = $"buyproducts/filtered?{string.Join("&", queryParams)}";
            var response = httpClient.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

        public int GetFilteredProductCount(List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null)
        {
            if (httpClient == null || httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HTTP client is not properly initialized");
            }

            var queryParams = new List<string>();

            if (conditionIds != null && conditionIds.Any())
            {
                foreach (var conditionId in conditionIds)
                {
                    queryParams.Add($"conditionIds={conditionId}");
                }
            }

            if (categoryIds != null && categoryIds.Any())
            {
                foreach (var categoryId in categoryIds)
                {
                    queryParams.Add($"categoryIds={categoryId}");
                }
            }

            if (maxPrice.HasValue)
            {
                queryParams.Add($"maxPrice={maxPrice.Value}");
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
            }

            string url = queryParams.Any()
                ? $"buyproducts/filtered/count?{string.Join("&", queryParams)}"
                : "buyproducts/filtered/count";

            var response = httpClient.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            var countString = response.Content.ReadAsStringAsync().Result;
            return int.Parse(countString);
        }

        public string GetFilteredProducts(int offset, int count, List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null, int? sellerId = null)
        {
            if (httpClient == null || httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HTTP client is not properly initialized");
            }

            var queryParams = new List<string>
            {
                $"offset={offset}",
                $"count={count}"
            };

            if (conditionIds != null && conditionIds.Any())
            {
                foreach (var conditionId in conditionIds)
                {
                    queryParams.Add($"conditionIds={conditionId}");
                }
            }

            if (categoryIds != null && categoryIds.Any())
            {
                foreach (var categoryId in categoryIds)
                {
                    queryParams.Add($"categoryIds={categoryId}");
                }
            }

            if (maxPrice.HasValue)
            {
                queryParams.Add($"maxPrice={maxPrice.Value}");
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
            }

            if (sellerId.HasValue)
            {
                queryParams.Add($"sellerId={sellerId.Value}");
            }

            string url = $"buyproducts/filtered/seller?{string.Join("&", queryParams)}";
            var response = httpClient.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

        public int GetFilteredProductCount(List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null, int? sellerId = null)
        {
            if (httpClient == null || httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HTTP client is not properly initialized");
            }

            var queryParams = new List<string>();

            if (conditionIds != null && conditionIds.Any())
            {
                foreach (var conditionId in conditionIds)
                {
                    queryParams.Add($"conditionIds={conditionId}");
                }
            }

            if (categoryIds != null && categoryIds.Any())
            {
                foreach (var categoryId in categoryIds)
                {
                    queryParams.Add($"categoryIds={categoryId}");
                }
            }

            if (maxPrice.HasValue)
            {
                queryParams.Add($"maxPrice={maxPrice.Value}");
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
            }

            if (sellerId.HasValue)
            {
                queryParams.Add($"sellerId={sellerId.Value}");
            }

            string url = queryParams.Any()
                ? $"buyproducts/filtered/count/seller?{string.Join("&", queryParams)}"
                : "buyproducts/filtered/count/seller";

            var response = httpClient.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            var countString = response.Content.ReadAsStringAsync().Result;
            return int.Parse(countString);
        }

        public string CreateListing(object productToSend)
        {
            if (httpClient == null || httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HTTP client is not properly initialized");
            }

            try
            {
                Console.WriteLine($"Sending POST request to create product at URL: {httpClient.BaseAddress}buyproducts");
                
                var response = httpClient.PostAsJsonAsync("buyproducts", productToSend).Result;
                
                Console.WriteLine($"Response status: {(int)response.StatusCode} {response.ReasonPhrase}");
                
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorContent = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine($"400 Bad Request Error Details: {errorContent}");
                    
                    // Try to parse ModelState errors if present
                    try
                    {
                        var errorResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(errorContent);
                        if (errorResponse.ContainsKey("errors"))
                        {
                            Console.WriteLine("Validation errors found in response:");
                            var errorsJson = errorResponse["errors"].ToString();
                            Console.WriteLine(errorsJson);
                        }
                    }
                    catch
                    {
                        // If we can't parse as JSON, just log the raw content
                        Console.WriteLine($"Raw error content: {errorContent}");
                    }
                    
                    throw new HttpRequestException($"Bad Request (400): {errorContent}");
                }
                
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Request failed: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in CreateListing: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public void DeleteListing(int id)
        {
            var response = httpClient.DeleteAsync($"buyproducts/{id}").Result;
            response.EnsureSuccessStatusCode();
        }

        public async Task<string> GetProductByIdAsync(int id)
        {
            if (httpClient == null || httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HTTP client is not properly initialized");
            }

            var response = await httpClient.GetAsync($"buyproducts/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
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

        /// <summary>
        /// Updates the stock quantity for a product by decreasing it by the specified amount
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="decreaseAmount">Amount to decrease from current stock</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task UpdateProductStockAsync(int id, int newStockQuantity)
        {
            if (httpClient == null || httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HTTP client is not properly initialized");
            }

            try
            {
                // Create simple request with just the quantity to decrease
                var stockUpdateRequest = new { Quantity = newStockQuantity };

                // Use the dedicated stock update endpoint
                string url = $"buyproducts/stock/{id}";
                Console.WriteLine($"Sending POST request to update product {id} stock with {newStockQuantity}");

                var response = await httpClient.PostAsJsonAsync(url, stockUpdateRequest);

                // Log the response status
                Console.WriteLine($"Stock update response status: {(int)response.StatusCode} {response.ReasonPhrase}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Stock update failed: {errorContent}");
                    throw new HttpRequestException($"Stock update failed: {response.StatusCode} - {errorContent}");
                }

                await ThrowOnError(nameof(UpdateProductStockAsync), response);
                Console.WriteLine($"Stock updated successfully for product {id} with {newStockQuantity}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product stock: {ex.Message}");
                throw;
            }
        }

        public async Task<double> GetMaxPriceAsync()
        {
            if (httpClient == null || httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HTTP client is not properly initialized");
            }

            try
            {
                var response = await httpClient.GetAsync("buyproducts/maxprice");
                response.EnsureSuccessStatusCode();
                var priceString = await response.Content.ReadAsStringAsync();
                return double.Parse(priceString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting max price for buy products: {ex.Message}");
                return 0.0; // Return 0 on error
            }
        }

        private async Task ThrowOnError(string methodName, HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"{methodName} failed: {response.StatusCode} - {errorContent}");
            }
        }
    }
}
