// -----------------------------------------------------------------------
// <copyright file="ShoppingCartRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Server.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Server.DataModels;
    using Server.DBConnection;
    using SharedClassLibrary.IRepository;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// Repository for managing shopping cart operations in the database.
    /// </summary>
    public class ShoppingCartRepository : IShoppingCartRepository
    {
        // private DatabaseConnection databaseConnection;
        private readonly MarketPlaceDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShoppingCartRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context to be used by the repository.</param>
        public ShoppingCartRepository(MarketPlaceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Adds a product to the user's shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product to add to the cart. Must be a positive integer.</param>
        /// <param name="quantity">The quantity of the product to add. Must be greater than 0.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when buyer or product is not found or the quantity is not greater than 0.</exception>
        public async Task AddProductToCartAsync(int buyerId, int productId, int quantity)
        {
            bool buyerExists = await this.dbContext.Buyers.AnyAsync(buyer => buyer.Id == buyerId);
            if (!buyerExists)
            {
                throw new Exception($"AddProductToCartAsync: Buyer not found for the buyer id: {buyerId}");
            }

            bool productExists = await this.dbContext.Products.AnyAsync(product => product.ProductId == productId);
            if (!productExists)
            {
                throw new Exception($"AddProductToCartAsync: Product not found for the product id: {productId}");
            }

            if (quantity <= 0)
            {
                throw new Exception($"AddProductToCartAsync: Quantity must be greater than 0.");
            }

            //BuyerCartItemEntity buyerCartItem = new BuyerCartItemEntity(buyerId, productId, quantity);

            //this.dbContext.BuyerCartItems.Add(buyerCartItem);
            //await this.dbContext.SaveChangesAsync();

            // MODIFIED SO ADD TO CART DOESN'T CRASH WHEN THE PRODUCT IS ALREADY IN THE CART,
            // JUST INCREASE THE QUANTITY

            // Check if the product is already in the cart
            var existingCartItem = await this.dbContext.BuyerCartItems
                .FirstOrDefaultAsync(item => item.BuyerId == buyerId && item.ProductId == productId);

            if (existingCartItem != null)
            {
                // Update quantity if it already exists
                existingCartItem.Quantity += quantity;
            }
            else
            {
                // Add new cart item if it doesn't exist
                BuyerCartItemEntity buyerCartItem = new BuyerCartItemEntity(buyerId, productId, quantity);
                this.dbContext.BuyerCartItems.Add(buyerCartItem);
            }

            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Removes a product from the user's shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product to remove from the cart. Must be a positive integer.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when buyer cart item is not found.</exception>
        public async Task RemoveProductFromCartAsync(int buyerId, int productId)
        {
            BuyerCartItemEntity? buyerCartItemToRemove = await this.dbContext.BuyerCartItems.FindAsync(buyerId, productId)
                        ?? throw new Exception($"RemoveProductFromCartAsync: Buyer cart item not found for the buyer id: {buyerId} and product id: {productId}");
            this.dbContext.BuyerCartItems.Remove(buyerCartItemToRemove);
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Updates the quantity of a product in the user's shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product to update. Must be a positive integer.</param>
        /// <param name="quantity">The new quantity. Must be greater than 0.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when buyer cart item to update is not found.</exception>
        public async Task UpdateProductQuantityAsync(int buyerId, int productId, int quantity)
        {
            BuyerCartItemEntity? buyerCartItemToUpdate = await this.dbContext.BuyerCartItems.FindAsync(buyerId, productId)
                        ?? throw new Exception($"UpdateProductQuantityAsync: Buyer cart item not found for the buyer id: {buyerId} and product id: {productId}");
            buyerCartItemToUpdate.Quantity = quantity;
            await this.dbContext.SaveChangesAsync();
        }

        // OLD METHOD

        /// <summary>
        /// Gets all products in the user's shopping cart with their quantities.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer. Must be a positive integer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list with products.</returns>
        /// <exception cref="Exception">Thrown when product is not found.</exception>
        //public async Task<List<Product>> GetCartItemsAsync(int buyerId)
        //{
        //    List<BuyerCartItemEntity> buyerCartItems = await this.dbContext.BuyerCartItems
        //        .Where(buyerCartItem => buyerCartItem.BuyerId == buyerId).ToListAsync();

        //    List<Product> products = new List<Product>();
        //    foreach (BuyerCartItemEntity buyerCartItem in buyerCartItems)
        //    {
        //        Product product = await this.dbContext.Products.FindAsync(buyerCartItem.ProductId)
        //            ?? throw new Exception($"GetCartItemsAsync: Product not found for the product id: {buyerCartItem.ProductId}");
        //        products.Add(product);
        //    }

        //    return products;
        //}



        // MODIFIED THIS METHOD SO THE PRODUCTS RETURNED HAVE THE QUANTITY FROM THE CART NOT THEIR STOCK
        // BEST SOLUTION WOULD BE TO USE THE BuyerCartItemEntity somehow... 

        /// <summary>
        /// Gets all products in the user's shopping cart with their quantities.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer. Must be a positive integer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list with products.</returns>
        /// <exception cref="Exception">Thrown when product is not found.</exception>
        public async Task<List<Product>> GetCartItemsAsync(int buyerId)
        {
            List<BuyerCartItemEntity> buyerCartItems = await this.dbContext.BuyerCartItems
                .Where(buyerCartItem => buyerCartItem.BuyerId == buyerId).ToListAsync();

            List<Product> products = new List<Product>();
            foreach (BuyerCartItemEntity buyerCartItem in buyerCartItems)
            {
                Product product = await this.dbContext.Products.FindAsync(buyerCartItem.ProductId)
                    ?? throw new Exception($"GetCartItemsAsync: Product not found for the product id: {buyerCartItem.ProductId}");

                // Create a new product with the quantity from the cart instead of the actual stock
                Product productWithCartQuantity = new Product(
                    product.ProductId,
                    product.Name,
                    product.Description,
                    product.Price,
                    buyerCartItem.Quantity,  // Use the cart quantity instead of actual stock
                    product.SellerId
                );

                // Copy any additional properties that might be needed
                productWithCartQuantity.ProductType = product.ProductType;
                productWithCartQuantity.StartDate = product.StartDate;
                productWithCartQuantity.EndDate = product.EndDate;

                products.Add(productWithCartQuantity);
            }

            return products;
        }


        /// <summary>
        /// Clears all items from the user's shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer. Must be a positive integer.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ClearCartAsync(int buyerId)
        {
            this.dbContext.BuyerCartItems.RemoveRange(this.dbContext.BuyerCartItems.Where(buyerCartItem => buyerCartItem.BuyerId == buyerId));
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Gets the total number of items in the user's shopping cart (sum of all quantities).
        /// </summary>
        /// <param name="buyerId">The ID of the buyer. Must be a positive integer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total number of items.</returns>
        public async Task<int> GetCartItemCountAsync(int buyerId)
        {
            List<BuyerCartItemEntity> buyerCartItems = await this.dbContext.BuyerCartItems
                .Where(buyerCartItem => buyerCartItem.BuyerId == buyerId).ToListAsync();
            return buyerCartItems.Sum(buyerCartItem => buyerCartItem.Quantity);
        }

        /// <summary>
        /// Checks if a specific product is in the buyer's cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product to check. Must be a positive integer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the product is in the cart, otherwise false.</returns>
        public async Task<bool> IsProductInCartAsync(int buyerId, int productId)
        {
            return await this.dbContext.BuyerCartItems.AnyAsync(buyerCartItem => buyerCartItem.BuyerId == buyerId && buyerCartItem.ProductId == productId);
        }

        /// <summary>
        /// Gets the quantity of a specific product in the buyer's cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product to check. Must be a positive integer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is the quantity of the product in the cart, or 0 if it's not in the cart.</returns>
        /// <exception cref="Exception">Thrown when buyer cart item is not found.</exception>
        public async Task<int> GetProductQuantityAsync(int buyerId, int productId)
        {
            BuyerCartItemEntity? buyerCartItem = await this.dbContext.BuyerCartItems.FindAsync(buyerId, productId)
                        ?? throw new Exception($"GetProductQuantityAsync: Buyer cart item not found for the buyer id: {buyerId} and product id: {productId}");
            return buyerCartItem.Quantity;
        }
    }
}
