using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.IRepository
{
    /// <summary>
    /// Defines database operations for order history.
    /// </summary>
    public interface IOrderHistoryRepository
    {
        /// <summary>
        /// Retrieves products associated with the specified order history.
        /// </summary>
        /// <param name="orderHistoryId">The unique identifier for the order history.</param>
        /// <returns>A task representing the asynchronous operation that returns a list of products.</returns>
        Task<List<Product>> GetProductsFromOrderHistoryAsync(int orderHistoryId);

        /// <summary>
        /// Creates a new order history record in the database.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer for this order history.</param>
        /// <returns>A task representing the asynchronous operation that returns the ID of the newly created order history.</returns>
        Task<int> CreateOrderHistoryAsync(int buyerId);

        /// <summary>
        /// Retrieves all order histories for a specific buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A task representing the asynchronous operation that returns a list of order histories.</returns>
        Task<List<OrderHistory>> GetOrderHistoriesByBuyerAsync(int buyerId);

        /// <summary>
        /// Retrieves a specific order history by ID.
        /// </summary>
        /// <param name="orderHistoryId">The ID of the order history.</param>
        /// <returns>A task representing the asynchronous operation that returns the order history.</returns>
        Task<OrderHistory> GetOrderHistoryByIdAsync(int orderHistoryId);

        /// <summary>
        /// Updates an order history record in the database.
        /// </summary>
        /// <param name="orderHistoryId">The ID of the order history to update.</param>
        /// <param name="note">The note for the order history.</param>
        /// <param name="shippingAddress">The shipping address for the order history.</param>
        /// <param name="paymentMethod">The payment method for the order history.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateOrderHistoryAsync(int orderHistoryId, string note, string shippingAddress, string paymentMethod);
    }
}
