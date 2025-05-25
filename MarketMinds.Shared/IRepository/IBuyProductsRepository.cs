using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.IRepository
{
    /// <summary>
    /// Interface for managing buy products in the repository.
    /// </summary>
    public interface IBuyProductsRepository
    {
        /// <summary>
        /// Adds a new buy product to the repository.
        /// </summary>
        /// <param name="product">The product to add.</param>
        void AddProduct(BuyProduct product);

        /// <summary>
        /// Deletes a buy product from the repository.
        /// </summary>
        /// <param name="product">The product to delete.</param>
        void DeleteProduct(BuyProduct product);

        /// <summary>
        /// Retrieves all buy products from the repository.
        /// </summary>
        /// <returns>A list of all buy products.</returns>
        List<BuyProduct> GetProducts();

        /// <summary>
        /// Retrieves buy products with pagination support.
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
        /// Retrieves a buy product by its ID.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>The product with the given ID.</returns>
        BuyProduct GetProductByID(int productId);

        /// <summary>
        /// Updates an existing buy product in the repository.
        /// </summary>
        /// <param name="product">The product with updated information.</param>
        void UpdateProduct(BuyProduct product);

        /// <summary>
        /// Adds an image to an existing product.
        /// </summary>
        /// <param name="productId">The ID of the product to add the image to.</param>
        /// <param name="image">The image to add.</param>
        void AddImageToProduct(int productId, BuyProductImage image);
    }
}
