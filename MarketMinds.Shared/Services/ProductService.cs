using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;

namespace MarketMinds.Shared.Services
{
    /// <summary>
    /// Service implementation for managing products
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductService"/> class.
        /// </summary>
        /// <param name="productRepository">The product repository.</param>
        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        /// <inheritdoc/>
        public async Task<Product> GetProductByIdAsync(int productId)
        {
            return await _productRepository.GetProductByIdAsync(productId);
        }

        /// <inheritdoc/>
        public async Task<string> GetSellerNameAsync(int? sellerId)
        {
            // Use the repository to fetch the seller name from the database
            return await productRepository.GetSellerNameAsync(sellerId);
        }

        /// <inheritdoc/>
        public async Task<List<BorrowProduct>> GetBorrowableProductsAsync()
        {
            // Use the repository to fetch borrowable products from the database
            return await productRepository.GetBorrowableProductsAsync();
        }

        public List<Product> GetSortedFilteredProducts(List<Condition> selectedConditions, List<Category> selectedCategories, List<ProductTag> selectedTags, ProductSortType sortCondition, string searchQuery)
        {
            throw new NotImplementedException();
            return await _productRepository.GetSellerNameAsync(sellerId);
        }
    }
}
