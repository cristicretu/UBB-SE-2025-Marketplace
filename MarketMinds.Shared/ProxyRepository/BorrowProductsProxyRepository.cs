using System.Text.Json.Serialization;
using System.Linq;
using Microsoft.Extensions.Configuration;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.IRepository;

namespace MarketMinds.Shared.ProxyRepository
{
    public class BorrowProductsProxyRepository : IBorrowProductsRepository
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

            // Process tags for sending to API
            var tagsToSend = new List<object>();
            if (borrowProduct.Tags != null && borrowProduct.Tags.Any())
            {
                tagsToSend = borrowProduct.Tags.Select(tag => new { id = tag.Id, title = tag.Title }).Cast<object>().ToList();
            }

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
                Tags = tagsToSend,
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

        public Product GetProductByID(int id)
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

        List<BorrowProduct> IBorrowProductsRepository.GetProducts()
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
            return products ?? new List<BorrowProduct>();
        }

        List<BorrowProduct> IBorrowProductsRepository.GetProducts(int offset, int count)
        {
            var serializerOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            serializerOptions.Converters.Add(new UserJsonConverter());

            string url = $"borrowproducts?offset={offset}&count={count}";
            var response = httpClient.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;

            var products = System.Text.Json.JsonSerializer.Deserialize<List<BorrowProduct>>(json, serializerOptions);
            return products ?? new List<BorrowProduct>();
        }

        int IBorrowProductsRepository.GetProductCount()
        {
            var response = httpClient.GetAsync("borrowproducts/count").Result;
            response.EnsureSuccessStatusCode();
            var countString = response.Content.ReadAsStringAsync().Result;
            return int.Parse(countString);
        }

        void IBorrowProductsRepository.DeleteProduct(BorrowProduct product)
        {
            var response = httpClient.DeleteAsync($"borrowproducts/{product.Id}").Result;
            response.EnsureSuccessStatusCode();
        }

        void IBorrowProductsRepository.AddProduct(BorrowProduct product)
        {
            var serializerOptions = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            };

            var content = System.Net.Http.Json.JsonContent.Create(product, null, serializerOptions);
            var response = httpClient.PostAsync("borrowproducts", content).Result;
            response.EnsureSuccessStatusCode();
        }

        void IBorrowProductsRepository.UpdateProduct(BorrowProduct product)
        {
            var sellerId = product.Seller?.Id ?? product.SellerId;
            var conditionId = product.Condition?.Id ?? product.ConditionId;
            var categoryId = product.Category?.Id ?? product.CategoryId;

            var productToSend = new
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.Description,
                SellerId = sellerId,
                ConditionId = conditionId,
                CategoryId = categoryId,
                DailyRate = product.DailyRate,
                StartDate = product.StartDate,
                EndDate = product.EndDate,
                TimeLimit = product.TimeLimit,
                IsBorrowed = product.IsBorrowed,
                BorrowerId = product.BorrowerId
            };

            var serializerOptions = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            };

            var content = System.Net.Http.Json.JsonContent.Create(productToSend, null, serializerOptions);
            var response = httpClient.PutAsync($"borrowproducts/{product.Id}", content).Result;
            response.EnsureSuccessStatusCode();
        }

        BorrowProduct IBorrowProductsRepository.GetProductByID(int id)
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

        void IBorrowProductsRepository.AddImageToProduct(int productId, BorrowProductImage image)
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

        /// <summary>
        /// Attempts to borrow a product for a user by hitting the 
        /// POST api/waitlist/{productId}/borrow/user/{userId}?startDate=&endDate=
        /// </summary>
        public async Task BorrowProductAsync(int userId, int productId, DateTime start, DateTime end)
        {
            // Format both dates as ISO-8601 and URL-encode
            var startIso = Uri.EscapeDataString(start.ToString("o"));
            var endIso = Uri.EscapeDataString(end.ToString("o"));

            // Relative to BaseAddress which you configured as ".../api/"
            var url = $"borrowproducts/{productId}/borrow/user/{userId}"
                    + $"?startDate={startIso}&endDate={endIso}";

            var response = await this.httpClient.PostAsync(url, null);
            await ThrowOnError(nameof(BorrowProductAsync), response);
        }

        public async Task<List<BorrowProduct>> GetAllBorrowProductsAsync()
        {
            try
            {
                var serializerOptions = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                serializerOptions.Converters.Add(new UserJsonConverter());

                var response = await httpClient.GetAsync("borrowproducts");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();

                var products = System.Text.Json.JsonSerializer.Deserialize<List<BorrowProduct>>(json, serializerOptions);
                return products ?? new List<BorrowProduct>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting all borrow products: {ex.Message}", ex);
            }
        }

        public async Task<List<BorrowProduct>> GetAllBorrowProductsAsync(int offset, int count)
        {
            try
            {
                var serializerOptions = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                serializerOptions.Converters.Add(new UserJsonConverter());

                string url = $"borrowproducts?offset={offset}&count={count}";
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();

                var products = System.Text.Json.JsonSerializer.Deserialize<List<BorrowProduct>>(json, serializerOptions);
                return products ?? new List<BorrowProduct>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting borrow products with pagination (offset: {offset}, count: {count}): {ex.Message}", ex);
            }
        }

        public async Task<int> GetBorrowProductCountAsync()
        {
            try
            {
                var response = await httpClient.GetAsync("borrowproducts/count");
                response.EnsureSuccessStatusCode();
                var countString = await response.Content.ReadAsStringAsync();
                return int.Parse(countString);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting borrow product count: {ex.Message}", ex);
            }
        }

        private async Task ThrowOnError(string methodName, HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error in {methodName}: {response.StatusCode} - {errorContent}");
            }
        }

    }
}