using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Service;
using SharedClassLibrary.Shared;

namespace MarketPlace924.ViewModel
{
    /// <summary>
    /// Represents the view model for orders and facilitates order management operations.
    /// </summary>
    public class OrderViewModel : IOrderViewModel
    {
        private readonly IOrderService orderService;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderViewModel"/> class using the specified connection string and default SQL database provider.
        /// </summary>
        /// <param name="connectionString">The connection string used for database operations.</param>
        public OrderViewModel(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderViewModel"/> class using the specified connection string and database provider.
        /// </summary>
        /// <param name="connectionString">The connection string used for database operations.</param>
        /// <param name="databaseProvider">The database provider used for creating database connections.</param>
        public OrderViewModel(string connectionString, IDatabaseProvider databaseProvider)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
            if (databaseProvider == null)
            {
                throw new ArgumentNullException(nameof(databaseProvider));
            }

            orderService = new OrderService();
        }

        /// <summary>
        /// Asynchronously adds a new order with the specified parameters.
        /// </summary>
        /// <param name="productId">The unique identifier of the product.</param>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <param name="productType">The type of the product.</param>
        /// <param name="paymentMethod">The payment method used for the order.</param>
        /// <param name="orderSummaryId">The unique identifier of the order summary.</param>
        /// <param name="orderDate">The date when the order was placed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddOrderAsync(int productId, int buyerId, string productType, string paymentMethod, int orderSummaryId, DateTime orderDate)
        {
            await orderService.AddOrderAsync(productId, buyerId, productType, paymentMethod, orderSummaryId, orderDate);
        }

        /// <summary>
        /// Asynchronously updates the specified order with new values.
        /// </summary>
        /// <param name="orderId">The unique identifier of the order to update.</param>
        /// <param name="productType">The new product type.</param>
        /// <param name="paymentMethod">The new payment method.</param>
        /// <param name="orderDate">The new order date.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateOrderAsync(int orderId, string productType, string paymentMethod, DateTime orderDate)
        {
            await orderService.UpdateOrderAsync(orderId, productType, paymentMethod, orderDate);
        }

        /// <summary>
        /// Asynchronously deletes the order specified by the order ID.
        /// </summary>
        /// <param name="orderId">The unique identifier of the order to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteOrderAsync(int orderId)
        {
            await orderService.DeleteOrderAsync(orderId);
        }

        /// <summary>
        /// Asynchronously retrieves the borrowed order history for the specified buyer.
        /// </summary>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <returns>A task that returns a list of borrowed orders.</returns>
        public async Task<List<Order>> GetBorrowedOrderHistoryAsync(int buyerId)
        {
            return await orderService.GetBorrowedOrderHistoryAsync(buyerId);
        }

        /// <summary>
        /// Asynchronously retrieves the order history for new or used products for the specified buyer.
        /// </summary>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <returns>A task that returns a list of orders.</returns>
        public async Task<List<Order>> GetNewOrUsedOrderHistoryAsync(int buyerId)
        {
            return await orderService.GetNewOrUsedOrderHistoryAsync(buyerId);
        }

        /// <summary>
        /// Asynchronously retrieves orders from the last three months for the specified buyer.
        /// </summary>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <returns>A task that returns a list of orders.</returns>
        public async Task<List<Order>> GetOrdersFromLastThreeMonthsAsync(int buyerId)
        {
            return await orderService.GetOrdersFromLastThreeMonthsAsync(buyerId);
        }

        /// <summary>
        /// Asynchronously retrieves orders from the last six months for the specified buyer.
        /// </summary>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <returns>A task that returns a list of orders.</returns>
        public async Task<List<Order>> GetOrdersFromLastSixMonthsAsync(int buyerId)
        {
            return await orderService.GetOrdersFromLastSixMonthsAsync(buyerId);
        }

        /// <summary>
        /// Asynchronously retrieves orders from the year 2024 for the specified buyer.
        /// </summary>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <returns>A task that returns a list of orders.</returns>
        public async Task<List<Order>> GetOrdersFrom2024Async(int buyerId)
        {
            return await orderService.GetOrdersFrom2024Async(buyerId);
        }

