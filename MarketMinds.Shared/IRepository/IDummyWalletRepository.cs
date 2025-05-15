using System.Threading.Tasks;

namespace SharedClassLibrary.IRepository
{
    /// <summary>
    /// Defines database operations for wallet management.
    /// </summary>
    public interface IDummyWalletRepository
    {
        /// <summary>
        /// Retrieves the wallet balance for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user whose wallet balance to retrieve.</param>
        /// <returns>A task representing the asynchronous operation that returns the wallet balance.</returns>
        /// <exception cref="Exception">Thrown when the wallet is not found for the specified user.</exception>
        Task<double> GetWalletBalanceAsync(int userId);

        /// <summary>
        /// Updates the wallet balance for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user whose wallet balance to update.</param>
        /// <param name="newBalance">The new wallet balance.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when the wallet is not found for the specified user.</exception>
        Task UpdateWalletBalance(int userId, double newBalance);
    }
} 