using System.Collections.Generic;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services.ProductCategoryService
{
    /// <summary>
    /// Interface for ProductCategoryService to manage product category operations.
    /// </summary>
    public interface IProductCategoryService
    {
        /// <summary>
        /// Returns all the product categories.
        /// </summary>
        /// <returns>A list of all product categories.</returns>
        List<Category> GetAllProductCategories();

        /// <summary>
        /// Creates a new product category.
        /// </summary>
        /// <param name="displayTitle">The display title of the product category.</param>
        /// <param name="description">The description of the product category.</param>
        /// <returns>The created product category.</returns>
        Category CreateProductCategory(string displayTitle, string? description);

        /// <summary>
        /// Deletes a product category by its display title.
        /// </summary>
        /// <param name="displayTitle">The display title of the product category to delete.</param>
        void DeleteProductCategory(string displayTitle);
    }
}
