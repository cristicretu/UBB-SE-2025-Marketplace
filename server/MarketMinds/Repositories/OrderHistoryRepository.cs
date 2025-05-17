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
        /// <returns>A task representing the asynchronous operation that returns the ID of the newly created order history.</returns>
        public async Task<int> CreateOrderHistoryAsync()
        {
            throw new NotImplementedException("The CreateOrderHistoryAsync method is not implemented.");
        }
    }
}