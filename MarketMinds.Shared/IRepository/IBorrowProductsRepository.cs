using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.IRepository
{
    public interface IBorrowProductsRepository
    {
        /// <summary>
        /// Get all borrow products
        /// </summary>
        /// <returns>List of borrow products</returns>
        List<BorrowProduct> GetProducts();

        /// <summary>
        /// Get borrow products with pagination support
        /// </summary>
        /// <param name="offset">The number of products to skip (0 for first page)</param>
        /// <param name="count">The number of products to return (0 for all products)</param>
        /// <returns>List of borrow products for the specified page</returns>
        List<BorrowProduct> GetProducts(int offset, int count);

        /// <summary>
        /// Retrieves borrow products filtered by conditions, categories, maximum price, and search term with pagination support.
        /// </summary>
        /// <param name="offset">The number of products to skip (0 for first page).</param>
        /// <param name="count">The number of products to return (0 for all products).</param>
        /// <param name="conditionIds">List of condition IDs to filter by (null or empty for no filter).</param>
        /// <param name="categoryIds">List of category IDs to filter by (null or empty for no filter).</param>
        /// <param name="maxPrice">Maximum price filter (null for no maximum).</param>
        /// <param name="searchTerm">Search term to filter by title, description, or seller (null or empty for no search).</param>
        /// <returns>A list of filtered borrow products for the specified page.</returns>
        List<BorrowProduct> GetFilteredProducts(int offset, int count, List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null);

        /// <summary>
        /// Get the total count of borrow products
        /// </summary>
        /// <returns>The total number of borrow products</returns>
        int GetProductCount();

        /// <summary>
        /// Gets the total count of borrow products matching the specified filters.
        /// </summary>
        /// <param name="conditionIds">List of condition IDs to filter by (null or empty for no filter).</param>
        /// <param name="categoryIds">List of category IDs to filter by (null or empty for no filter).</param>
        /// <param name="maxPrice">Maximum price filter (null for no maximum).</param>
        /// <param name="searchTerm">Search term to filter by title, description, or seller (null or empty for no search).</param>
        /// <returns>The total number of borrow products matching the filters.</returns>
        int GetFilteredProductCount(List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null);

        /// <summary>
        /// Delete a borrow product
        /// </summary>
        /// <param name="product">The product to delete</param>
        void DeleteProduct(BorrowProduct product);

        /// <summary>
        /// Add a new borrow product
        /// </summary>
        /// <param name="product">The product to add</param>
        void AddProduct(BorrowProduct product);

        /// <summary>
        /// Update an existing borrow product
        /// </summary>
        /// <param name="product">The product with updated values</param>
        void UpdateProduct(BorrowProduct product);

        /// <summary>
        /// Get a borrow product by ID
        /// </summary>
        /// <param name="id">The ID of the product to retrieve</param>
        /// <returns>The borrow product with the specified ID</returns>
        BorrowProduct GetProductByID(int id);

        /// <summary>
        /// Adds an image to an existing product
        /// </summary>
        /// <param name="productId">The ID of the product to add the image to</param>
        /// <param name="image">The image to add</param>
        void AddImageToProduct(int productId, BorrowProductImage image);

        /// <summary>
        /// Gets the maximum daily rate of all borrow products asynchronously.
        /// </summary>
        /// <returns>The maximum daily rate of borrow products, or 0 if no products exist.</returns>
        Task<double> GetMaxPriceAsync();
    }
}