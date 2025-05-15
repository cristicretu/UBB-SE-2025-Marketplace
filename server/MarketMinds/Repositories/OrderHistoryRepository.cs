// <copyright file="OrderHistoryRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Server.DBConnection;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// Provides database operations for order history management.
    /// </summary>
    public class OrderHistoryRepository : IOrderHistoryRepository
    {
        // private readonly string connectionString;
        // private readonly IDatabaseProvider databaseProvider;
        private readonly MarketPlaceDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderHistoryRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public OrderHistoryRepository(MarketPlaceDbContext dbContext)
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
                Product? product = await this.dbContext.Products.FindAsync(order.ProductID);

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
    }
}