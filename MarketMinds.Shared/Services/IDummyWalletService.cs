using System.Threading.Tasks;

namespace SharedClassLibrary.Service
{
    /// <summary>
    /// Defines operations for managing wallet operations.
    /// </summary>
    public interface IDummyWalletService
    {
        /// <summary>
        /// Retrieves the wallet balance for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user whose wallet balance to retrieve.</param>
        /// <returns>A task representing the asynchronous operation that returns the wallet balance.</returns>
        Task<double> GetWalletBalanceAsync(int userId);

        /// <summary>
        /// Updates the wallet balance for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user whose wallet balance to update.</param>
        /// <param name="newBalance">The new wallet balance.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateWalletBalance(int userId, double newBalance);
    }
}