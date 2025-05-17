using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.IRepository
{
    /// <summary>
    /// Defines database operations for dummy products.
    /// </summary>
    public interface IProductRepository
    {
        Task<string> GetSellerNameAsync(int? sellerId);
        
        /// <summary>
        /// Gets all borrowable products from the waitlist
        /// </summary>
        /// <returns>A list of products that are available for borrowing</returns>
        Task<List<Product>> GetBorrowableProductsAsync();


        // merge-nicusor
        /// <summary>
        /// Retrieves all products.
        /// </summary>
        /// <returns>A list of all products.</returns>
        List<Product> GetProducts();

        /// <summary>
        /// Retrieves a product by its ID.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <returns>The product with the given ID.</returns>
        Product GetProductByID(int id);

        /// <summary>
        /// Adds a new product to the repository.
        /// </summary>
        /// <param name="product">The product to add.</param>
        void AddProduct(Product product);

        /// <summary>
        /// Updates an existing product in the repository.
        /// </summary>
        /// <param name="product">The product with updated information.</param>
        void UpdateProduct(Product product);

        /// <summary>
        /// Deletes a product from the repository.
        /// </summary>
        /// <param name="product">The product to delete.</param>
        void DeleteProduct(Product product);
    }
} 