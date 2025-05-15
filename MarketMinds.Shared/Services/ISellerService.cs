// -----------------------------------------------------------------------
// <copyright file="ISellerService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SharedClassLibrary.Service
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;

    /// <summary>
    /// Defines the interface for seller-related operations.
    /// </summary>
    public interface ISellerService
    {
        /// <summary>
        /// Retrieves seller information based on the provided user.
        /// </summary>
        /// <param name="user">The user associated with the seller.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the seller information.</returns>
        Task<Seller> GetSellerByUser(User user);

        /// <summary>
        /// Retrieves a list of all products associated with the specified seller ID.
        /// </summary>
        /// <param name="sellerID">The ID of the seller.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of products.</returns>
        Task<List<Product>> GetAllProducts(int sellerID);

        /// <summary>
        /// Updates the seller information with the provided seller details.
        /// </summary>
        /// <param name="seller">The seller object containing the updated information.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task UpdateSeller(Seller seller);

        /// <summary>
        /// Creates a new seller with the provided seller details.
        /// </summary>
        /// <param name="seller">The seller object containing the information for the new seller.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task CreateSeller(Seller seller);

        /// <summary>
        /// Calculates the average review score for the specified seller and updates the seller's trust score.
        /// </summary>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the average review score.</returns>
        Task<double> CalculateAverageReviewScore(int sellerId);

        /// <summary>
        /// Retrieves the latest notifications for the specified seller.
        /// </summary>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <param name="maxNotifications">The maximum number of notifications to return.</param
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of notification messages.</returns>
        Task<List<string>> GetNotifications(int sellerId, int maxNotifications);

        /// <summary>
        /// Generate a new follower notification if the follower count changed.
        /// </summary>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <param name="currentFollowerCount">The current follower count of the seller.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task GenerateFollowersChangedNotification(int sellerId, int currentFollowerCount);

    }
}