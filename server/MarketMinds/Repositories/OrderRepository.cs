// <copyright file="OrderRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Server.DataModels;
    using Server.DBConnection;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;
    using SharedClassLibrary.Shared;

    /// <summary>
    /// Represents a repository for managing orders in the database.
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        // private readonly string connectionString;
        // private readonly IDatabaseProvider databaseProvider;
        private readonly MarketPlaceDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public OrderRepository(MarketPlaceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Adds a new order to the database.
        /// </summary>
        /// <param name="productId">The ID of the product being ordered.</param>
        /// <param name="buyerId">The ID of the buyer placing the order.</param>
        /// <param name="productType">The type of product being ordered.</param>
        /// <param name="paymentMethod">The method of payment for the order.</param>
        /// <param name="orderSummaryId">The ID of the order summary for the order.</param>
        /// <param name="orderDate">The date and time of the order.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddOrderAsync(int productId, int buyerId, string productType, string paymentMethod, int orderSummaryId, DateTime orderDate)
        {
            // First, add a new OrderHistory record because we need the ID for the Order record
            OrderHistory orderHistory = new OrderHistory
            {
                OrderID = 0, // will be populated by the database
            };
            await this.dbContext.OrderHistory.AddAsync(orderHistory);
            await this.dbContext.SaveChangesAsync(); // Here the OrderHistory record is created and the orderHistory.OrderID is populated

            // Now, add the new Order
            Order order = new Order
            {
                OrderID = 0, // Will be populated by the database
                ProductID = productId,
                BuyerID = buyerId,
                ProductType = productType,
                PaymentMethod = paymentMethod,
                OrderSummaryID = orderSummaryId,
                OrderDate = DateTime.SpecifyKind(orderDate, DateTimeKind.Utc), // Ensure the order date is in UTC
                OrderHistoryID = orderHistory.OrderID, // The new order history record ID which was populated by the database
            };
            await this.dbContext.Orders.AddAsync(order);
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing order with new product type, payment method, and order date.
        /// </summary>
        /// <param name="orderId">The ID of the order to update.</param>
        /// <param name="productType">The new product type for the order.</param>
        /// <param name="paymentMethod">The new payment method for the order.</param>
        /// <param name="orderDate">The new order date for the order.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the order with the specified ID is not found.</exception>
        public async Task UpdateOrderAsync(int orderId, string productType, string paymentMethod, DateTime orderDate)
        {
            Order? order = await this.dbContext.Orders.FindAsync(orderId)
                                ?? throw new KeyNotFoundException($"UpdateOrderAsync: Order with ID {orderId} not found");

            order.ProductType = productType;
            order.PaymentMethod = paymentMethod;
            order.OrderDate = DateTime.SpecifyKind(orderDate, DateTimeKind.Utc);

            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes an order from the database.
        /// </summary>
        /// <param name="orderId">The ID of the order to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the order with the specified ID is not found.</exception>
        public async Task DeleteOrderAsync(int orderId)
        {
            Order? order = await this.dbContext.Orders.FindAsync(orderId)
                                ?? throw new KeyNotFoundException($"DeleteOrderAsync: Order with ID {orderId} not found");

            this.dbContext.Orders.Remove(order);
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves the borrowed order history for a buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A list of borrowed orders.</returns>
        public async Task<List<Order>> GetBorrowedOrderHistoryAsync(int buyerId)
        {
            List<Order> orders = await this.dbContext.Orders
                .Where(order => order.BuyerID == buyerId && order.ProductType == "borrowed")
                .OrderByDescending(order => order.OrderDate) // Exactly like in the get_borrowed_order_history stored procedure
                .ToListAsync();

            return orders;
        }

        /// <summary>
        /// Retrieves the new or used order history for a buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A list of new or used orders.</returns>
        public async Task<List<Order>> GetNewOrUsedOrderHistoryAsync(int buyerId)
        {
            List<Order> orders = await this.dbContext.Orders
                .Where(order => order.BuyerID == buyerId && (order.ProductType == "new" || order.ProductType == "used"))
                .OrderByDescending(order => order.OrderDate) // Exactly like in the get_new_or_used_order_history stored procedure
                .ToListAsync();

            return orders;
        }

        /// <summary>
        /// Retrieves orders by name for a buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="text">The text to search for.</param>
        /// <returns>A list of orders filtered by name.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the product with the specified ID is not found.</exception>
        public async Task<List<Order>> GetOrdersByNameAsync(int buyerId, string text)
        {
            List<Order> ordersFilteredByName = new List<Order>();

            // First fetch all orders from the buyer
            List<Order> buyerOrders = await this.dbContext.Orders
                .Where(order => order.BuyerID == buyerId)
                .ToListAsync();

            // Then, take the product from each order in the buyerOrders and filter them by name like in the stored procedure get_orders_by_name
            foreach (Order order in buyerOrders)
            {
                Product product = await this.dbContext.Products.FindAsync(order.ProductID)
                                        ?? throw new KeyNotFoundException($"GetOrdersByNameAsync: Product with ID {order.ProductID} not found");
                if (product.Name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0) // This is exactly like the sql server <LIKE '%@'+@text+'%'> (case insensitive)
                {
                    ordersFilteredByName.Add(order);
                }
            }

            return ordersFilteredByName;
        }

        /// <summary>
        /// Retrieves orders from 2024 for a buyer.
        /// OBS: This function and the one with 2025 should have the year parametrized.
        /// CAN BE REFACTORED.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A list of orders from 2024.</returns>
        public async Task<List<Order>> GetOrdersFrom2024Async(int buyerId)
        {
            List<Order> ordersFrom2024 = await this.dbContext.Orders
                .Where(order => order.BuyerID == buyerId && order.OrderDate.Year == 2024)
                .OrderByDescending(order => order.OrderDate)
                .ToListAsync();

            return ordersFrom2024;
        }

        /// <summary>
        /// Retrieves orders from 2025 for a buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A list of orders from 2025.</returns>
        public async Task<List<Order>> GetOrdersFrom2025Async(int buyerId)
        {
            List<Order> ordersFrom2025 = await this.dbContext.Orders
                .Where(order => order.BuyerID == buyerId && order.OrderDate.Year == 2025)
                .OrderByDescending(order => order.OrderDate)
                .ToListAsync();

            return ordersFrom2025;
        }

        /// <summary>
        /// Retrieves orders from the last six months for a buyer.
        /// OBS: The funcitons related to the number of months should have the number of months parametrized.
        /// CAN BE REFACTORED.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A list of orders from the last six months.</returns>
        public async Task<List<Order>> GetOrdersFromLastSixMonthsAsync(int buyerId)
        {
            List<Order> ordersFromLastSixMonths = await this.dbContext.Orders
                .Where(order => order.BuyerID == buyerId && order.OrderDate >= DateTime.Now.AddMonths(-6))
                .OrderByDescending(order => order.OrderDate)
                .ToListAsync();

            return ordersFromLastSixMonths;
        }

        /// <summary>
        /// Retrieves orders from the last three months for a buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A list of orders from the last three months.</returns>
        public async Task<List<Order>> GetOrdersFromLastThreeMonthsAsync(int buyerId)
        {
            List<Order> ordersFromLastThreeMonths = await this.dbContext.Orders
                .Where(order => order.BuyerID == buyerId && order.OrderDate >= DateTime.Now.AddMonths(-3))
                .OrderByDescending(order => order.OrderDate)
                .ToListAsync();

            return ordersFromLastThreeMonths;
        }

        /// <summary>
        /// Retrieves orders from an order history for a buyer.
        /// </summary>
        /// <param name="orderHistoryId">The ID of the order history.</param>
        /// <returns>A list of orders from the order history.</returns>
        public async Task<List<Order>> GetOrdersFromOrderHistoryAsync(int orderHistoryId)
        {
            List<Order> ordersFromOrderHistory = await this.dbContext.Orders
                .Where(order => order.OrderHistoryID == orderHistoryId)
                .ToListAsync();

            return ordersFromOrderHistory;
        }

        /// <summary>
        /// Retrieves orders with product information for a buyer.
        /// Filters orders by search text and time period only if present.
        /// </summary>
        /// <param name="userId">The ID of the buyer.</param>
        /// <param name="searchText">The text to search for.</param>
        /// <param name="timePeriod">The time period to filter by.</param>
        /// <returns>A list of orders with product information.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the product with the specified ID is not found.</exception>
        /// <exception cref="ArgumentException">Thrown when the time period is invalid.</exception>
        public async Task<List<OrderDisplayInfo>> GetOrdersWithProductInfoAsync(int userId, string? searchText = null, string? timePeriod = null)
        {
            List<Order> ordersDb = await this.dbContext.Orders
                .Where(order => order.BuyerID == userId)
                .ToListAsync();

            List<OrderDisplayInfo> orderDisplayInfos = new List<OrderDisplayInfo>();

            foreach (Order order in ordersDb)
            {
                Product product = await this.dbContext.Products.FindAsync(order.ProductID)
                                        ?? throw new KeyNotFoundException($"GetOrdersWithProductInfoAsync: Product with ID {order.ProductID} not found");

                // This boolean is used to check if the product name corresponds to the search text, if the search text is present.
                bool shouldIncludeProductBySearchText = searchText == null || (searchText != null && product.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);

                if (shouldIncludeProductBySearchText) // if searching by text corresponds, then we can check the time period
                {
                    switch (timePeriod)
                    {
                        case null:
                            orderDisplayInfos.Add(CreateOrderDisplayInfoFromOrderAndProduct(order, product));
                            break;
                        case "Last 3 Months" when order.OrderDate >= DateTime.Now.AddMonths(-3):
                            orderDisplayInfos.Add(CreateOrderDisplayInfoFromOrderAndProduct(order, product));
                            break;
                        case "Last 6 Months" when order.OrderDate >= DateTime.Now.AddMonths(-6):
                            orderDisplayInfos.Add(CreateOrderDisplayInfoFromOrderAndProduct(order, product));
                            break;
                        case "This Year" when order.OrderDate.Year == DateTime.Now.Year:
                            orderDisplayInfos.Add(CreateOrderDisplayInfoFromOrderAndProduct(order, product));
                            break;
                        default:
                            throw new ArgumentException($"GetOrdersWithProductInfoAsync: Invalid time period: {timePeriod}");
                    }
                }
            }

            return orderDisplayInfos;
        }

        /// <summary>
        /// Retrieves the product category types for a buyer.
        /// </summary>
        /// <param name="userId">The ID of the buyer.</param>
        /// <returns>A dictionary of order summary IDs and product category types.</returns>
        public async Task<Dictionary<int, string>> GetProductCategoryTypesAsync(int userId)
        {
            Dictionary<int, string> productCategoryTypes = new Dictionary<int, string>();

            List<Order> ordersDb = await this.dbContext.Orders
                .Where(order => order.BuyerID == userId)
                .ToListAsync();

            foreach (Order order in ordersDb)
            {
                string productCategory = (order.ProductType == "new" || order.ProductType == "used") ? "new" : "borrowed";
                productCategoryTypes.Add(order.OrderSummaryID, productCategory);
            }

            return productCategoryTypes;
        }

        /// <summary>
        /// Retrieves an order summary by its ID.
        /// </summary>
        /// <param name="orderSummaryId">The ID of the order summary.</param>
        /// <returns>The order summary.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the order summary with the specified ID is not found.</exception>
        public async Task<OrderSummary> GetOrderSummaryAsync(int orderSummaryId)
        {
            return await this.dbContext.OrderSummary.FindAsync(orderSummaryId)
                ?? throw new KeyNotFoundException($"GetOrderSummaryAsync: OrderSummary with ID {orderSummaryId} not found");
        }

        private static OrderDisplayInfo CreateOrderDisplayInfoFromOrderAndProduct(Order order, Product product)
        {
            string productCategory = (product.ProductType == "new" || product.ProductType == "used") ? "new" : "borrowed";

            return new OrderDisplayInfo
            {
                OrderID = order.OrderID,
                ProductName = product.Name,
                ProductTypeName = product.ProductType,
                OrderDate = order.OrderDate.ToString("yyyy-MM-dd"),
                PaymentMethod = order.PaymentMethod,
                OrderSummaryID = order.OrderSummaryID,
                ProductCategory = productCategory,
            };
        }
    }
}