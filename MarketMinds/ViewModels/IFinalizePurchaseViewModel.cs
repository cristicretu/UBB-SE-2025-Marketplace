using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;

namespace MarketPlace924.ViewModel
{
    /// <summary>
    /// Defines operations for finalizing the purchase process.
    /// </summary>
    internal interface IFinalizePurchaseViewModel
    {
        event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Asynchronously retrieves the dummy products associated with the specified order history.
        /// </summary>
        /// <param name="orderHistoryID">The unique identifier for the order history.</param>
        /// <returns>A task that represents the asynchronous operation and returns a list of <see cref="Product"/> objects.</returns>
        Task<List<Product>> GetProductsFromOrderHistoryAsync(int orderHistoryID);
    }
}
