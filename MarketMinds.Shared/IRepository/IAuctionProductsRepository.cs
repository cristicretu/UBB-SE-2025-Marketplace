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