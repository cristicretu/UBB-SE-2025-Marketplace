using System.Collections.Generic;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;

namespace SharedClassLibrary.IRepository
{
    /// <summary>
    /// Defines database operations for order history.
    /// </summary>
    public interface IOrderHistoryRepository
    {
        /// <summary>
        /// Retrieves dummy products associated with the specified order history.
        /// </summary>
        /// <param name="orderHistoryId">The unique identifier for the order history.</param>
        /// <returns>A task representing the asynchronous operation that returns a list of dummy products.</returns>
        Task<List<Product>> GetProductsFromOrderHistoryAsync(int orderHistoryId);
    }
}
