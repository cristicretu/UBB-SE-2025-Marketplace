namespace MarketMinds.Shared.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MarketMinds.Shared.Models;

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
        /// Adds a user to the waitlist for a product with a preferred end date
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="productId">The product ID</param>
        /// <param name="preferredEndDate">The user's preferred end date for borrowing</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task AddUserToWaitlist(int userId, int productId, DateTime? preferredEndDate);

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

        /// <summary>
        /// Gets all users in the waitlist for a product
        /// </summary>
        /// <param name="productId">The ID of the product</param>
        /// <returns>A list of users in the waitlist</returns>
        Task<List<UserWaitList>> GetUsersInWaitlist(int productId);

        /// <summary>
        /// Gets all waitlists a user is part of
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>A list of waitlists the user is in</returns>
        Task<List<UserWaitList>> GetUserWaitlists(int userId);

        /// <summary>
        /// Gets the size of a waitlist for a product
        /// </summary>
        /// <param name="productId">The ID of the product</param>
        /// <returns>The number of users in the waitlist</returns>
        Task<int> GetWaitlistSize(int productId);

        /// <summary>
        /// Gets a specific user's waitlist entry for a product
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="productId">The product ID</param>
        /// <returns>The waitlist entry or null if not found</returns>
        Task<UserWaitList?> GetUserWaitlistEntry(int userId, int productId);
    }
}