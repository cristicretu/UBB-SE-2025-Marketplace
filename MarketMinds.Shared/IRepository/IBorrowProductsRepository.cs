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
    }
}