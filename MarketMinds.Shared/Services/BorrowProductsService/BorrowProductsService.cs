using MarketMinds.Shared.Models;
using MarketMinds.Shared.ProxyRepository;
using MarketMinds.Shared.Models.DTOs;
using MarketMinds.Shared.Helper;
using MarketMinds.Shared.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using MarketMinds.Shared.IRepository;

namespace MarketMinds.Shared.Services.BorrowProductsService
{
    public class BorrowProductsService : IBorrowProductsService, IProductService
    {
        private readonly BorrowProductsProxyRepository borrowProductsRepository;

        private const int DEFAULT_PRODUCT_COUNT = 0;
        private const int NULL_PRODUCT_ID = 0;

        public BorrowProductsService()
        {
            this.borrowProductsRepository = new BorrowProductsProxyRepository(AppConfig.Configuration);
        }

        public void CreateListing(Product product)
        {
            if (!(product is BorrowProduct borrowProduct))
            {
                throw new ArgumentException("Product must be a BorrowProduct.", nameof(product));
            }

            ApplyDefaultDates(borrowProduct);

            borrowProductsRepository.CreateListing(borrowProduct);
        }

        public BorrowProduct CreateProduct(CreateBorrowProductDTO productDTO)
        {
            if (productDTO == null)
            {
                throw new ArgumentNullException(nameof(productDTO), "Product data cannot be null");
            }

            var product = new BorrowProduct
            {
                Title = productDTO.Title,
                Description = productDTO.Description,
                SellerId = productDTO.SellerId,
                CategoryId = productDTO.CategoryId ?? NULL_PRODUCT_ID,
                ConditionId = productDTO.ConditionId ?? NULL_PRODUCT_ID,
                TimeLimit = productDTO.TimeLimit,
                StartDate = productDTO.StartDate,
                EndDate = productDTO.EndDate,
                DailyRate = productDTO.DailyRate,
                IsBorrowed = productDTO.IsBorrowed
            };

            ApplyDefaultDates(product);

            borrowProductsRepository.CreateListing(product);

            if (productDTO.Images != null && productDTO.Images.Any())
            {
                foreach (var imageInfo in productDTO.Images)
                {
                    if (string.IsNullOrWhiteSpace(imageInfo.Url))
                    {
                        continue;
                    }

                    var image = new BorrowProductImage
                    {
                        Url = imageInfo.Url,
                        ProductId = product.Id
                    };

                    borrowProductsRepository.AddImageToProduct(product.Id, image);
                }
            }

            return (BorrowProduct)GetProductById(product.Id);
        }

        public Dictionary<string, string[]> ValidateProductDTO(CreateBorrowProductDTO productDTO)
        {
            Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();

            if (string.IsNullOrWhiteSpace(productDTO.Title))
            {
                AddError(errors, "Title", "Title is required");
            }

            if (productDTO.SellerId <= 0)
            {
                AddError(errors, "SellerId", "Valid seller ID is required");
            }

            if (productDTO.DailyRate < 0)
            {
                AddError(errors, "DailyRate", "Daily rate cannot be negative");
            }

            if (productDTO.StartDate != default && productDTO.EndDate != default && productDTO.EndDate < productDTO.StartDate)
            {
                AddError(errors, "EndDate", "End date cannot be before start date");
            }

            return errors.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());
        }

        private void AddError(Dictionary<string, List<string>> errors, string key, string errorMessage)
        {
            if (!errors.ContainsKey(key))
            {
                errors[key] = new List<string>();
            }
            errors[key].Add(errorMessage);
        }

        private void ApplyDefaultDates(BorrowProduct product)
        {
            if (product.StartDate == default)
            {
                product.StartDate = DateTime.Now;
            }

            if (product.EndDate == default)
            {
                product.EndDate = DateTime.Now.AddDays(7);
            }

            if (product.TimeLimit == default)
            {
                product.TimeLimit = DateTime.Now.AddDays(7);
            }

            DateTime startDate = product.StartDate ?? DateTime.Now;
            DateTime endDate = product.EndDate ?? startDate.AddDays(7);

            if (endDate < startDate)
            {
                product.EndDate = startDate.AddDays(7);
            }
        }

        public void DeleteListing(Product product)
        {
            if (product.Id == 0)
            {
                throw new ArgumentException("Product ID must be set for delete.", nameof(product.Id));
            }

            borrowProductsRepository.DeleteListing(product);
        }

        public void UpdateListing(Product product)
        {
            if (!(product is BorrowProduct borrowProduct))
            {
                throw new ArgumentException("Product must be a BorrowProduct.", nameof(product));
            }

            if (borrowProduct.Id == 0)
            {
                throw new ArgumentException("Product ID must be set for update.", nameof(borrowProduct.Id));
            }

            try
            {
                ((IBorrowProductsRepository)borrowProductsRepository).UpdateProduct(borrowProduct);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<Product> GetProducts()
        {
            return borrowProductsRepository.GetProducts();
        }

        public Product GetProductById(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Product ID must be greater than 0", nameof(id));
            }

            try
            {
                var product = borrowProductsRepository.GetProductByID(id);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Borrow product with ID {id} not found.");
                }
                return product;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new KeyNotFoundException($"Borrow product with ID {id} not found: {ex.Message}");
            }
        }

