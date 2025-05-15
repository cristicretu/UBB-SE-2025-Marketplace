namespace SharedClassLibrary.Service
{
    using SharedClassLibrary.Domain;
    using System.Threading.Tasks;

    /// <summary>
    /// Service interface for buyer UI-related operations
    /// </summary>
    public interface IBuyerUIService
    {
        /// <summary>
        /// Tracks an order by ID and returns the appropriate window
        /// </summary>
        /// <param name="trackedOrderId">The ID of the order to track</param>
        /// <param name="userId">Current user ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task TrackOrderAsync(int trackedOrderId, int userId);

        /// <summary>
        /// Opens the billing info window for the specified order history type
        /// </summary>
        /// <param name="orderHistoryType">Type of order history</param>
        void OpenBillingInfo(int orderHistoryType);

        /// <summary>
        /// Generates and saves a contract of the specified type
        /// </summary>
        /// <param name="contractType">Type of contract to generate</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task GenerateAndSaveContractAsync(PredefinedContractType contractType);

        /// <summary>
        /// Opens the borrow product window for the specified product
        /// </summary>
        /// <param name="productId">ID of the product to borrow</param>
        void OpenBorrowProduct(int productId);

        /// <summary>
        /// Opens the notifications window
        /// </summary>
        void OpenNotifications();

        /// <summary>
        /// Opens the order history window for the specified user
        /// </summary>
        /// <param name="userId">ID of the user whose order history to display</param>
        void OpenOrderHistory(int userId);

        /// <summary>
        /// Opens the renew contract window
        /// </summary>
        void OpenRenewContract();
    }
}