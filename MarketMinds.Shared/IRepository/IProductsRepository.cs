using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.IRepository
{
    /// <summary>
    /// Repository interface for managing products.
    /// </summary>
    public interface IProductsRepository
    {
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