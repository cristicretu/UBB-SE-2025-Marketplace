using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.IRepository
{
    /// <summary>
    /// Defines database operations for dummy products.
    /// </summary>
    public interface IProductRepository
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

        Task AddProductAsync(string name, double price, int sellerId, string productType, DateTime startDate, DateTime endDate);
        Task DeleteProduct(int id);
        Task<string> GetSellerNameAsync(int? sellerId);
        Task<Product> GetProductByIdAsync(int productId);
        
        /// <summary>
        /// Gets all borrowable products from the waitlist
        /// </summary>
        /// <returns>A list of products that are available for borrowing</returns>
        Task<List<BorrowProduct>> GetBorrowableProductsAsync();


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