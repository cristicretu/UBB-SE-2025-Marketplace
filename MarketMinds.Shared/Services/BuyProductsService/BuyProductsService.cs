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

        public List<BuyProduct> GetProducts(int offset, int count)
        {
            try
            {
                var json = buyProductsRepository.GetProducts(offset, count);
                Console.WriteLine($"Received JSON from server (offset: {offset}, count: {count}):");
                Console.WriteLine(json.Substring(0, Math.Min(500, json.Length)) + (json.Length > 500 ? "..." : string.Empty));

                // First deserialize to DTOs
                var productDTOs = JsonSerializer.Deserialize<List<BuyProductDTO>>(json, jsonOptions);

                // Then map DTOs to domain models
                var products = BuyProductMapper.FromDTOList(productDTOs);

                return products ?? new List<BuyProduct>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting products with pagination: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return new List<BuyProduct>();
            }
        }

        public int GetProductCount()
        {
            try
            {
                return buyProductsRepository.GetProductCount();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting product count: {ex.Message}");
                return 0;
            }
        }

        public List<BuyProduct> GetFilteredProducts(int offset, int count, List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null)
        {
            try
            {
                var json = buyProductsRepository.GetFilteredProducts(offset, count, conditionIds, categoryIds, maxPrice, searchTerm);
                Console.WriteLine($"Received filtered JSON from server (offset: {offset}, count: {count}, conditions: {string.Join(",", conditionIds ?? new List<int>())}, categories: {string.Join(",", categoryIds ?? new List<int>())}, maxPrice: {maxPrice}, searchTerm: {searchTerm}):");
                Console.WriteLine(json.Substring(0, Math.Min(500, json.Length)) + (json.Length > 500 ? "..." : string.Empty));

                // First deserialize to DTOs
                var productDTOs = JsonSerializer.Deserialize<List<BuyProductDTO>>(json, jsonOptions);

                // Then map DTOs to domain models
                var products = BuyProductMapper.FromDTOList(productDTOs);

                return products ?? new List<BuyProduct>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting filtered products: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return new List<BuyProduct>();
            }
        }

        public int GetFilteredProductCount(List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null)
        {
            try
            {
                return buyProductsRepository.GetFilteredProductCount(conditionIds, categoryIds, maxPrice, searchTerm);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting filtered product count: {ex.Message}");
                return 0;
            }
        }

        public void CreateListing(BuyProduct product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product), "Product cannot be null.");
            }

            // Comprehensive validation with detailed error messages
            var validationErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(product.Title))
            {
                validationErrors.Add("Product title is required and cannot be empty.");
            }

            if (product.SellerId <= 0)
            {
                validationErrors.Add("Valid seller ID is required (must be > 0).");
            }

            if (product.Price < 0)
            {
                validationErrors.Add("Price cannot be negative.");
            }

            if (product.ConditionId <= 0 && (product.Condition == null || product.Condition.Id <= 0))
            {
                validationErrors.Add("Valid condition ID is required (must be > 0).");
            }

            if (product.CategoryId <= 0 && (product.Category == null || product.Category.Id <= 0))
            {
                validationErrors.Add("Valid category ID is required (must be > 0).");
            }

            if (product.Stock < 0)
            {
                validationErrors.Add("Stock quantity cannot be negative.");
            }

            if (validationErrors.Any())
            {
                var errorMessage = string.Join(" ", validationErrors);
                Console.WriteLine($"CreateListing validation failed: {errorMessage}");
                throw new ArgumentException(errorMessage);
            }

            Console.WriteLine($"Creating buy product with SellerId={product.SellerId}, " +
                             $"CategoryId={product.CategoryId}, ConditionId={product.ConditionId}, " +
                             $"Title='{product.Title}', Price={product.Price}, Stock={product.Stock}");

            try
            {
                // Create a clean product without tags initially to avoid validation issues
                var productToSend = new BuyProduct
                {
                    Title = product.Title?.Trim(),
                    Description = product.Description?.Trim() ?? string.Empty,
                    SellerId = product.SellerId,
                    ConditionId = product.Condition?.Id ?? product.ConditionId,
                    CategoryId = product.Category?.Id ?? product.CategoryId,
                    Price = product.Price,
                    Stock = product.Stock > 0 ? product.Stock : 1,
                    ProductTags = new List<BuyProductProductTag>(), // Empty list
                    Images = new List<BuyProductImage>()
                };

                // Add images if any
                if (product.NonMappedImages != null && product.NonMappedImages.Any())
                {
                    foreach (var img in product.NonMappedImages)
                    {
                        if (!string.IsNullOrWhiteSpace(img.Url))
                        {
                            productToSend.Images.Add(new BuyProductImage { Url = img.Url });
                        }
                    }
                }
                else if (product.Images != null && product.Images.Any())
                {
                    foreach (var img in product.Images)
                    {
                        if (!string.IsNullOrWhiteSpace(img.Url))
                        {
                            productToSend.Images.Add(new BuyProductImage { Url = img.Url });
                        }
                    }
                }

                // Log the final product data being sent
                Console.WriteLine($"Final product to send: Title='{productToSend.Title}', " +
                                $"SellerId={productToSend.SellerId}, CategoryId={productToSend.CategoryId}, " +
                                $"ConditionId={productToSend.ConditionId}, Price={productToSend.Price}, " +
                                $"Stock={productToSend.Stock}, TagCount={0}, " + // No tags for now
                                $"ImageCount={productToSend.Images.Count}");

                // Serialize to see the actual JSON being sent
                var jsonToSend = System.Text.Json.JsonSerializer.Serialize(productToSend, 
                    new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                Console.WriteLine($"JSON being sent to server:\n{jsonToSend}");

                var responseJson = buyProductsRepository.CreateListing(productToSend);
                Console.WriteLine($"Server response: {responseJson}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Error creating product: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating product: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
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
                var json = buyProductsRepository.GetProductByIdAsync(id).GetAwaiter().GetResult();
                if (string.IsNullOrWhiteSpace(json))
                {
                    throw new KeyNotFoundException($"Buy product with ID {id} not found.");
                }

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
                var productJson = await buyProductsRepository.GetProductByIdAsync(productId);
                if (string.IsNullOrWhiteSpace(productJson))
                {
                    return null;
                }

                // First deserialize to DTO
                var productDTO = JsonSerializer.Deserialize<BuyProductDTO>(productJson, jsonOptions);
                if (productDTO == null)
                {
                    return null;
                }

                // Then map DTO to domain model
                var product = BuyProductMapper.FromDTO(productDTO);
                return product;
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

        public async Task<double> GetMaxPriceAsync()
        {
            try
            {
                return await buyProductsRepository.GetMaxPriceAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting max price for buy products: {ex.Message}");
                return 0.0; // Return 0 on error
            }
        }
    }
}