        /// <summary>
        /// Asynchronously retrieves orders from the year 2025 for the specified buyer.
        /// </summary>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <returns>A task that returns a list of orders.</returns>
        public async Task<List<Order>> GetOrdersFrom2025Async(int buyerId)
        {
            return await orderService.GetOrdersFrom2025Async(buyerId);
        }

        /// <summary>
        /// Asynchronously retrieves orders for the specified buyer based on a text search.
        /// </summary>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <param name="text">The text to search for within orders.</param>
        /// <returns>A task that returns a list of orders.</returns>
        public async Task<List<Order>> GetOrdersByNameAsync(int buyerId, string text)
        {
            return await orderService.GetOrdersByNameAsync(buyerId, text);
        }

        /// <summary>
        /// Asynchronously retrieves the orders associated with the specified order history ID.
        /// </summary>
        /// <param name="orderHistoryId">The unique identifier of the order history.</param>
        /// <returns>A task that returns a list of orders.</returns>
        public async Task<List<Order>> GetOrdersFromOrderHistoryAsync(int orderHistoryId)
        {
            return await orderService.GetOrdersFromOrderHistoryAsync(orderHistoryId);
        }

        /// <summary>
        /// Asynchronously retrieves the order summary for the specified order summary ID.
        /// </summary>
        /// <param name="orderSummaryId">The unique identifier of the order summary.</param>
        /// <returns>A task that returns the <see cref="OrderSummary"/> object.</returns>
        public async Task<OrderSummary> GetOrderSummaryAsync(int orderSummaryId)
        {
            return await orderService.GetOrderSummaryAsync(orderSummaryId);
        }

        /// <summary>
        /// Asynchronously retrieves an order by its unique identifier.
        /// </summary>
        /// <param name="orderId">The unique identifier of the order.</param>
        /// <returns>
        /// A task that returns the <see cref="Order"/> object if found; otherwise, <c>null</c>.
        /// </returns>
        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            return await orderService.GetOrderByIdAsync(orderId);
        }

        /// <summary>
        /// Asynchronously retrieves a combined order history for the specified buyer using an optional time period filter.
        /// </summary>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <param name="timePeriodFilter">Optional filter specifying the time period ("3months", "6months", "2024", "2025", or "all").</param>
        /// <returns>A task that returns a list of orders.</returns>
        public async Task<List<Order>> GetCombinedOrderHistoryAsync(int buyerId, string timePeriodFilter = "all")
        {
            return await orderService.GetCombinedOrderHistoryAsync(buyerId, timePeriodFilter);
        }

        /// <summary>
        /// Asynchronously retrieves order details along with product information for a specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose orders to retrieve. Must be a positive integer.</param>
        /// <param name="searchText">Optional. Text to filter orders by product name. Can be null or empty.</param>
        /// <param name="timePeriod">Optional. Time period filter ("Last 3 Months", "Last 6 Months", "This Year", etc.). Can be null or empty.</param>
        /// <returns>
        /// A task that returns a list of <see cref="OrderDisplayInfo"/> objects containing order details and product information.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when userId is less than or equal to zero.</exception>
        /// <remarks>
        /// The method returns an empty list if no orders match the specified criteria.
        /// Orders are categorized as either "new" or "borrowed" based on the product type.
        /// </remarks>
        public async Task<List<OrderDisplayInfo>> GetOrdersWithProductInfoAsync(int userId, string searchText = null, string timePeriod = null)
        {
            return await orderService.GetOrdersWithProductInfoAsync(userId, searchText, timePeriod);
        }


        /// <summary>
        /// Asynchronously retrieves product category types (new/borrowed) for each order summary ID for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user whose product categories to retrieve. Must be a positive integer.</param>
        /// <returns>
        /// A task that returns a dictionary mapping order summary IDs to product category types ("new" or "borrowed").
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when userId is less than or equal to zero.</exception>
        /// <remarks>
        /// Products are categorized as "new" if their type is "new" or "used", otherwise as "borrowed".
        /// This information is used to determine whether to show contract details for borrowed products.
        /// </remarks>
        public async Task<Dictionary<int, string>> GetProductCategoryTypesAsync(int userId)
        {
            return await orderService.GetProductCategoryTypesAsync(userId);
        }
    }
}
