﻿using MarketMinds.Shared.Models;
using MarketMinds.Shared.Models.DTOs;

namespace MarketMinds.Shared.Services.BorrowProductsService
{
    /// <summary>
    /// Interface for the borrow products service.
    /// </summary>
    public interface IBorrowProductsService
    {
        /// <summary>
        /// Creates a new product listing.
        /// </summary>
        /// <param name="product">The product to add.</param>
        void CreateListing(Product product);

        /// <summary>
        /// Creates a new borrow product from a DTO.
        /// </summary>
        /// <param name="productDTO">The DTO containing product information.</param>
        /// <returns>The created BorrowProduct with its ID set.</returns>
        BorrowProduct CreateProduct(CreateBorrowProductDTO productDTO);

        /// <summary>
        /// Validates the product DTO for creating a borrow product.
        /// </summary>
        /// <param name="productDTO">The DTO to validate.</param>
        /// <returns>A dictionary of validation errors, if any.</returns>
        Dictionary<string, string[]> ValidateProductDTO(CreateBorrowProductDTO productDTO);

        /// <summary>
        /// Deletes an existing product listing.
        /// </summary>
        /// <param name="product">The product to delete.</param>
        void DeleteListing(Product product);

        /// <summary>
        /// Gets all products.
        /// </summary>
        /// <returns>A list of all products.</returns>
        List<Product> GetProducts();

        /// <summary>
        /// Gets a product by ID.
        /// </summary>
        /// <param name="id">The ID of the product to retrieve.</param>
        /// <returns>The product with the specified ID.</returns>
        Product GetProductById(int id);

        /// <summary>
        /// Gets all borrow products asynchronously.
        /// </summary>
        /// <returns>A list of all borrow products.</returns>
        Task<List<BorrowProduct>> GetAllBorrowProductsAsync();

        /// <summary>
        /// Gets borrow products with pagination support asynchronously.
        /// </summary>
        /// <param name="offset">The number of products to skip (0 for first page).</param>
        /// <param name="count">The number of products to return (0 for all products).</param>
        /// <returns>A list of borrow products for the specified page.</returns>
        Task<List<BorrowProduct>> GetAllBorrowProductsAsync(int offset, int count);

        /// <summary>
        /// Gets the total count of borrow products asynchronously.
        /// </summary>
        /// <returns>The total number of borrow products.</returns>
        Task<int> GetBorrowProductCountAsync();

        /// <summary>
        /// Gets a borrow product by ID asynchronously.
        /// </summary>
        /// <param name="id">The ID of the product to retrieve.</param>
        /// <returns>The borrow product with the specified ID or null if not found.</returns>
        Task<BorrowProduct> GetBorrowProductByIdAsync(int id);

        /// <summary>
        /// Creates a new borrow product asynchronously.
        /// </summary>
        /// <param name="borrowProduct">The borrow product to create.</param>
        /// <returns>True if successful, false otherwise.</returns>
        Task<bool> CreateBorrowProductAsync(BorrowProduct borrowProduct);

        /// <summary>
        /// Creates a new borrow product from a DTO asynchronously.
        /// </summary>
        /// <param name="productDTO">The DTO containing product information.</param>
        /// <returns>The created BorrowProduct with its ID set or null if failed.</returns>
        Task<BorrowProduct> CreateProductAsync(CreateBorrowProductDTO productDTO);

        /// <summary>
        /// Updates an existing borrow product asynchronously.
        /// </summary>
        /// <param name="borrowProduct">The borrow product with updated values.</param>
        /// <returns>True if successful, false otherwise.</returns>
        Task<bool> UpdateBorrowProductAsync(BorrowProduct borrowProduct);

        /// <summary>
        /// Deletes a borrow product by ID asynchronously.
        /// </summary>
        /// <param name="id">The ID of the product to delete.</param>
        /// <returns>True if successful, false otherwise.</returns>
        Task<bool> DeleteBorrowProductAsync(int id);

        /// <summary>
        /// Validates the product DTO asynchronously.
        /// </summary>
        /// <param name="productDTO">The DTO to validate.</param>
        /// <returns>A dictionary of validation errors, if any.</returns>
        Task<Dictionary<string, string[]>> ValidateProductDTOAsync(CreateBorrowProductDTO productDTO);

        /// <summary>
        /// Gets filtered borrow products with pagination support including seller filter.
        /// </summary>
        /// <param name="offset">The number of products to skip (0 for first page).</param>
        /// <param name="count">The number of products to return (0 for all products).</param>
        /// <param name="conditionIds">List of condition IDs to filter by (null or empty for no filter).</param>
        /// <param name="categoryIds">List of category IDs to filter by (null or empty for no filter).</param>
        /// <param name="maxPrice">Maximum price filter (null for no maximum).</param>
        /// <param name="searchTerm">Search term to filter by title, description, or seller (null or empty for no search).</param>
        /// <param name="sellerId">Seller ID to filter by (null for no seller filter).</param>
        /// <returns>A list of filtered borrow products for the specified page.</returns>
        List<BorrowProduct> GetFilteredProducts(int offset, int count, List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null, int? sellerId = null);

        /// <summary>
        /// Gets the total count of borrow products matching the specified filters including seller filter.
        /// </summary>
        /// <param name="conditionIds">List of condition IDs to filter by (null or empty for no filter).</param>
        /// <param name="categoryIds">List of category IDs to filter by (null or empty for no filter).</param>
        /// <param name="maxPrice">Maximum price filter (null for no maximum).</param>
        /// <param name="searchTerm">Search term to filter by title, description, or seller (null or empty for no search).</param>
        /// <param name="sellerId">Seller ID to filter by (null for no seller filter).</param>
        /// <returns>The total number of borrow products matching the filters.</returns>
        int GetFilteredProductCount(List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null, int? sellerId = null);

        /// <summary>
        /// Gets the maximum daily rate of all borrow products asynchronously.
        /// </summary>
        /// <returns>The maximum daily rate of borrow products, or 0 if no products exist.</returns>
        Task<double> GetMaxPriceAsync();
    }
}
