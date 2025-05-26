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
        /// Gets the total count of auction products.
        /// </summary>
        /// <returns>The total number of auction products.</returns>
        int GetProductCount();

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
    }
}