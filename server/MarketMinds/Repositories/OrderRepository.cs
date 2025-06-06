// <copyright file="OrderRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using global::MarketMinds.Shared.IRepository;
    using global::MarketMinds.Shared.Models;
    using Microsoft.EntityFrameworkCore;
    using Server.DataAccessLayer;

    /// <summary>
    /// Represents a repository for managing orders in the database.
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        // private readonly string connectionString;
        // private readonly IDatabaseProvider databaseProvider;
        private readonly ApplicationDbContext dbContext;
        private readonly INotificationRepository notificationRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="notificationRepository">The notification repository.</param>
        public OrderRepository(ApplicationDbContext dbContext, INotificationRepository notificationRepository)
        {
            this.dbContext = dbContext;
            this.notificationRepository = notificationRepository;
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
            try
            {
                // First, add a new OrderHistory record because we need the ID for the Order record
                OrderHistory orderHistory = new OrderHistory
                {
                    OrderID = 0, // will be populated by the database
                    BuyerID = buyerId, // Set the BuyerID for the order history
                    CreatedAt = DateTime.UtcNow, // Ensure we use UTC time for consistency
                };

                await this.dbContext.OrderHistory.AddAsync(orderHistory);
                await this.dbContext.SaveChangesAsync();

                // Get product details based on product type
                string productName = "Unknown";
                double productCost = 0;
                int sellerId = 0;

                if (productType.Equals("new", StringComparison.OrdinalIgnoreCase) ||
                    productType.Equals("used", StringComparison.OrdinalIgnoreCase))
                {
                    var buyProduct = await this.dbContext.BuyProducts.FindAsync(productId);
                    if (buyProduct != null)
                    {
                        productName = buyProduct.Title;
                        productCost = buyProduct.Price;
                        sellerId = buyProduct.SellerId;
                    }
                    else
                    {
                        throw new KeyNotFoundException($"Buy product with ID {productId} not found");
                    }
                }
                else if (productType.Equals("borrowed", StringComparison.OrdinalIgnoreCase))
                {
                    var borrowProduct = await this.dbContext.BorrowProducts.FindAsync(productId);
                    if (borrowProduct != null)
                    {
                        productName = borrowProduct.Title;
                        var duration = borrowProduct.EndDate - borrowProduct.StartDate;
                        int days = duration.HasValue ? (int)Math.Ceiling(duration.Value.TotalDays) : 0;
                        productCost = days * borrowProduct.DailyRate;
                        sellerId = borrowProduct.SellerId;
                    }
                    else
                    {
                        throw new KeyNotFoundException($"Borrow product with ID {productId} not found");
                    }
                }
                else
                {
                    throw new ArgumentException($"Invalid product type: {productType}");
                }

                // Now, add the new Order
                Order order = new Order
                {
                    Id = 0, // Will be populated by the database
                    Name = productName,
                    Description = "Order for " + productName,
                    Cost = productCost,
                    SellerId = sellerId,
                    ProductID = productId,
                    BuyerId = buyerId,
                    ProductType = productType,
                    PaymentMethod = paymentMethod,
                    OrderSummaryID = orderSummaryId,
                    OrderDate = DateTime.SpecifyKind(orderDate, DateTimeKind.Utc), // Ensure the order date is in UTC
                    OrderHistoryID = orderHistory.OrderID, // The new order history record ID which was populated by the database
                };

                await this.dbContext.Orders.AddAsync(order);
                await this.dbContext.SaveChangesAsync();

                // Create and send payment confirmation notification to the buyer
                PaymentConfirmationNotification notification = new PaymentConfirmationNotification(
                    recipientId: buyerId,
                    timestamp: DateTime.Now,
                    productId: productId,
                    orderId: order.Id,
                    isRead: false);

                await this.notificationRepository.AddNotification(notification);
            }
            catch (Exception ex)
            {
                throw;
            }
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
                .Where(order => order.BuyerId == buyerId && order.ProductType == "borrowed")
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
                .Where(order => order.BuyerId == buyerId && (order.ProductType == "new" || order.ProductType == "used"))
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
                .Where(order => order.BuyerId == buyerId)
                .ToListAsync();

            // Then, take the product from each order in the buyerOrders and filter them by name like in the stored procedure get_orders_by_name
            foreach (Order order in buyerOrders)
            {
                Product product = await this.dbContext.BuyProducts.FindAsync(order.ProductID)
                                        ?? throw new KeyNotFoundException($"GetOrdersByNameAsync: Product with ID {order.ProductID} not found");
                if (product.Title.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0) // This is exactly like the sql server <LIKE '%@'+@text+'%'> (case insensitive)
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
                .Where(order => order.BuyerId == buyerId && order.OrderDate.Year == 2024)
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
                .Where(order => order.BuyerId == buyerId && order.OrderDate.Year == 2025)
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
                .Where(order => order.BuyerId == buyerId && order.OrderDate >= DateTime.Now.AddMonths(-6))
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
                .Where(order => order.BuyerId == buyerId && order.OrderDate >= DateTime.Now.AddMonths(-3))
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
        /// Retrieves orders with product information for a user (buyer or seller).
        /// Filters orders by search text and time period only if present.
        /// </summary>
        /// <param name="userId">The ID of the user (buyer or seller).</param>
        /// <param name="searchText">The text to search for.</param>
        /// <param name="timePeriod">The time period to filter by.</param>
        /// <returns>A list of orders with product information.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the product with the specified ID is not found.</exception>
        /// <exception cref="ArgumentException">Thrown when the time period is invalid.</exception>
        public async Task<List<OrderDisplayInfo>> GetOrdersWithProductInfoAsync(int userId, string? searchText = null, string? timePeriod = null)
        {
            // Check if the user is a seller by looking in the Sellers table
            bool isUserSeller = await this.dbContext.Sellers.AnyAsync(s => s.Id == userId);

            // Debug logging
            Console.WriteLine($"[DEBUG] GetOrdersWithProductInfoAsync - UserId: {userId}, IsUserSeller: {isUserSeller}");

            List<Order> ordersDb;
            if (isUserSeller)
            {
                // For sellers, get orders where they are the seller
                ordersDb = await this.dbContext.Orders
                    .Where(order => order.SellerId == userId)
                    .ToListAsync();
                Console.WriteLine($"[DEBUG] Seller query found {ordersDb.Count} orders");
            }
            else
            {
                // For buyers, get orders where they are the buyer
                ordersDb = await this.dbContext.Orders
                    .Where(order => order.BuyerId == userId)
                    .ToListAsync();
                Console.WriteLine($"[DEBUG] Buyer query found {ordersDb.Count} orders");
            }

            List<OrderDisplayInfo> orderDisplayInfos = new List<OrderDisplayInfo>();

            foreach (Order order in ordersDb)
            {
                Product product = await this.dbContext.BuyProducts.FindAsync(order.ProductID)
                                        ?? throw new KeyNotFoundException($"GetOrdersWithProductInfoAsync: Product with ID {order.ProductID} not found");

                Condition condition = await this.dbContext.ProductConditions.FindAsync(product.ConditionId)
                                        ?? throw new KeyNotFoundException($"GetOrdersWithProductInfoAsync: Condition with ID {product.ConditionId} not found");

                Category category = await this.dbContext.ProductCategories.FindAsync(product.CategoryId)
                                        ?? throw new KeyNotFoundException($"GetOrdersWithProductInfoAsync: Category with ID {product.CategoryId} not found");

                // This boolean is used to check if the search text has common words with order information
                bool shouldIncludeProductBySearchText = searchText == null;

                if (searchText != null)
                {
                    // Create a single searchable string with all order information
                    string orderSearchString = $"{product.Title} {order.Id} {condition.Name} {category.Name} {order.PaymentMethod}";

                    // Split search text into words and check for common words
                    var searchWords = searchText.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    var orderWords = orderSearchString.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    shouldIncludeProductBySearchText = searchWords.Any(searchWord =>
                        orderWords.Any(orderWord =>
                            orderWord.IndexOf(searchWord, StringComparison.OrdinalIgnoreCase) >= 0));
                }

                if (shouldIncludeProductBySearchText) // if searching by text corresponds, then we can check the time period
                {
                    bool shouldIncludeByTimePeriod = false;

                    switch (timePeriod)
                    {
                        case null:
                            shouldIncludeByTimePeriod = true;
                            break;
                        case "Last 3 Months" when order.OrderDate >= DateTime.Now.AddMonths(-3):
                            shouldIncludeByTimePeriod = true;
                            break;
                        case "Last 6 Months" when order.OrderDate >= DateTime.Now.AddMonths(-6):
                            shouldIncludeByTimePeriod = true;
                            break;
                        case "This Year" when order.OrderDate.Year == DateTime.Now.Year:
                            shouldIncludeByTimePeriod = true;
                            break;
                        // Both cases are valid, the first one is used on web and the second one on desktop
                        case "all":
                        case "All Orders":
                            shouldIncludeByTimePeriod = true;
                            break;
                        default:
                            throw new ArgumentException($"GetOrdersWithProductInfoAsync: Invalid time period: {timePeriod}");
                    }

                    if (shouldIncludeByTimePeriod)
                    {
                        orderDisplayInfos.Add(CreateOrderDisplayInfoFromOrderAndProduct(order, product, condition, category));
                    }
                }
            }

            return orderDisplayInfos;
        }

        /// <summary>
        /// Retrieves orders with product information for a user (buyer or seller) with pagination.
        /// Filters orders by search text and time period only if present.
        /// </summary>
        /// <param name="userId">The ID of the user (buyer or seller).</param>
        /// <param name="offset">The number of orders to skip.</param>
        /// <param name="count">The number of orders to take.</param>
        /// <param name="searchText">The text to search for.</param>
        /// <param name="timePeriod">The time period to filter by.</param>
        /// <returns>A list of orders with product information.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the product with the specified ID is not found.</exception>
        /// <exception cref="ArgumentException">Thrown when the time period is invalid.</exception>
        public async Task<List<OrderDisplayInfo>> GetOrdersWithProductInfoAsync(int userId, int offset, int count, string? searchText = null, string? timePeriod = null)
        {
            // Check if the user is a seller by looking in the Sellers table
            bool isUserSeller = await this.dbContext.Sellers.AnyAsync(s => s.Id == userId);

            // Debug logging
            Console.WriteLine($"[DEBUG] GetOrdersWithProductInfoAsync (Paginated) - UserId: {userId}, IsUserSeller: {isUserSeller}, Offset: {offset}, Count: {count}");

            List<Order> ordersDb;
            if (isUserSeller)
            {
                // For sellers, get orders where they are the seller
                ordersDb = await this.dbContext.Orders
                    .Where(order => order.SellerId == userId)
                    .OrderByDescending(order => order.OrderDate)
                    .ToListAsync();
                Console.WriteLine($"[DEBUG] Seller query found {ordersDb.Count} orders");
            }
            else
            {
                // For buyers, get orders where they are the buyer
                ordersDb = await this.dbContext.Orders
                    .Where(order => order.BuyerId == userId)
                    .OrderByDescending(order => order.OrderDate)
                    .ToListAsync();
                Console.WriteLine($"[DEBUG] Buyer query found {ordersDb.Count} orders");
            }

            List<OrderDisplayInfo> allOrderDisplayInfos = new List<OrderDisplayInfo>();

            foreach (Order order in ordersDb)
            {
                Product? product = null;

                // Try to find the product in different tables based on product type
                if (order.ProductType == "new" || order.ProductType == "used")
                {
                    product = await this.dbContext.BuyProducts.FindAsync(order.ProductID);
                }
                else if (order.ProductType == "borrowed")
                {
                    product = await this.dbContext.BorrowProducts.FindAsync(order.ProductID);
                }

                if (product == null)
                {
                    // Try auction products as fallback
                    product = await this.dbContext.AuctionProducts.FindAsync(order.ProductID);
                }

                if (product == null)
                {
                    Console.WriteLine($"[WARNING] Product with ID {order.ProductID} not found for order {order.Id}");
                    continue; // Skip this order if product not found
                }

                Condition condition = await this.dbContext.ProductConditions.FindAsync(product.ConditionId)
                                        ?? throw new KeyNotFoundException($"GetOrdersWithProductInfoAsync: Condition with ID {product.ConditionId} not found");

                Category category = await this.dbContext.ProductCategories.FindAsync(product.CategoryId)
                                        ?? throw new KeyNotFoundException($"GetOrdersWithProductInfoAsync: Category with ID {product.CategoryId} not found");

                // This boolean is used to check if the product name corresponds to the search text, if the search text is present.
                bool shouldIncludeProductBySearchText = searchText == null;

                if (searchText != null)
                {
                    // Create a single searchable string with all order information
                    string orderSearchString = $"{product.Title} {order.Id} {condition.Name} {category.Name} {order.PaymentMethod}";

                    // Split search text into words and check for common words
                    var searchWords = searchText.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    var orderWords = orderSearchString.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    shouldIncludeProductBySearchText = searchWords.Any(searchWord =>
                        orderWords.Any(orderWord =>
                            orderWord.IndexOf(searchWord, StringComparison.OrdinalIgnoreCase) >= 0));
                }

                if (shouldIncludeProductBySearchText) // if searching by text corresponds, then we can check the time period
                {
                    bool shouldIncludeByTimePeriod = false;

                    switch (timePeriod)
                    {
                        case null:
                        case "all":
                        case "All Orders":
                            shouldIncludeByTimePeriod = true;
                            break;
                        case "Last 3 Months":
                            shouldIncludeByTimePeriod = order.OrderDate >= DateTime.Now.AddMonths(-3);
                            break;
                        case "Last 6 Months":
                            shouldIncludeByTimePeriod = order.OrderDate >= DateTime.Now.AddMonths(-6);
                            break;
                        case "This Year":
                            shouldIncludeByTimePeriod = order.OrderDate.Year == DateTime.Now.Year;
                            break;
                        default:
                            throw new ArgumentException($"GetOrdersWithProductInfoAsync: Invalid time period: {timePeriod}");
                    }

                    if (shouldIncludeByTimePeriod)
                    {
                        allOrderDisplayInfos.Add(CreateOrderDisplayInfoFromOrderAndProduct(order, product, condition, category));
                    }
                }
            }

            // Apply pagination
            var paginatedOrders = allOrderDisplayInfos
                .Skip(offset)
                .Take(count)
                .ToList();

            Console.WriteLine($"[DEBUG] Returning {paginatedOrders.Count} orders out of {allOrderDisplayInfos.Count} total filtered orders");
            return paginatedOrders;
        }

        /// <summary>
        /// Gets the total count of orders for a user with optional filtering.
        /// </summary>
        /// <param name="userId">The ID of the user (buyer or seller).</param>
        /// <param name="searchText">The text to search for.</param>
        /// <param name="timePeriod">The time period to filter by.</param>
        /// <returns>The total count of orders matching the criteria.</returns>
        public async Task<int> GetOrdersCountAsync(int userId, string? searchText = null, string? timePeriod = null)
        {
            // Check if the user is a seller by looking in the Sellers table
            bool isUserSeller = await this.dbContext.Sellers.AnyAsync(s => s.Id == userId);

            List<Order> ordersDb;
            if (isUserSeller)
            {
                // For sellers, get orders where they are the seller
                ordersDb = await this.dbContext.Orders
                    .Where(order => order.SellerId == userId)
                    .ToListAsync();
            }
            else
            {
                // For buyers, get orders where they are the buyer
                ordersDb = await this.dbContext.Orders
                    .Where(order => order.BuyerId == userId)
                    .ToListAsync();
            }

            int count = 0;

            foreach (Order order in ordersDb)
            {
                Product? product = null;

                // Try to find the product in different tables based on product type
                if (order.ProductType == "new" || order.ProductType == "used")
                {
                    product = await this.dbContext.BuyProducts.FindAsync(order.ProductID);
                }
                else if (order.ProductType == "borrowed")
                {
                    product = await this.dbContext.BorrowProducts.FindAsync(order.ProductID);
                }

                if (product == null)
                {
                    // Try auction products as fallback
                    product = await this.dbContext.AuctionProducts.FindAsync(order.ProductID);
                }

                if (product == null)
                {
                    continue; // Skip this order if product not found
                }

                // This boolean is used to check if the product name corresponds to the search text, if the search text is present.
                bool shouldIncludeProductBySearchText = searchText == null || (searchText != null && product.Title.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);

                if (shouldIncludeProductBySearchText) // if searching by text corresponds, then we can check the time period
                {
                    bool shouldIncludeByTimePeriod = false;

                    switch (timePeriod)
                    {
                        case null:
                        case "all":
                            shouldIncludeByTimePeriod = true;
                            break;
                        case "Last 3 Months":
                            shouldIncludeByTimePeriod = order.OrderDate >= DateTime.Now.AddMonths(-3);
                            break;
                        case "Last 6 Months":
                            shouldIncludeByTimePeriod = order.OrderDate >= DateTime.Now.AddMonths(-6);
                            break;
                        case "This Year":
                            shouldIncludeByTimePeriod = order.OrderDate.Year == DateTime.Now.Year;
                            break;
                        default:
                            throw new ArgumentException($"GetOrdersCountAsync: Invalid time period: {timePeriod}");
                    }

                    if (shouldIncludeByTimePeriod)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Retrieves the product category types for a user (buyer or seller).
        /// </summary>
        /// <param name="userId">The ID of the user (buyer or seller).</param>
        /// <returns>A dictionary of order summary IDs and product category types.</returns>
        public async Task<Dictionary<int, string>> GetProductCategoryTypesAsync(int userId)
        {
            Dictionary<int, string> productCategoryTypes = new Dictionary<int, string>();

            // Check if the user is a seller by looking in the Sellers table
            bool isUserSeller = await this.dbContext.Sellers.AnyAsync(s => s.Id == userId);

            List<Order> ordersDb;
            if (isUserSeller)
            {
                // For sellers, get orders where they are the seller
                ordersDb = await this.dbContext.Orders
                    .Where(order => order.SellerId == userId)
                    .ToListAsync();
            }
            else
            {
                // For buyers, get orders where they are the buyer
                ordersDb = await this.dbContext.Orders
                    .Where(order => order.BuyerId == userId)
                    .ToListAsync();
            }

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

        /// <summary>
        /// Retrieves an order by its ID.
        /// </summary>
        /// <param name="orderId">The ID of the order to retrieve.</param>
        /// <returns>The order with the specified ID, or null if not found.</returns>
        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            Order? order = await this.dbContext.Orders.FindAsync(orderId);
            return order;
        }

        private static OrderDisplayInfo CreateOrderDisplayInfoFromOrderAndProduct(Order order, Product product, Condition condition, Category category)
        {
            return new OrderDisplayInfo
            {
                OrderID = order.Id,
                ProductName = product.Title,
                ProductTypeName = condition.Name,
                OrderDate = order.OrderDate.ToString("yyyy-MM-dd"),
                PaymentMethod = order.PaymentMethod,
                OrderSummaryID = order.OrderSummaryID,
                ProductCategory = category.Name,
            };
        }
    }
}