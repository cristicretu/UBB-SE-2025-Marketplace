using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;

namespace MarketPlace924.Services
{
    /// <summary>
    /// Defines operations for managing waitlist functionality for users and products.
    /// </summary>
    public interface IWaitListViewModel
    {
        /// <summary>
        /// Adds a user to the waitlist for a specified product.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to add.</param>
        /// <param name="productId">The unique identifier of the product.</param>
        Task AddUserToWaitlist(int userId, int productId);

        /// <summary>
        /// Removes a user from the waitlist for a specified product.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to remove.</param>
        /// <param name="productId">The unique identifier of the product.</param>
        Task RemoveUserFromWaitlist(int userId, int productId);

        /// <summary>
        /// Retrieves all users in the waitlist for the specified product.
        /// </summary>
        /// <param name="waitListProductId">The unique identifier of the product waitlist.</param>
        /// <returns>
        /// A list of <see cref="UserWaitList"/> objects representing the users in the waitlist.
        /// </returns>
        Task<List<UserWaitList>> GetUsersInWaitlist(int productId);

        /// <summary>
        /// Retrieves all waitlists that a specific user is part of.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>
        /// A list of <see cref="UserWaitList"/> objects representing the waitlists the user is participating in.
        /// </returns>
        Task<List<UserWaitList>> GetUserWaitlists(int userId);

        /// <summary>
        /// Retrieves the size of the waitlist for the specified product.
        /// </summary>
        /// <param name="productId">The unique identifier of the product.</param>
        /// <returns>The size of the waitlist.</returns>
        Task<int> GetWaitlistSize(int productId);

        /// <summary>
        /// Determines whether a user is already present in the waitlist for a specified product.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="productId">The unique identifier of the product.</param>
        /// <returns><c>true</c> if the user is in the waitlist; otherwise, <c>false</c>.</returns>
        Task<bool> IsUserInWaitlist(int userId, int productId);

        /// <summary>
        /// Asynchronously retrieves the seller's name based on an optional seller identifier.
        /// </summary>
        /// <param name="sellerId">The unique identifier of the seller (optional).</param>
        /// <returns>A task that returns the seller's name as a string.</returns>
        Task<string> GetSellerNameAsync(int? sellerId);

        /// <summary>
        /// Asynchronously retrieves the dummy product corresponding to the specified product ID.
        /// </summary>
        /// <param name="productId">The unique identifier of the product.</param>
        /// <returns>A task that returns the <see cref="Product"/> object.</returns>
        Task<Product> GetProductByIdAsync(int productId);

        /// <summary>
        /// Retrieves the waitlist position of a user for a specific product.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="productId">The unique identifier of the product.</param>
        /// <returns>The user's position in the waitlist.</returns>
        Task<int> GetUserWaitlistPosition(int userId, int productId);
    }
}
