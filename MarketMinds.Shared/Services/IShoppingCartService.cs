// -----------------------------------------------------------------------
// <copyright file="IShoppingCartService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SharedClassLibrary.Service
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;

    /// <summary>
    /// Interface for service operations related to shopping cart functionality.
    /// </summary>
    public interface IShoppingCartService
    {
        /// <summary>
        /// Adds a product to the buyer's shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="productId">The ID of the product to add to the cart.</param>
        /// <param name="quantity">The quantity of the product to add. Default is 1.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task AddProductToCartAsync(int buyerId, int productId, int quantity = 1);

        /// <summary>
        /// Removes a product from the buyer's shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="productId">The ID of the product to remove from the cart.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RemoveProductFromCartAsync(int buyerId, int productId);

        /// <summary>
        /// Updates the quantity of a product in the buyer's shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="productId">The ID of the product to update.</param>
        /// <param name="quantity">The new quantity.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateProductQuantityAsync(int buyerId, int productId, int quantity);

        /// <summary>
        /// Increments the quantity of a product in the buyer's shopping cart by 1.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="productId">The ID of the product to increment.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task IncrementProductQuantityAsync(int buyerId, int productId);

        /// <summary>
        /// Decrements the quantity of a product in the buyer's shopping cart by 1.
        /// If the quantity becomes 0, the product is removed from the cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="productId">The ID of the product to decrement.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DecrementProductQuantityAsync(int buyerId, int productId);

        /// <summary>
        /// Gets all products in the buyer's shopping cart with their quantities.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list with products and quantities.</returns>
        Task<List<Product>> GetCartItemsAsync(int buyerId);

        Task<double> GetProductPriceAsync(int buyerId, int productId);

        /// <summary>
        /// Gets the total price of all items in the buyer's shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total price.</returns>
        Task<double> GetCartTotalAsync(int buyerId);

        /// <summary>
        /// Clears all items from the buyer's shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ClearCartAsync(int buyerId);

        /// <summary>
        /// Gets the total number of items in the buyer's shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total number of items.</returns>
        Task<int> GetCartItemCountAsync(int buyerId);

        /// <summary>
        /// Checks if a specific product is in the buyer's cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="productId">The ID of the product to check.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the product is in the cart, otherwise false.</returns>
        Task<bool> IsProductInCartAsync(int buyerId, int productId);
    }
}