        public List<Product> GetSortedFilteredProducts(List<Condition> selectedConditions, List<Category> selectedCategories, List<ProductTag> selectedTags, ProductSortType? sortCondition, string searchQuery)
        {
            List<Product> products = GetProducts();
            List<Product> productResultSet = new List<Product>();
            foreach (Product product in products)
            {
                bool matchesConditions = selectedConditions == null || selectedConditions.Count == DEFAULT_PRODUCT_COUNT || selectedConditions.Any(condition => condition.Id == product.Condition.Id);
                bool matchesCategories = selectedCategories == null || selectedCategories.Count == DEFAULT_PRODUCT_COUNT || selectedCategories.Any(category => category.Id == product.Category.Id);
                bool matchesTags = selectedTags == null || selectedTags.Count == DEFAULT_PRODUCT_COUNT || selectedTags.Any(tag => product.Tags.Any(productTag => productTag.Id == tag.Id));
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
                        product =>
                        {
                            var property = product?.GetType().GetProperty(sortCondition.InternalAttributeFieldTitle);
                            return property?.GetValue(product);
                        }).ToList();
                }
                else
                {
                    productResultSet = productResultSet.OrderByDescending(
                        product =>
                        {
                            var property = product?.GetType().GetProperty(sortCondition.InternalAttributeFieldTitle);
                            return property?.GetValue(product);
                        }).ToList();
                }
            }

            return productResultSet;
        }

        #region Async Methods

        public async Task<List<BorrowProduct>> GetAllBorrowProductsAsync()
        {
            try
            {
                return await borrowProductsRepository.GetAllBorrowProductsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all borrow products: {ex.Message}");
                return new List<BorrowProduct>();
            }
        }

        public async Task<List<BorrowProduct>> GetAllBorrowProductsAsync(int offset, int count)
        {
            try
            {
                return await borrowProductsRepository.GetAllBorrowProductsAsync(offset, count);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting borrow products with pagination (offset: {offset}, count: {count}): {ex.Message}");
                return new List<BorrowProduct>();
            }
        }

        public async Task<int> GetBorrowProductCountAsync()
        {
            try
            {
                return await borrowProductsRepository.GetBorrowProductCountAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting borrow product count: {ex.Message}");
                return 0;
            }
        }

        public async Task<BorrowProduct> GetBorrowProductByIdAsync(int id)
        {
            // In your existing sync GetProductById you already return or throw NotFound.
            // Here we just wrap it in a Task so that callers get an actual product instead of null.
            try
            {
                var product = GetProductById(id);
                return product as BorrowProduct;
            }
            catch (KeyNotFoundException)
            {
                // let the caller know there is no product
                return null;
            }
        }

        public async Task<bool> CreateBorrowProductAsync(BorrowProduct borrowProduct)
        {
            try
            {
                if (borrowProduct == null)
                {
                    throw new ArgumentNullException(nameof(borrowProduct), "BorrowProduct cannot be null");
                }

                if (string.IsNullOrWhiteSpace(borrowProduct.Title))
                {
                    throw new ArgumentException("Title is required", nameof(borrowProduct.Title));
                }

                if (borrowProduct.CategoryId <= 0)
                {
                    throw new ArgumentException("CategoryId must be greater than zero", nameof(borrowProduct.CategoryId));
                }

                if (borrowProduct.ConditionId <= 0)
                {
                    throw new ArgumentException("ConditionId must be greater than zero", nameof(borrowProduct.ConditionId));
                }

                if (borrowProduct.DailyRate <= 0)
                {
                    throw new ArgumentException("DailyRate must be greater than zero", nameof(borrowProduct.DailyRate));
                }

                CreateListing(borrowProduct);
                return true;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error in CreateBorrowProductAsync: {exception.Message}");
                if (exception.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {exception.InnerException.Message}");
                }

                throw;
            }
        }

        public async Task<BorrowProduct> CreateProductAsync(CreateBorrowProductDTO productDTO)
        {
            try
            {
                return CreateProduct(productDTO);
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<bool> UpdateBorrowProductAsync(BorrowProduct borrowProduct)
        {
            try
            {
                UpdateListing(borrowProduct);
                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteBorrowProductAsync(int id)
        {
            try
            {
                var product = GetProductById(id);
                DeleteListing(product);
                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        public async Task<Dictionary<string, string[]>> ValidateProductDTOAsync(CreateBorrowProductDTO productDTO)
        {
            try
            {
                return ValidateProductDTO(productDTO);
            }
            catch (Exception)
            {
                return new Dictionary<string, string[]>();
            }
        }

        public List<BorrowProduct> GetFilteredProducts(int offset, int count, List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null)
        {
            try
            {
                return borrowProductsRepository.GetFilteredProducts(offset, count, conditionIds, categoryIds, maxPrice, searchTerm);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting filtered borrow products: {ex.Message}");
                return new List<BorrowProduct>();
            }
        }

        public int GetFilteredProductCount(List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null)
        {
            try
            {
                return borrowProductsRepository.GetFilteredProductCount(conditionIds, categoryIds, maxPrice, searchTerm);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting filtered borrow product count: {ex.Message}");
                return 0;
            }
        }

        Task<Product> IProductService.GetProductByIdAsync(int productId)
        => Task.FromResult<Product>(GetBorrowProductByIdAsync(productId).Result);

        #endregion

        // merge-nicusor

        Task<string> IProductService.GetSellerNameAsync(int? sellerId)
        {
            // Not implemented in borrow products service
            return Task.FromResult<string>(null);
        }

        /// <summary>
        /// Gets a list of products that can be borrowed.
        /// Implementation for IProductService interface.
        /// </summary>
        /// <returns>A list of borrowable products.</returns>
        public async Task<List<Product>> GetBorrowableProductsAsync()
        {
            // Since this is BorrowProductsService, we can return all our products as they are borrowable
            var borrowProducts = await GetAllBorrowProductsAsync();
            return borrowProducts.Cast<Product>().ToList();
        }

        public async Task BorrowProductAsync(int userId, int productId, DateTime start, DateTime end)
        {
            await borrowProductsRepository.BorrowProductAsync(userId, productId, start, end);
        }




    }
}