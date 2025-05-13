using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services.ProductComparisonService
{
    /// <summary>
    /// Interface for ProductComparisonService to manage product comparison operations.
    /// </summary>
    public interface IProductComparisonService
    {
        /// <summary>
        /// Adds a product to the comparison and determines if the comparison is complete.
        /// </summary>
        /// <param name="leftProduct">The left product in the comparison.</param>
        /// <param name="rightProduct">The right product in the comparison.</param>
        /// <param name="newProduct">The new product to add to the comparison.</param>
        /// <returns>A tuple containing the updated left product, right product, and a boolean indicating if the comparison is complete.</returns>
        (Product LeftProduct, Product RightProduct, bool IsComplete) AddProduct(Product leftProduct, Product rightProduct, Product newProduct);
    }
}
