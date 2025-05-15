using System;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using System.Collections.Generic;

namespace SharedClassLibrary.Service
{
    /// <summary>
    /// Defines operations for managing dummy products.
    /// </summary>
    public interface IProductService
    {
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
        /// Gets a seller's name by ID
        /// </summary>
        /// <param name="sellerId">The seller ID</param>
        /// <returns>The seller name</returns>
        Task<string> GetSellerNameAsync(int sellerId);

        /// <summary>
        /// Gets all products available for borrowing from the waitlist
        /// </summary>
        /// <returns>List of borrowable products</returns>
        Task<List<Product>> GetBorrowableProductsAsync();
    }
}