using MarketMinds.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketMinds.Shared.Services
{
    /// <summary>
    /// Interface for managing products in the service layer.
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Gets sorted and filtered products based on various criteria.
        /// </summary>
        /// <param name="selectedConditions">The conditions to filter by.</param>
        /// <param name="selectedCategories">The categories to filter by.</param>
        /// <param name="selectedTags">The tags to filter by.</param>
        /// <param name="sortCondition">How to sort the products.</param>
        /// <param name="searchQuery">Optional search term to filter by.</param>
        /// <returns>A filtered and sorted list of products.</returns>
        List<Product> GetSortedFilteredProducts(List<Condition> selectedConditions, List<Category> selectedCategories, List<ProductTag> selectedTags, ProductSortType sortCondition, string searchQuery);

        /// <summary>
        /// Gets a product by its ID.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>The product with the specified ID.</returns>
        Task<Product> GetProductByIdAsync(int productId);

        /// <summary>
        /// Gets the name of a seller based on their ID.
        /// </summary>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <returns>The name of the seller.</returns>
        Task<string> GetSellerNameAsync(int? sellerId);
        
        /// <summary>
        /// Gets a list of products that can be borrowed.
        /// </summary>
        /// <returns>A list of borrowable products.</returns>
        Task<List<Product>> GetBorrowableProductsAsync();
    }
}
