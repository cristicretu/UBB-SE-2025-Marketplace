using System.Collections.Generic;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.IRepository
{
    /// <summary>
    /// Interface for ProductCategoryRepository to manage product category operations.
    /// </summary>
    public interface IProductCategoryRepository
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
        Category CreateProductCategory(string displayTitle, string description);

        /// <summary>
        /// Deletes a product category by its display title.
        /// </summary>
        /// <param name="displayTitle">The display title of the product category to delete.</param>
        void DeleteProductCategory(string displayTitle);

        /// <summary>
        /// Gets the raw JSON data of all product categories.
        /// </summary>
        /// <returns>Raw JSON response as string</returns>
        string GetAllProductCategoriesRaw();

        /// <summary>
        /// Creates a new product category and returns the raw response.
        /// </summary>
        /// <param name="displayTitle">The display title of the product category.</param>
        /// <param name="description">The description of the product category.</param>
        /// <returns>Raw JSON response as string</returns>
        string CreateProductCategoryRaw(string displayTitle, string description);

        /// <summary>
        /// Deletes a product category by its display title.
        /// </summary>
        /// <param name="displayTitle">The display title of the product category to delete.</param>
        void DeleteProductCategoryRaw(string displayTitle);
    }
}
