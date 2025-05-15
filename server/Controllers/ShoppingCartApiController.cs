// <copyright file="ShoppingCartApiController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Controllers
{
    using System.Diagnostics;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// API controller for managing shopping cart data.
    /// </summary>
    [Authorize]
    [Route("api/shoppingcart")]
    [ApiController]
    public class ShoppingCartApiController : ControllerBase
    {
        private readonly IShoppingCartRepository shoppingCartRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShoppingCartApiController"/> class.
        /// </summary>
        /// <param name="shoppingCartRepository">The shopping cart repository dependency.</param>
        public ShoppingCartApiController(IShoppingCartRepository shoppingCartRepository)
        {
            this.shoppingCartRepository = shoppingCartRepository;
        }

        /// <summary>
        /// Gets all items in the shopping cart for a specific buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A list of products and their quantities.</returns>
        [HttpGet("{buyerId}/items")]
        [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Product>>> GetCartItems(int buyerId)
        {
            try
            {
                var cartItems = await this.shoppingCartRepository.GetCartItemsAsync(buyerId);
                return this.Ok(cartItems);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving cart items: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the number of items in the shopping cart for a specific buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>The number of items in the shopping cart.</returns>
        [HttpGet("{buyerId}/items/count")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> GetCartItemCount(int buyerId)
        {
            try
            {
                var itemCount = await this.shoppingCartRepository.GetCartItemCountAsync(buyerId);
                return this.Ok(itemCount);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving cart item count: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the quantity of a specific product in the shopping cart for a specific buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>The quantity of the product in the shopping cart.</returns>
        [HttpGet("{buyerId}/items/{productId}/quantity")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> GetProductQuantity(int buyerId, int productId)
        {
            try
            {
                var quantity = await this.shoppingCartRepository.GetProductQuantityAsync(buyerId, productId);
                return this.Ok(quantity);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving product quantity: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if a specific product is in the shopping cart for a specific buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>True if the product is in the cart; otherwise, false.</returns>
        [HttpGet("{buyerId}/items/{productId}/exists")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> IsProductInCart(int buyerId, int productId)
        {
            try
            {
                var exists = await this.shoppingCartRepository.IsProductInCartAsync(buyerId, productId);
                return this.Ok(exists);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while checking if the product is in the cart: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a product to the shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="productId">The ID of the product to add.</param>
        /// <param name="quantity">The quantity of the product to add.</param>
        /// <returns>No content if successful.</returns>
        [HttpPost("{buyerId}/items")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddToCart(int buyerId, [FromQuery] int productId, [FromQuery] int quantity)
        {
            if (quantity <= 0)
            {
                return this.BadRequest("Quantity must be greater than zero.");
            }

            try
            {
                await this.shoppingCartRepository.AddProductToCartAsync(buyerId, productId, quantity);
                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while adding the product to the cart: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the quantity of a product in the shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="productId">The ID of the product to update.</param>
        /// <param name="quantity">The new quantity of the product.</param>
        /// <returns>No content if successful.</returns>
        [HttpPut("{buyerId}/items/{productId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCartItem(int buyerId, int productId, [FromQuery] int quantity)
        {
            if (quantity <= 0)
            {
                return this.BadRequest("Quantity must be greater than zero.");
            }

            try
            {
                await this.shoppingCartRepository.UpdateProductQuantityAsync(buyerId, productId, quantity);
                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating the product quantity: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes a product from the shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="productId">The ID of the product to remove.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{buyerId}/items/{productId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveFromCart(int buyerId, int productId)
        {
            try
            {
                await this.shoppingCartRepository.RemoveProductFromCartAsync(buyerId, productId);
                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while removing the product from the cart: {ex.Message}");
            }
        }

        /// <summary>
        /// Clears all items from the shopping cart for a specific buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{buyerId}/items")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ClearCart(int buyerId)
        {
            try
            {
                await this.shoppingCartRepository.ClearCartAsync(buyerId);
                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while clearing the cart: {ex.Message}");
            }
        }
    }
}
