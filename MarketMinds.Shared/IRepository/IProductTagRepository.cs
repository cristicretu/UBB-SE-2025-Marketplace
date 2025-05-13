using System.Collections.Generic;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.IRepository
{
    /// <summary>
    /// Interface for ProductTagRepository to manage product tag operations.
    /// </summary>
    public interface IProductTagRepository
    {
        /// <summary>
        /// Returns all the product tags.
        /// </summary>
        /// <returns>A list of all product tags.</returns>
        List<ProductTag> GetAllProductTags();

        /// <summary>
        /// Creates a new product tag.
        /// </summary>
        /// <param name="displayTitle">The display title of the product tag.</param>
        /// <returns>The created product tag.</returns>
        ProductTag CreateProductTag(string displayTitle);

        /// <summary>
        /// Deletes a product tag by its display title.
        /// </summary>
        /// <param name="displayTitle">The display title of the product tag to delete.</param>
        void DeleteProductTag(string displayTitle);
    }
}

