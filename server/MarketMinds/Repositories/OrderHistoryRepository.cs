// <copyright file="OrderHistoryRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using global::MarketMinds.Shared.IRepository;
    using global::MarketMinds.Shared.Models;
    using Microsoft.EntityFrameworkCore;
    using Server.DataAccessLayer;

    /// <summary>
    /// Provides database operations for order history management.
    /// </summary>
    public class OrderHistoryRepository : IOrderHistoryRepository
    {
        // private readonly string connectionString;
        // private readonly IDatabaseProvider databaseProvider;
        private readonly ApplicationDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderHistoryRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public OrderHistoryRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task<List<Product>> GetProductsFromOrderHistoryAsync(int orderHistoryId)
        {
            List<Product> products = new List<Product>();

            List<Order> orders = await this.dbContext.Orders.Where(order => order.OrderHistoryID == orderHistoryId).ToListAsync();

            foreach (Order order in orders)
            {
                Product? product = await this.dbContext.BuyProducts.FindAsync(order.ProductID);

                // here if for example a product is deleted but the order is still in the database, the product will be null
                // and it will not be added to the list
                // (could create a placeholder product in the future to display to the user the fact that the product is deleted)
                if (product != null)
                {
                    products.Add(product);
                }
            }

            return products;
        }

        /// <summary>
        /// Creates a new order history record in the database.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer for this order history.</param>
        /// <returns>A task representing the asynchronous operation that returns the ID of the newly created order history.</returns>
        public async Task<int> CreateOrderHistoryAsync(int buyerId)
        {
            try
            {
                var orderHistory = new OrderHistory
                {
                    BuyerID = buyerId,
                    CreatedAt = DateTime.UtcNow,
                    ShippingAddress = string.Empty,
                    PaymentMethod = string.Empty
                };
                this.dbContext.OrderHistory.Add(orderHistory);
                try
                {
                    await this.dbContext.SaveChangesAsync();

                }
                catch (Exception ex)
                {
                    throw;
                }

                return orderHistory.OrderID;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Gets all order histories for a specific buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A list of order histories for the specified buyer.</returns>
        public async Task<List<OrderHistory>> GetOrderHistoriesByBuyerAsync(int buyerId)
        {
            return await this.dbContext.OrderHistory
                .Where(history => history.BuyerID == buyerId)
                .ToListAsync();
        }

        /// <summary>
        /// Gets an order history by its ID.
        /// </summary>
        /// <param name="orderHistoryId">The ID of the order history to retrieve.</param>
        /// <returns>The order history with the specified ID.</returns>
        /// <exception cref="Exception">Thrown when the order history with the specified ID is not found.</exception>
        public async Task<OrderHistory> GetOrderHistoryByIdAsync(int orderHistoryId)
        {
            OrderHistory? orderHistory = await this.dbContext.OrderHistory.FindAsync(orderHistoryId);

            if (orderHistory == null)
            {
                throw new Exception($"GetOrderHistoryByIdAsync: No OrderHistory with id: {orderHistoryId}");
            }

            return orderHistory;
        }

        /// <summary>
        /// Updates an order history with new information.
        /// </summary>
        /// <param name="orderHistoryId">The ID of the order history to update.</param>
        /// <param name="shippingAddress">The new shipping address.</param>
        /// <param name="paymentMethod">The new payment method.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when the order history with the specified ID is not found.</exception>
        public async Task UpdateOrderHistoryAsync(int orderHistoryId, string shippingAddress, string paymentMethod)
        {
            OrderHistory? orderHistory = await this.dbContext.OrderHistory.FindAsync(orderHistoryId);

            if (orderHistory == null)
            {
                throw new Exception($"UpdateOrderHistoryAsync: No OrderHistory with id: {orderHistoryId}");
            }

            orderHistory.ShippingAddress = shippingAddress;
            orderHistory.PaymentMethod = paymentMethod;

            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an order history with new information including notes.
        /// </summary>
        /// <param name="orderHistoryId">The ID of the order history to update.</param>
        /// <param name="note">The note to add to the order history.</param>
        /// <param name="shippingAddress">The new shipping address.</param>
        /// <param name="paymentMethod">The new payment method.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task UpdateOrderHistoryAsync(int orderHistoryId, string note, string shippingAddress, string paymentMethod)
        {
            OrderHistory? orderHistory = await this.dbContext.OrderHistory.FindAsync(orderHistoryId);

            if (orderHistory == null)
            {
                throw new Exception($"UpdateOrderHistoryAsync: No OrderHistory with id: {orderHistoryId}");
            }

            orderHistory.Note = note;
            orderHistory.ShippingAddress = shippingAddress;
            orderHistory.PaymentMethod = paymentMethod;

            await this.dbContext.SaveChangesAsync();
        }
    }
}