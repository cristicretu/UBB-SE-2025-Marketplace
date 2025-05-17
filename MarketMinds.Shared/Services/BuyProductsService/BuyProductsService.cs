using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.ProxyRepository;
using MarketMinds.Shared.Services.Interfaces;

namespace MarketMinds.Shared.Services.BuyProductsService
{
    public class BuyProductsService : IBuyProductsService
    {
        private readonly BuyProductsProxyRepository buyProductsRepository;
        private readonly JsonSerializerOptions jsonOptions;
        private const int NOCOUNT = 0;

        public BuyProductsService(BuyProductsProxyRepository buyProductsRepository)
        {
            this.buyProductsRepository = buyProductsRepository;

            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            jsonOptions.Converters.Add(new UserJsonConverter());
        }

        public List<BuyProduct> GetProducts()
        {
            try
            {
                var json = buyProductsRepository.GetProducts();
                Console.WriteLine("Received JSON from server:");
                Console.WriteLine(json.Substring(0, Math.Min(500, json.Length)) + (json.Length > 500 ? "..." : string.Empty));

                var products = JsonSerializer.Deserialize<List<BuyProduct>>(json, jsonOptions);
                return products ?? new List<BuyProduct>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting products: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return new List<BuyProduct>();
            }
        }

        public void CreateListing(BuyProduct product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product), "Product cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(product.Title))
            {
                throw new ArgumentException("Product title is required.", nameof(product.Title));
            }

            if (product.Price < 0)
            {
                throw new ArgumentException("Price cannot be negative.", nameof(product.Price));
            }

            if (product.SellerId <= 0)
            {
                throw new ArgumentException("Valid seller ID is required.", nameof(product.SellerId));
            }

            Console.WriteLine($"Creating buy product with SellerId={product.SellerId}, " +
                             $"Seller object: {(product.Seller != null ? $"Id={product.Seller.Id}" : "null")}");

            try
            {
                var productToSend = new
                {
                    product.Title,
                    product.Description,
                    SellerId = product.SellerId,
                    ConditionId = product.Condition?.Id,
                    CategoryId = product.Category?.Id,
                    product.Price,
                    Images = product.Images == null || !product.Images.Any()
                           ? (product.NonMappedImages != null && product.NonMappedImages.Any()
                              ? product.NonMappedImages.Select(img => new { Url = img.Url ?? string.Empty }).Cast<object>().ToList()
                              : new List<object>())
                           : product.Images.Select(img => new { img.Url }).Cast<object>().ToList()
                };

                Console.WriteLine($"Sending to API: SellerId={productToSend.SellerId}");
                var responseJson = buyProductsRepository.CreateListing(productToSend);
                // Could deserialize response if needed
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error creating product: {ex.Message}");
                throw;
            }
        }

        public void DeleteListing(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product), "Product cannot be null.");
            }

            if (product.Id == 0)
            {
                throw new ArgumentException("Product ID must be set for delete.", nameof(product.Id));
            }

            try
            {
                buyProductsRepository.DeleteListing(product.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting product: {ex.Message}");
                throw;
            }
        }

        public BuyProduct GetProductById(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID must be a positive number.", nameof(id));
            }

            try
            {
                var json = buyProductsRepository.GetProductById(id);
                Console.WriteLine($"Received JSON for product {id} from server:");
                Console.WriteLine(json.Substring(0, Math.Min(500, json.Length)) + (json.Length > 500 ? "..." : string.Empty));

                var product = JsonSerializer.Deserialize<BuyProduct>(json, jsonOptions);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Buy product with ID {id} not found.");
                }
                return product;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting product by ID {id}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw new KeyNotFoundException($"Buy product with ID {id} not found: {ex.Message}");
            }
        }

        public List<Product> GetSortedFilteredProducts(List<Condition> selectedConditions, List<Category> selectedCategories, List<ProductTag> selectedTags, ProductSortType sortCondition, string searchQuery)
        {
            List<BuyProduct> buyProducts = GetProducts();
            List<Product> productResultSet = new List<Product>();
            foreach (BuyProduct product in buyProducts)
            {
                bool matchesConditions = selectedConditions == null || selectedConditions.Count == NOCOUNT || selectedConditions.Any(c => c.Id == product.Condition.Id);
                bool matchesCategories = selectedCategories == null || selectedCategories.Count == NOCOUNT || selectedCategories.Any(c => c.Id == product.Category.Id);
                bool matchesTags = selectedTags == null || selectedTags.Count == NOCOUNT || selectedTags.Any(t => product.Tags.Any(pt => pt.Id == t.Id));
                bool matchesSearchQuery = string.IsNullOrEmpty(searchQuery) || product.Title.ToLower().Contains(searchQuery.ToLower());

                if (matchesConditions && matchesCategories && matchesTags && matchesSearchQuery)
                {
                    productResultSet.Add(product);
                }
            }

            if (sortCondition != null)
            {
                if (sortCondition.IsAscending)
                {
                    productResultSet = productResultSet.OrderBy(
                        p =>
                        {
                            var prop = p?.GetType().GetProperty(sortCondition.InternalAttributeFieldTitle);
                            return prop?.GetValue(p);
                        }).ToList();
                }
                else
                {
                    productResultSet = productResultSet.OrderByDescending(
                        p =>
                        {
                            var prop = p?.GetType().GetProperty(sortCondition.InternalAttributeFieldTitle);
                            return prop?.GetValue(p);
                        }).ToList();
                }
            }
            return productResultSet;
        }

        // merge-nicusor

        public Task UpdateProductAsync(int id, string name, double price, int sellerId, string productType, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException("UpdateProductAsync is not implemented.");
        }

        public async Task<BuyProduct> GetProductByIdAsync(int productId)
        {
            try
            {
                return GetProductById(productId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting product by ID {productId}: {ex.Message}");
                return null;
            }
        }
        
        // Explicit implementation for IProductService's GetSellerNameAsync
        public Task<string> GetSellerNameAsync(int sellerId)
        {
            return this.buyProductsRepository.GetSellerNameAsync(sellerId);
        }
        
        /// <summary>
        /// Gets a list of products that can be borrowed.
        /// Implementation for IProductService interface.
        /// </summary>
        /// <returns>A list of borrowable products.</returns>
        public async Task<List<Product>> GetBorrowableProductsAsync()
        {
            // Since this is BuyProductsService, we don't have borrowable products
            // Return an empty list when this method is called on this service
            return new List<Product>();
        }
    }
}
