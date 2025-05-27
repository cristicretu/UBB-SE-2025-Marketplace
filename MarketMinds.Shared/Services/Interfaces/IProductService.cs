using MarketMinds.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketMinds.Shared.Services.Interfaces
{
    /// <summary>
    /// Common interface for all product services
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Gets a product by ID
        /// </summary>
        /// <param name="productId">The product ID</param>
        /// <returns>The product, or null if not found</returns>
        Task<Product> GetProductByIdAsync(int productId);

        /// <summary>
        /// Gets a seller's name by ID
        /// </summary>
        /// <param name="sellerId">The seller ID</param>
        /// <returns>The seller name</returns>
        Task<string> GetSellerNameAsync(int? sellerId);

        /// <summary>
        /// Get products with sorting and filtering
        /// </summary>
        /// <param name="selectedConditions">Conditions to filter by</param>
        /// <param name="selectedCategories">Categories to filter by</param>
        /// <param name="selectedTags">Tags to filter by</param>
        /// <param name="sortCondition">How to sort results</param>
        /// <param name="searchQuery">Search term</param>
        /// <returns>List of matching products</returns>
        List<Product> GetSortedFilteredProducts(
            List<Condition> selectedConditions,
            List<Category> selectedCategories,
            List<ProductTag> selectedTags,
            ProductSortType? sortCondition,
            string searchQuery);

        /// <summary>
        /// Gets a list of products that can be borrowed.
        /// </summary>
        /// <returns>A list of borrowable products.</returns>
        Task<List<Product>> GetBorrowableProductsAsync();
    }
}
