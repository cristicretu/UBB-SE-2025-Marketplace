using System.Collections.Generic;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;

namespace SharedClassLibrary.Service
{
    /// <summary>
    /// Defines operations for managing order history.
    /// </summary>
    public interface IOrderHistoryService
    {
        /// <summary>
        /// Retrieves dummy products associated with the specified order history.
        /// </summary>
        /// <param name="orderHistoryId">The unique identifier for the order history.</param>
        /// <returns>A task representing the asynchronous operation that returns a list of dummy products.</returns>
        Task<List<Product>> GetProductsFromOrderHistoryAsync(int orderHistoryId);

        /// <summary>
        /// Creates a new order history record in the database.
        /// </summary>
        /// <returns>A task representing the asynchronous operation that returns the ID of the newly created order history.</returns>
        //Task<int> CreateOrderHistoryAsync();
    }
}
