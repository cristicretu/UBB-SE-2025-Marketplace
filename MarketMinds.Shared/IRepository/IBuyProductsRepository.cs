using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.IRepository
{
    /// <summary>
    /// Interface for managing buy products in the repository.
    /// </summary>
    public interface IBuyProductsRepository
    {
        /// <summary>
        /// Adds a new buy product to the repository.
        /// </summary>
        /// <param name="product">The product to add.</param>
        void AddProduct(BuyProduct product);

        /// <summary>
        /// Deletes a buy product from the repository.
        /// </summary>
        /// <param name="product">The product to delete.</param>
        void DeleteProduct(BuyProduct product);

        /// <summary>
        /// Retrieves all buy products from the repository.
        /// </summary>
        /// <returns>A list of all buy products.</returns>
        List<BuyProduct> GetProducts();

        /// <summary>
        /// Retrieves buy products with pagination support.
        /// </summary>
        /// <param name="offset">The number of products to skip (0 for first page).</param>
        /// <param name="count">The number of products to return (0 for all products).</param>
        /// <returns>A list of buy products for the specified page.</returns>
        List<BuyProduct> GetProducts(int offset, int count);

        /// <summary>
        /// Retrieves buy products filtered by conditions, categories, maximum price, and search term with pagination support.
        /// </summary>
        /// <param name="offset">The number of products to skip (0 for first page).</param>
        /// <param name="count">The number of products to return (0 for all products).</param>
        /// <param name="conditionIds">List of condition IDs to filter by (null or empty for no filter).</param>
        /// <param name="categoryIds">List of category IDs to filter by (null or empty for no filter).</param>
        /// <param name="maxPrice">Maximum price filter (null for no maximum).</param>
        /// <param name="searchTerm">Search term to filter by title, description, or seller (null or empty for no search).</param>
        /// <returns>A list of filtered buy products for the specified page.</returns>
        List<BuyProduct> GetFilteredProducts(int offset, int count, List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null);

        /// <summary>
        /// Gets the total count of buy products.
        /// </summary>
        /// <returns>The total number of buy products.</returns>
        int GetProductCount();

        /// <summary>
        /// Gets the total count of buy products matching the specified filters.
        /// </summary>
        /// <param name="conditionIds">List of condition IDs to filter by (null or empty for no filter).</param>
        /// <param name="categoryIds">List of category IDs to filter by (null or empty for no filter).</param>
        /// <param name="maxPrice">Maximum price filter (null for no maximum).</param>
        /// <param name="searchTerm">Search term to filter by title, description, or seller (null or empty for no search).</param>
        /// <returns>The total number of buy products matching the filters.</returns>
        int GetFilteredProductCount(List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null);

        /// <summary>
        /// Retrieves a buy product by its ID.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>The product with the given ID.</returns>
        BuyProduct GetProductByID(int productId);

        /// <summary>
        /// Updates an existing buy product in the repository.
        /// </summary>
        /// <param name="product">The product with updated information.</param>
        void UpdateProduct(BuyProduct product);

        /// <summary>
        /// Adds an image to an existing product.
        /// </summary>
        /// <param name="productId">The ID of the product to add the image to.</param>
        /// <param name="image">The image to add.</param>
        void AddImageToProduct(int productId, BuyProductImage image);

        /// <summary>
        /// Gets the maximum price of all buy products asynchronously.
        /// </summary>
        /// <returns>The maximum price of buy products, or 0 if no products exist.</returns>
        Task<double> GetMaxPriceAsync();
    }
}
