// -----------------------------------------------------------------------
// <copyright file="ISellerRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SharedClassLibrary.IRepository
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;

    /// <summary>
    /// Defines the interface for managing seller-related data operations.
    /// </summary>
    public interface ISellerRepository
    {
        /// <summary>
        /// Adds a new follower notification for a given seller.
        /// </summary>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <param name="currentFollowerCount">The current follower count.</param>
        /// <param name="message">The notification message.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task AddNewFollowerNotification(int sellerId, int currentFollowerCount, string message);

        /// <summary>
        /// Adds a new seller.
        /// </summary>
        /// <param name="seller">The seller to be added.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task AddSeller(Seller seller);

        /// <summary>
        /// Gets the last follower count for a given seller.
        /// </summary>
        /// <param name="sellerId">The ID of the seller whose last follower count is to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the last follower count.</returns>
        Task<int> GetLastFollowerCount(int sellerId);

        /// <summary>
        /// Gets the notifications for a given seller.
        /// </summary>
        /// <param name="sellerID">The ID of the seller whose notifications are to be retrieved.</param>
        /// <param name="maxNotifications">The maximum number of notifications to return.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of notifications.</returns>
        Task<List<string>> GetNotifications(int sellerID, int maxNotifications);

        /// <summary>
        /// Gets the products for a given seller.
        /// </summary>
        /// <param name="sellerID">The ID of the seller whose products are to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of products.</returns>
        Task<List<Product>> GetProducts(int sellerID);

        /// <summary>
        /// Gets the reviews for a given seller.
        /// </summary>
        /// <param name="sellerId">The ID of the seller whose reviews are to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of reviews.</returns>
        Task<List<Review>> GetReviews(int sellerId);

        /// <summary>
        /// Gets the seller information for a given user.
        /// </summary>
        /// <param name="user">The user whose seller information is to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the seller information.</returns>
        Task<Seller> GetSellerInfo(User user);

        /// <summary>
        /// Updates the seller information.
        /// </summary>
        /// <param name="seller">The seller whose information is to be updated.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task UpdateSeller(Seller seller);

        /// <summary>
        /// Updates the trust score for a given seller.
        /// </summary>
        /// <param name="sellerId">The ID of the seller whose trust score is to be updated.</param>
        /// <param name="trustScore">The new trust score.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task UpdateTrustScore(int sellerId, double trustScore);
    }
}