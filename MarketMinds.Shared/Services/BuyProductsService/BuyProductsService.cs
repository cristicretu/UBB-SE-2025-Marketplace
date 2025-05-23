using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Models.DTOs;
using MarketMinds.Shared.Models.DTOs.Mappers;
using MarketMinds.Shared.ProxyRepository;
using MarketMinds.Shared.Services.Interfaces;

namespace MarketMinds.Shared.Services.BuyProductsService
{
    public class BuyProductsService : IBuyProductsService, IProductService
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

                // First deserialize to DTOs
                var productDTOs = JsonSerializer.Deserialize<List<BuyProductDTO>>(json, jsonOptions);

                // Then map DTOs to domain models
                var products = BuyProductMapper.FromDTOList(productDTOs);

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

        public BuyProduct? GetProductById(int id)
        {
            if (id <= 0)
            {
                Console.WriteLine($"GetProductById called with invalid ID: {id}");
                return null;
            }

            try
            {
                var json = buyProductsRepository.GetProductById(id);
                Console.WriteLine($"Received JSON for product {id} from server:");
                Console.WriteLine(json.Substring(0, Math.Min(500, json.Length)) + (json.Length > 500 ? "..." : string.Empty));

                if (string.IsNullOrEmpty(json) || json.Equals("null", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                var productDTO = JsonSerializer.Deserialize<BuyProductDTO>(json, jsonOptions);
                if (productDTO == null)
                {
                    return null;
                }

                var product = BuyProductMapper.FromDTO(productDTO);
                return product;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting product by ID {id}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return null;
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

        public async Task<Product> GetProductByIdAsync(int productId)
        {
            try
            {
                // Convert BuyProduct to Product when returning
                BuyProduct? buyProduct = GetProductById(productId);
                return buyProduct ?? throw new KeyNotFoundException($"Buy product with ID {productId} not found.");
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

        Task<string> IProductService.GetSellerNameAsync(int? sellerId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the stock quantity for a product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="newStockQuantity">New stock quantity</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task UpdateProductStockAsync(int productId, int newStockQuantity)
        {
            if (productId <= 0)
            {
                throw new ArgumentException("Product ID must be a positive number.", nameof(productId));
            }

            if (newStockQuantity < 0)
            {
                throw new ArgumentException("Stock quantity cannot be negative.", nameof(newStockQuantity));
            }

            try
            {
                // Get current product to check current stock
                var product = GetProductById(productId);

                // Calculate how much to decrease by
                int decreaseAmount = product.Stock - newStockQuantity;
                if (decreaseAmount < 0)
                {
                    // We're actually increasing stock
                    decreaseAmount = 0;
                }

                // Call the repository with the decrease amount
                await buyProductsRepository.UpdateProductStockAsync(productId, decreaseAmount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product stock: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Decreases the stock quantity for a product by the specified amount
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="decreaseAmount">Amount to decrease stock by</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task DecreaseProductStockAsync(int productId, int decreaseAmount)
        {
            if (productId <= 0)
            {
                throw new ArgumentException("Product ID must be a positive number.", nameof(productId));
            }

            if (decreaseAmount <= 0)
            {
                throw new ArgumentException("Decrease amount must be positive.", nameof(decreaseAmount));
            }

            try
            {
                // Directly call the repository with the decrease amount
                await buyProductsRepository.UpdateProductStockAsync(productId, decreaseAmount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decreasing product stock: {ex.Message}");
                throw;
            }
        }
    }
}
