namespace MarketMinds.Shared.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MarketMinds.Shared.Models;
    using MarketMinds.Shared.IRepository;

    /// <summary>
    /// Service for managing product waitlists
    /// </summary>
    public class WaitlistService : IWaitlistService
    {
        private readonly IWaitListRepository _waitListRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitlistService"/> class.
        /// </summary>
        /// <param name="waitListRepository">The waitlist repository.</param>
        public WaitlistService(IWaitListRepository waitListRepository)
        {
            _waitListRepository = waitListRepository ?? throw new ArgumentNullException(nameof(waitListRepository));
        }

        /// <summary>
        /// Adds a user to the waitlist for a product
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="productId">The product ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task AddUserToWaitlist(int userId, int productId)
        {
            await _waitListRepository.AddUserToWaitlist(userId, productId);
        }

        /// <summary>
        /// Gets a user's position in the waitlist for a product
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="productId">The product ID</param>
        /// <returns>The user's position in the waitlist (1-based)</returns>
        public async Task<int> GetUserWaitlistPosition(int userId, int productId)
        {
            return await _waitListRepository.GetUserWaitlistPosition(userId, productId);
        }

        /// <summary>
        /// Checks if a user is on the waitlist for a product
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="productId">The product ID</param>
        /// <returns>True if the user is on the waitlist, false otherwise</returns>
        public async Task<bool> IsUserInWaitlist(int userId, int productId)
        {
            return await _waitListRepository.IsUserInWaitlist(userId, productId);
        }

        /// <summary>
        /// Removes a user from the waitlist for a product
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="productId">The product ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task RemoveUserFromWaitlist(int userId, int productId)
        {
            await _waitListRepository.RemoveUserFromWaitlist(userId, productId);
        }

        /// <summary>
        /// Gets all users in the waitlist for a product
        /// </summary>
        /// <param name="productId">The ID of the product</param>
        /// <returns>A list of users in the waitlist</returns>
        public async Task<List<UserWaitList>> GetUsersInWaitlist(int productId)
        {
            return await _waitListRepository.GetUsersInWaitlist(productId);
        }

        /// <summary>
        /// Gets all waitlists a user is part of
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>A list of waitlists the user is in</returns>
        public async Task<List<UserWaitList>> GetUserWaitlists(int userId)
        {
            return await _waitListRepository.GetUserWaitlists(userId);
        }

        /// <summary>
        /// Gets the size of a waitlist for a product
        /// </summary>
        /// <param name="productId">The ID of the product</param>
        /// <returns>The number of users in the waitlist</returns>
        public async Task<int> GetWaitlistSize(int productId)
        {
            return await _waitListRepository.GetWaitlistSize(productId);
        }
    }
} 