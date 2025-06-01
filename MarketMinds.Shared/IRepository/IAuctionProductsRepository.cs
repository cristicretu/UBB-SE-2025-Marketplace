using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.IRepository
{
    public interface IAuctionProductsRepository
    {
        /// <summary>
        /// Retrieves all auction products from the repository.
        /// </summary>
        /// <returns>A list of all auction products.</returns>
        List<AuctionProduct> GetProducts();

        /// <summary>
        /// Retrieves auction products with pagination support.
        /// </summary>
        /// <param name="offset">The number of products to skip (0 for first page).</param>
        /// <param name="count">The number of products to return (0 for all products).</param>
        /// <returns>A list of auction products for the specified page.</returns>
        List<AuctionProduct> GetProducts(int offset, int count);

        /// <summary>
        /// Retrieves auction products filtered by conditions, categories, maximum price, and search term with pagination support.
        /// </summary>
        /// <param name="offset">The number of products to skip (0 for first page).</param>
        /// <param name="count">The number of products to return (0 for all products).</param>
        /// <param name="conditionIds">List of condition IDs to filter by (null or empty for no filter).</param>
        /// <param name="categoryIds">List of category IDs to filter by (null or empty for no filter).</param>
        /// <param name="maxPrice">Maximum price filter (null for no maximum).</param>
        /// <param name="searchTerm">Search term to filter by title, description, or seller (null or empty for no search).</param>
        /// <returns>A list of filtered auction products for the specified page.</returns>
        List<AuctionProduct> GetFilteredProducts(int offset, int count, List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null);

        /// <summary>
        /// Retrieves auction products filtered by conditions, categories, maximum price, search term, and seller ID with pagination support.
        /// </summary>
        /// <param name="offset">The number of products to skip (0 for first page).</param>
        /// <param name="count">The number of products to return (0 for all products).</param>
        /// <param name="conditionIds">List of condition IDs to filter by (null or empty for no filter).</param>
        /// <param name="categoryIds">List of category IDs to filter by (null or empty for no filter).</param>
        /// <param name="maxPrice">Maximum price filter (null for no maximum).</param>
        /// <param name="searchTerm">Search term to filter by title, description, or seller (null or empty for no search).</param>
        /// <param name="sellerId">Seller ID to filter by (null for no seller filter).</param>
        /// <returns>A list of filtered auction products for the specified page.</returns>
        List<AuctionProduct> GetFilteredProducts(int offset, int count, List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null, int? sellerId = null);

        /// <summary>
        /// Gets the total count of auction products.
        /// </summary>
        /// <returns>The total number of auction products.</returns>
        int GetProductCount();

        /// <summary>
        /// Gets the total count of auction products matching the specified filters.
        /// </summary>
        /// <param name="conditionIds">List of condition IDs to filter by (null or empty for no filter).</param>
        /// <param name="categoryIds">List of category IDs to filter by (null or empty for no filter).</param>
        /// <param name="maxPrice">Maximum price filter (null for no maximum).</param>
        /// <param name="searchTerm">Search term to filter by title, description, or seller (null or empty for no search).</param>
        /// <returns>The total number of auction products matching the filters.</returns>
        int GetFilteredProductCount(List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null);

        /// <summary>
        /// Gets the total count of auction products matching the specified filters including seller ID.
        /// </summary>
        /// <param name="conditionIds">List of condition IDs to filter by (null or empty for no filter).</param>
        /// <param name="categoryIds">List of category IDs to filter by (null or empty for no filter).</param>
        /// <param name="maxPrice">Maximum price filter (null for no maximum).</param>
        /// <param name="searchTerm">Search term to filter by title, description, or seller (null or empty for no search).</param>
        /// <param name="sellerId">Seller ID to filter by (null for no seller filter).</param>
        /// <returns>The total number of auction products matching the filters.</returns>
        int GetFilteredProductCount(List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null, int? sellerId = null);

        /// <summary>
        /// Gets an auction product by its ID.
        /// </summary>
        /// <param name="id">The ID of the auction product to retrieve.</param>
        /// <returns>The auction product with the specified ID.</returns>
        AuctionProduct GetProductByID(int id);

        /// <summary>
        /// Adds a new auction product to the repository.
        /// </summary>
        /// <param name="product">The auction product to add.</param>
        void AddProduct(AuctionProduct product);

        /// <summary>
        /// Updates an existing auction product in the repository.
        /// </summary>
        /// <param name="product">The auction product to update.</param>
        void UpdateProduct(AuctionProduct product);

        /// <summary>
        /// Deletes an auction product from the repository.
        /// </summary>
        /// <param name="product">The auction product to delete.</param>
        void DeleteProduct(AuctionProduct product);

        /// <summary>
        /// Gets the maximum current price of all auction products asynchronously.
        /// </summary>
        /// <returns>The maximum current price of auction products, or 0 if no products exist.</returns>
        Task<double> GetMaxPriceAsync();
    }
}