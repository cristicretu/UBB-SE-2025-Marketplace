﻿using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services.BuyProductsService
{
    /// <summary>
    /// Interface for the buy products service.
    /// </summary>
    public interface IBuyProductsService
    {
        /// <summary>
        /// Creates a new product listing.
        /// </summary>
        /// <param name="product">The product to add.</param>
        void CreateListing(BuyProduct product);

        /// <summary>
        /// Deletes an existing product listing.
        /// </summary>
        /// <param name="product">The product to delete.</param>
        void DeleteListing(Product product);

        /// <summary>
        /// Updates a dummy product in the database.
        /// </summary>
        /// <param name="id">The ID of the product to update.</param>
        /// <param name="name">The name of the product.</param>
        /// <param name="price">The price of the product.</param>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <param name="productType">The type of the product.</param>
        /// <param name="startDate">The start date for borrowed products.</param>
        /// <param name="endDate">The end date for borrowed products.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateProductAsync(int id, string name, double price, int sellerId, string productType, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets a product by ID
        /// </summary>
        /// <param name="productId">The product ID</param>
        /// <returns>The product, or null if not found</returns>
        Task<Product> GetProductByIdAsync(int productId);

        /// <summary>
        /// Gets a BuyProduct by ID
        /// </summary>
        /// <param name="productId">The product ID</param>
        /// <returns>The BuyProduct, or null if not found</returns>
        BuyProduct? GetProductById(int productId);

        /// <summary>
        /// Gets a seller's name by ID
        /// </summary>
        /// <param name="sellerId">The seller ID</param>
        /// <returns>The seller name</returns>
        Task<string> GetSellerNameAsync(int sellerId);

        /// <summary>
        /// Updates the stock quantity for a product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="newStockQuantity">New stock quantity</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task UpdateProductStockAsync(int productId, int newStockQuantity);

        /// <summary>
        /// Decreases the stock quantity for a product by the specified amount
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="decreaseAmount">Amount to decrease stock by</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task DecreaseProductStockAsync(int productId, int decreaseAmount);

        /// <summary>
        /// Gets all products (for backward compatibility).
        /// </summary>
        /// <returns>A list of all buy products.</returns>
        List<BuyProduct> GetProducts();

        /// <summary>
        /// Gets products with pagination support.
        /// </summary>
        /// <param name="offset">The number of products to skip (0 for first page).</param>
        /// <param name="count">The number of products to return (0 for all products).</param>
        /// <returns>A list of buy products for the specified page.</returns>
        List<BuyProduct> GetProducts(int offset, int count);

        /// <summary>
        /// Gets the total count of buy products.
        /// </summary>
        /// <returns>The total number of buy products.</returns>
        int GetProductCount();

        /// <summary>
        /// Gets filtered products with pagination support.
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
        /// Gets the total count of buy products matching the specified filters.
        /// </summary>
        /// <param name="conditionIds">List of condition IDs to filter by (null or empty for no filter).</param>
        /// <param name="categoryIds">List of category IDs to filter by (null or empty for no filter).</param>
        /// <param name="maxPrice">Maximum price filter (null for no maximum).</param>
        /// <param name="searchTerm">Search term to filter by title, description, or seller (null or empty for no search).</param>
        /// <returns>The total number of buy products matching the filters.</returns>
        int GetFilteredProductCount(List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null);

        /// <summary>
        /// Gets filtered products with pagination support including seller filter.
        /// </summary>
        /// <param name="offset">The number of products to skip (0 for first page).</param>
        /// <param name="count">The number of products to return (0 for all products).</param>
        /// <param name="conditionIds">List of condition IDs to filter by (null or empty for no filter).</param>
        /// <param name="categoryIds">List of category IDs to filter by (null or empty for no filter).</param>
        /// <param name="maxPrice">Maximum price filter (null for no maximum).</param>
        /// <param name="searchTerm">Search term to filter by title, description, or seller (null or empty for no search).</param>
        /// <param name="sellerId">Seller ID to filter by (null for no seller filter).</param>
        /// <returns>A list of filtered buy products for the specified page.</returns>
        List<BuyProduct> GetFilteredProducts(int offset, int count, List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null, int? sellerId = null);

        /// <summary>
        /// Gets the total count of buy products matching the specified filters including seller filter.
        /// </summary>
        /// <param name="conditionIds">List of condition IDs to filter by (null or empty for no filter).</param>
        /// <param name="categoryIds">List of category IDs to filter by (null or empty for no filter).</param>
        /// <param name="maxPrice">Maximum price filter (null for no maximum).</param>
        /// <param name="searchTerm">Search term to filter by title, description, or seller (null or empty for no search).</param>
        /// <param name="sellerId">Seller ID to filter by (null for no seller filter).</param>
        /// <returns>The total number of buy products matching the filters.</returns>
        int GetFilteredProductCount(List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null, int? sellerId = null);

        /// <summary>
        /// Gets the maximum price of all buy products asynchronously.
        /// </summary>
        /// <returns>The maximum price of buy products, or 0 if no products exist.</returns>
        Task<double> GetMaxPriceAsync();
    }
}
