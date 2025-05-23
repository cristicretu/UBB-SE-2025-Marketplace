// -----------------------------------------------------------------------
// <copyright file="ISellerService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace MarketMinds.Shared.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MarketMinds.Shared.Models;

    /// <summary>
    /// Interface for seller-related operations.
    /// </summary>
    public interface ISellerService
    {
        /// <summary>
        /// Gets seller information for a specific user.
        /// </summary>
        /// <param name="user">The user to get seller info for.</param>
        /// <returns>The seller information.</returns>
        Task<Seller> GetSellerByUser(User user);

        /// <summary>
        /// Updates seller information.
        /// </summary>
        /// <param name="seller">The seller information to update.</param>
        Task UpdateSeller(Seller seller);

        /// <summary>
        /// Gets all products for a seller.
        /// </summary>
        /// <param name="sellerId">The seller ID.</param>
        /// <returns>List of products for the seller.</returns>
        Task<List<BuyProduct>> GetAllProducts(int sellerId);

        /// <summary>
        /// Gets notifications for a seller.
        /// </summary>
        /// <param name="sellerId">The seller ID.</param>
        /// <param name="maxNotifications">Maximum number of notifications to retrieve.</param>
        /// <returns>List of notification messages.</returns>
        Task<List<string>> GetNotifications(int sellerId, int maxNotifications);

        /// <summary>
        /// Calculates the average review score for a seller.
        /// </summary>
        /// <param name="sellerId">The seller ID.</param>
        /// <returns>The average review score.</returns>
        Task<double> CalculateAverageReviewScore(int sellerId);

        /// <summary>
        /// Generates notification for followers count change.
        /// </summary>
        /// <param name="sellerId">The seller ID.</param>
        /// <param name="followerCount">Current follower count.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task GenerateFollowersChangedNotification(int sellerId, int followerCount);
    }
}