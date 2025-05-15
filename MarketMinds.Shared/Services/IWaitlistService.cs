using System.Threading.Tasks;

namespace SharedClassLibrary.Service
{
    /// <summary>
    /// Service for managing product waitlists
    /// </summary>
    public interface IWaitlistService
    {
        /// <summary>
        /// Adds a user to the waitlist for a product
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="productId">The product ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task AddUserToWaitlist(int userId, int productId);

        /// <summary>
        /// Removes a user from the waitlist for a product
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="productId">The product ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task RemoveUserFromWaitlist(int userId, int productId);

        /// <summary>
        /// Checks if a user is on the waitlist for a product
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="productId">The product ID</param>
        /// <returns>True if the user is on the waitlist, false otherwise</returns>
        Task<bool> IsUserInWaitlist(int userId, int productId);

        /// <summary>
        /// Gets a user's position in the waitlist for a product
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="productId">The product ID</param>
        /// <returns>The user's position in the waitlist (1-based)</returns>
        Task<int> GetUserWaitlistPosition(int userId, int productId);
    }
} 