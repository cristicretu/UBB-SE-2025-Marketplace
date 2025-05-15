// -----------------------------------------------------------------------
// <copyright file="ShoppingCartService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SharedClassLibrary.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;
    using SharedClassLibrary.ProxyRepository;
    using SharedClassLibrary.Helper;


    /// <summary>
    /// Service for managing shopping cart operations.
    /// </summary>
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IShoppingCartRepository shoppingCartRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShoppingCartService"/> class with dependency injection.
        /// </summary>
        /// <param name="shoppingCartRepository">The repository used for shopping cart operations.</param>
        public ShoppingCartService(IShoppingCartRepository shoppingCartRepository)
        {
            this.shoppingCartRepository = shoppingCartRepository ?? throw new ArgumentNullException(nameof(shoppingCartRepository));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShoppingCartService"/> class. Default constructor for ShoppingCartService.
        /// </summary>
        /// <remarks>
        /// Creates a new instance with the default repository using the application's connection string.
        /// </remarks>
        public ShoppingCartService()
        {
            this.shoppingCartRepository = new ShoppingCartProxyRepository(AppConfig.GetBaseApiUrl());
        }

        /// <summary>
        /// Adds a product to the buyer's shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="productId">The ID of the product to add to the cart.</param>
        /// <param name="quantity">The quantity of the product to add. Default is 1.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown if the buyer ID, product ID, or quantity is invalid.</exception>
        public async Task AddProductToCartAsync(int buyerId, int productId, int quantity = 1)
        {
            if (buyerId <= 0)
            {
                throw new ArgumentException("Buyer ID must be a positive integer.", nameof(buyerId));
            }

            if (productId <= 0)
            {
                throw new ArgumentException("Product ID must be a positive integer.", nameof(productId));
            }

            if (quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than 0.", nameof(quantity));
            }

            await this.shoppingCartRepository.AddProductToCartAsync(buyerId, productId, quantity);
        }

        /// <summary>
        /// Removes a product from the buyer's shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="productId">The ID of the product to remove from the cart.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown if the buyer ID or product ID is invalid.</exception>
        public async Task RemoveProductFromCartAsync(int buyerId, int productId)
        {
            if (buyerId <= 0)
            {
                throw new ArgumentException("Buyer ID must be a positive integer.", nameof(buyerId));
            }

            if (productId <= 0)
            {
                throw new ArgumentException("Product ID must be a positive integer.", nameof(productId));
            }

            await this.shoppingCartRepository.RemoveProductFromCartAsync(buyerId, productId);
        }

        /// <summary>
        /// Updates the quantity of a product in the buyer's shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="productId">The ID of the product to update.</param>
        /// <param name="quantity">The new quantity.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown if the buyer ID, product ID, or quantity is invalid.</exception>
        public async Task UpdateProductQuantityAsync(int buyerId, int productId, int quantity)
        {
            if (buyerId <= 0)
            {
                throw new ArgumentException("Buyer ID must be a positive integer.", nameof(buyerId));
            }

            if (productId <= 0)
            {
                throw new ArgumentException("Product ID must be a positive integer.", nameof(productId));
            }

            if (quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than 0.", nameof(quantity));
            }

            await this.shoppingCartRepository.UpdateProductQuantityAsync(buyerId, productId, quantity);
        }

        /// <summary>
        /// Increments the quantity of a product in the buyer's shopping cart by 1.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="productId">The ID of the product to increment.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown if the buyer ID or product ID is invalid.</exception>
        public async Task IncrementProductQuantityAsync(int buyerId, int productId)
        {
            if (buyerId <= 0)
            {
                throw new ArgumentException("Buyer ID must be a positive integer.", nameof(buyerId));
            }

            if (productId <= 0)
            {
                throw new ArgumentException("Product ID must be a positive integer.", nameof(productId));
            }

            // Check if the product is already in the cart
            if (await this.shoppingCartRepository.IsProductInCartAsync(buyerId, productId))
            {
                int currentQuantity = await this.shoppingCartRepository.GetProductQuantityAsync(buyerId, productId);
                await this.shoppingCartRepository.UpdateProductQuantityAsync(buyerId, productId, currentQuantity + 1);
            }
            else
            {
                await this.shoppingCartRepository.AddProductToCartAsync(buyerId, productId, 1);
            }
        }

        /// <summary>
        /// Decrements the quantity of a product in the buyer's shopping cart by 1.
        /// If the quantity becomes 0, the product is removed from the cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="productId">The ID of the product to decrement.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown if the buyer ID or product ID is invalid.</exception>
        public async Task DecrementProductQuantityAsync(int buyerId, int productId)
        {
            if (buyerId <= 0)
            {
                throw new ArgumentException("Buyer ID must be a positive integer.", nameof(buyerId));
            }

            if (productId <= 0)
            {
                throw new ArgumentException("Product ID must be a positive integer.", nameof(productId));
            }

            int currentQuantity = await this.shoppingCartRepository.GetProductQuantityAsync(buyerId, productId);

            if (currentQuantity > 1)
            {
                await this.shoppingCartRepository.UpdateProductQuantityAsync(buyerId, productId, currentQuantity - 1);
            }
            else if (currentQuantity == 1)
            {
                await this.shoppingCartRepository.RemoveProductFromCartAsync(buyerId, productId);
            }
        }

        /// <summary>
        /// Gets all products in the buyer's shopping cart with their quantities.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list with product objects.</returns>
        /// <exception cref="ArgumentException">Thrown if the buyer ID is invalid.</exception>
        public async Task<List<Product>> GetCartItemsAsync(int buyerId)
        {
            if (buyerId <= 0)
            {
                throw new ArgumentException("Buyer ID must be a positive integer.", nameof(buyerId));
            }

            return await this.shoppingCartRepository.GetCartItemsAsync(buyerId);
        }

        /// <summary>
        /// Gets the price of a specific product.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the price of the product.</returns>
        /// <exception cref="ArgumentException">Thrown if the product ID is invalid.</exception>
        /// <exception cref="Exception">Thrown when the product is not found.</exception>
        public async Task<double> GetProductPriceAsync(int buyerId, int productId)
        {
            if (productId <= 0)
            {
                throw new ArgumentException("Product ID must be a positive integer.", nameof(productId));
            }

            // Try to find the product in cart items first (if it's already in someone's cart)
            var products = await this.shoppingCartRepository.GetCartItemsAsync(buyerId);
            var product = products.FirstOrDefault(p => p.ProductId == productId);

            if (product == null)
            {
                throw new Exception($"Product not found for the ID: {productId}");
            }

            return product.Price;
        }

        /// <summary>
        /// Gets the total price of all items in the buyer's shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total price.</returns>
        /// <exception cref="ArgumentException">Thrown if the buyer ID is invalid.</exception>
        public async Task<double> GetCartTotalAsync(int buyerId)
        {
            if (buyerId <= 0)
            {
                throw new ArgumentException("Buyer ID must be a positive integer.", nameof(buyerId));
            }

            var cartItems = await this.shoppingCartRepository.GetCartItemsAsync(buyerId);
            double total = 0;

            foreach (var item in cartItems)
            {
                total += item.Price * item.Stock;
            }

            return total;
        }

        /// <summary>
        /// Clears all items from the buyer's shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown if the buyer ID is invalid.</exception>
        public async Task ClearCartAsync(int buyerId)
        {
            if (buyerId <= 0)
            {
                throw new ArgumentException("Buyer ID must be a positive integer.", nameof(buyerId));
            }

            await this.shoppingCartRepository.ClearCartAsync(buyerId);
        }

        /// <summary>
        /// Gets the total number of items in the buyer's shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total number of items.</returns>
        /// <exception cref="ArgumentException">Thrown if the buyer ID is invalid.</exception>
        public async Task<int> GetCartItemCountAsync(int buyerId)
        {
            if (buyerId <= 0)
            {
                throw new ArgumentException("Buyer ID must be a positive integer.", nameof(buyerId));
            }

            return await this.shoppingCartRepository.GetCartItemCountAsync(buyerId);
        }

        /// <summary>
        /// Checks if a specific product is in the buyer's cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="productId">The ID of the product to check.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the product is in the cart, otherwise false.</returns>
        /// <exception cref="ArgumentException">Thrown when buyerId or productId is invalid.</exception>
        public async Task<bool> IsProductInCartAsync(int buyerId, int productId)
        {
            if (buyerId <= 0)
            {
                throw new ArgumentException("Buyer ID must be a positive integer.", nameof(buyerId));
            }

            if (productId <= 0)
            {
                throw new ArgumentException("Product ID must be a positive integer.", nameof(productId));
            }

            return await this.shoppingCartRepository.IsProductInCartAsync(buyerId, productId);
        }
    }
}
