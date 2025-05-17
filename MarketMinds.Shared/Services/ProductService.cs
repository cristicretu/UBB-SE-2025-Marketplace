using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.ProxyRepository;
using MarketMinds.Shared.Helper;
using System.Text.Json;
using System.Linq;

namespace MarketMinds.Shared.Services
{
    /// <summary>
    /// Service implementation for managing products
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly BuyProductsProxyRepository _productRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductService"/> class.
        /// </summary>
        /// <param name="productRepository">The product repository.</param>
        public ProductService(BuyProductsProxyRepository productRepository)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        /// <inheritdoc/>
        public List<Product> GetSortedFilteredProducts(List<Condition> selectedConditions, List<Category> selectedCategories, 
            List<ProductTag> selectedTags, ProductSortType sortCondition, string searchQuery)
        {
            // Implementation would depend on repository methods
            // This is a placeholder until the repository has this method
            return new List<Product>();
        }

        /// <inheritdoc/>
        public async Task<Product> GetProductByIdAsync(int productId)
        {
            // Get the raw JSON string
            var productJson = _productRepository.GetProductById(productId);

            if (string.IsNullOrWhiteSpace(productJson))
            {
                throw new KeyNotFoundException($"Product with ID {productId} was not found.");
            }

            // Deserialize to Product object
            var product = JsonSerializer.Deserialize<Product>(productJson);

            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} was not found or could not be parsed.");
            }

            return product;
        }

        /// <inheritdoc/>
        public async Task<string> GetSellerNameAsync(int? sellerId)
        {
            // Use the repository to fetch the seller name from the database
            return await _productRepository.GetSellerNameAsync(sellerId);
        }

        /// <inheritdoc/>
        public async Task<List<Product>> GetBorrowableProductsAsync()
        {
            // Use the repository to fetch borrowable products from the database
            var borrowProducts = await _productRepository.GetBorrowableProductsAsync();
            
            // Convert to List<Product> - BorrowProduct should inherit from Product
            return borrowProducts.Cast<Product>().ToList();
        }
    }
}
