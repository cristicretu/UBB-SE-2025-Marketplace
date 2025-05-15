using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;

namespace MarketPlace924.ViewModel
{
    /// <summary>
    /// Defines operations for managing orders including adding, deleting, and retrieval of orders and their summaries.
    /// </summary>
    public interface IOrderViewModel
    {
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
        Task AddOrderAsync(int productId, int buyerId, string productType, string paymentMethod, int orderSummaryId, DateTime orderDate);

        /// <summary>
        /// Asynchronously deletes the order specified by the order ID.
        /// </summary>
        /// <param name="orderId">The unique identifier of the order to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteOrderAsync(int orderId);

        /// <summary>
        /// Asynchronously retrieves the borrowed order history for the specified buyer.
        /// </summary>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <returns>
        /// A task that represents the asynchronous operation and returns a list of borrowed orders.
        /// </returns>
        Task<List<Order>> GetBorrowedOrderHistoryAsync(int buyerId);

        /// <summary>
        /// Asynchronously retrieves a combined order history for the specified buyer with an optional time period filter.
        /// </summary>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <param name="timePeriodFilter">The time period filter (default is "all").</param>
        /// <returns>
        /// A task that represents the asynchronous operation and returns a list of orders.
        /// </returns>
        Task<List<Order>> GetCombinedOrderHistoryAsync(int buyerId, string timePeriodFilter = "all");

        /// <summary>
        /// Asynchronously retrieves the order history for new or used products for the specified buyer.
        /// </summary>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <returns>
        /// A task that represents the asynchronous operation and returns a list of orders.
        /// </returns>
        Task<List<Order>> GetNewOrUsedOrderHistoryAsync(int buyerId);

        /// <summary>
        /// Asynchronously retrieves the order details by its unique identifier.
        /// </summary>
        /// <param name="orderId">The unique identifier of the order.</param>
        /// <returns>
        /// A task that represents the asynchronous operation and returns the <see cref="Order"/> object.
        /// </returns>
        Task<Order> GetOrderByIdAsync(int orderId);

        /// <summary>
        /// Asynchronously retrieves orders for the specified buyer based on a text search.
        /// </summary>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <param name="text">The text to search for within orders.</param>
        /// <returns>
        /// A task that represents the asynchronous operation and returns a list of orders.
        /// </returns>
        Task<List<Order>> GetOrdersByNameAsync(int buyerId, string text);

        /// <summary>
        /// Asynchronously retrieves orders from the year 2024 for the specified buyer.
        /// </summary>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <returns>
        /// A task that represents the asynchronous operation and returns a list of orders.
        /// </returns>
        Task<List<Order>> GetOrdersFrom2024Async(int buyerId);

        /// <summary>
        /// Asynchronously retrieves orders from the year 2025 for the specified buyer.
        /// </summary>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <returns>
        /// A task that represents the asynchronous operation and returns a list of orders.
        /// </returns>
        Task<List<Order>> GetOrdersFrom2025Async(int buyerId);

        /// <summary>
        /// Asynchronously retrieves orders from the last six months for the specified buyer.
        /// </summary>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <returns>
        /// A task that represents the asynchronous operation and returns a list of orders.
        /// </returns>
        Task<List<Order>> GetOrdersFromLastSixMonthsAsync(int buyerId);

        /// <summary>
        /// Asynchronously retrieves orders from the last three months for the specified buyer.
        /// </summary>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <returns>
        /// A task that represents the asynchronous operation and returns a list of orders.
        /// </returns>
        Task<List<Order>> GetOrdersFromLastThreeMonthsAsync(int buyerId);

        /// <summary>
        /// Asynchronously retrieves the orders associated with the specified order history ID.
        /// </summary>
        /// <param name="orderHistoryId">The unique identifier of the order history.</param>
        /// <returns>
        /// A task that represents the asynchronous operation and returns a list of orders.
        /// </returns>
        Task<List<Order>> GetOrdersFromOrderHistoryAsync(int orderHistoryId);

        /// <summary>
        /// Asynchronously retrieves the order summary for the specified order summary ID.
        /// </summary>
        /// <param name="orderSummaryId">The unique identifier of the order summary.</param>
        /// <returns>
        /// A task that represents the asynchronous operation and returns the <see cref="OrderSummary"/> object.
        /// </returns>
        Task<OrderSummary> GetOrderSummaryAsync(int orderSummaryId);

        /// <summary>
        /// Asynchronously retrieves orders along with product information for the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="searchText">Optional text to search within the orders.</param>
        /// <param name="timePeriod">Optional filter for the time period.</param>
        /// <returns>
        /// A task that represents the asynchronous operation and returns a list of <see cref="OrderDisplayInfo"/> objects.
        /// </returns>
        Task<List<OrderDisplayInfo>> GetOrdersWithProductInfoAsync(int userId, string searchText = null, string timePeriod = null);

        /// <summary>
        /// Asynchronously retrieves the product category types for the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>
        /// A task that represents the asynchronous operation and returns a dictionary mapping product category type IDs to their names.
        /// </returns>
        Task<Dictionary<int, string>> GetProductCategoryTypesAsync(int userId);

        /// <summary>
        /// Asynchronously updates the specified order with new values.
        /// </summary>
        /// <param name="orderId">The unique identifier of the order to update.</param>
        /// <param name="productType">The new product type.</param>
        /// <param name="paymentMethod">The new payment method.</param>
        /// <param name="orderDate">The new order date.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateOrderAsync(int orderId, string productType, string paymentMethod, DateTime orderDate);
    }
}
