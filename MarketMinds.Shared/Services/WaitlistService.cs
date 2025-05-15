using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharedClassLibrary.Service
{
    /// <summary>
    /// Service for managing product waitlists
    /// </summary>
    public class WaitlistService : IWaitlistService
    {
        // This is a placeholder implementation. In a real app, this would use a database
        private static readonly Dictionary<int, List<int>> ProductWaitlists = new();

        /// <summary>
        /// Adds a user to the waitlist for a product
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="productId">The product ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public Task AddUserToWaitlist(int userId, int productId)
        {
            if (!ProductWaitlists.ContainsKey(productId))
            {
                ProductWaitlists[productId] = new List<int>();
            }

            if (!ProductWaitlists[productId].Contains(userId))
            {
                ProductWaitlists[productId].Add(userId);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets a user's position in the waitlist for a product
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="productId">The product ID</param>
        /// <returns>The user's position in the waitlist (1-based)</returns>
        public Task<int> GetUserWaitlistPosition(int userId, int productId)
        {
            if (!ProductWaitlists.TryGetValue(productId, out var waitlist) || !waitlist.Contains(userId))
            {
                return Task.FromResult(0);
            }

            return Task.FromResult(waitlist.IndexOf(userId) + 1);
        }

        /// <summary>
        /// Checks if a user is on the waitlist for a product
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="productId">The product ID</param>
        /// <returns>True if the user is on the waitlist, false otherwise</returns>
        public Task<bool> IsUserInWaitlist(int userId, int productId)
        {
            if (!ProductWaitlists.TryGetValue(productId, out var waitlist))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(waitlist.Contains(userId));
        }

        /// <summary>
        /// Removes a user from the waitlist for a product
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="productId">The product ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public Task RemoveUserFromWaitlist(int userId, int productId)
        {
            if (ProductWaitlists.TryGetValue(productId, out var waitlist))
            {
                waitlist.Remove(userId);
            }

            return Task.CompletedTask;
        }
    }
} 