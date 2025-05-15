// -----------------------------------------------------------------------
// <copyright file="SellerService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SharedClassLibrary.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// Provides services related to seller operations.
    /// </summary>
    public class SellerService : ISellerService
    {
        private ISellerRepository sellerRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SellerService"/> class with the specified seller repository.
        /// </summary>
        /// <param name="sellerRepository">The repository used to access seller data.</param>
        public SellerService(ISellerRepository sellerRepository)
        {
            this.sellerRepository = sellerRepository;
        }

        /// <summary>
        /// Retrieves seller information based on the provided user.
        /// </summary>
        /// <param name="user">The user associated with the seller.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the seller information.</returns>
        public async Task<Seller> GetSellerByUser(User user)
        {
            Seller seller = await this.sellerRepository.GetSellerInfo(user);
            return seller;
        }

        /// <summary>
        /// Retrieves a list of all products associated with the specified seller ID.
        /// </summary>
        /// <param name="sellerID">The ID of the seller.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of products.</returns>
        public async Task<List<Product>> GetAllProducts(int sellerID)
        {
            return await this.sellerRepository.GetProducts(sellerID);
        }

        /// <summary>
        /// Updates the seller information with the provided seller details.
        /// </summary>
        /// <param name="seller">The seller object containing the updated information.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task UpdateSeller(Seller seller)
        {
            await this.sellerRepository.UpdateSeller(seller);
        }

        /// <summary>
        /// Creates a new seller with the provided seller details.
        /// </summary>
        /// <param name="seller">The seller object containing the information for the new seller.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task CreateSeller(Seller seller)
        {
            await this.sellerRepository.AddSeller(seller);
        }

        /// <summary>
        /// Calculates the average review score for the specified seller and updates the seller's trust score.
        /// </summary>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the average review score.</returns>
        public async Task<double> CalculateAverageReviewScore(int sellerId)
        {
            var reviews = await this.sellerRepository.GetReviews(sellerId);
            double reviewScore = 0;
            if (reviews.Count != 0)
            {
                reviewScore = reviews.Average(r => r.Score);
            }

            await this.sellerRepository.UpdateTrustScore(sellerId, reviewScore);
            return reviewScore;
        }

        /// <summary>
        /// Retrieves the latest notifications for the specified seller.
        /// </summary>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <param name="maxNotifications">The maximum number of notifications to return.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of notification messages.</returns>
        public async Task<List<string>> GetNotifications(int sellerId, int maxNotifications)
        {
            return await this.sellerRepository.GetNotifications(sellerId, maxNotifications);
        }

        /// <summary>
        /// Generate a new follower notification if the follower count changed.
        /// </summary>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <param name="currentFollowerCount">The current follower count of the seller.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task GenerateFollowersChangedNotification(int sellerId, int currentFollowerCount)
        {
            int lastFollowerCount = await this.sellerRepository.GetLastFollowerCount(sellerId);
            if (currentFollowerCount != lastFollowerCount)
            {
                string notificationMessage = this.GenerateNotificationMessageFromFollowerCount(currentFollowerCount, lastFollowerCount);
                await this.sellerRepository.AddNewFollowerNotification(sellerId, currentFollowerCount, notificationMessage);
            }
        }

        private string GenerateNotificationMessageFromFollowerCount(int currentFollowerCount, int lastFollowerCount)
        {
            List<int> milestones = new List<int>() { 100, 50, 10 };
            Dictionary<int, string> milestoneMessages = new Dictionary<int, string>()
            {
                { 100, "Congratulations!" },
                { 50, "Amazing!" },
                { 10, "Incredible!" },
            };
            string message = string.Empty;
            int followersGained = currentFollowerCount - lastFollowerCount;
            int followersLost = -followersGained;
            if (followersGained > 0)
            {
                foreach (int milestone in milestones)
                {
                    if (currentFollowerCount >= milestone && currentFollowerCount % milestone == 0)
                    {
                        message = $"{milestoneMessages[milestone]} You've reached {currentFollowerCount} followers!";
                        return message;
                    }
                }
            }

            if (followersGained > 1)
            {
                message = $"You have gained {followersGained} new followers!";
            }
            else if (followersGained == 1)
            {
                message = $"You have gained {followersGained} new follower!";
            }
            else if (followersLost == 1)
            {
                message = $"You have lost {followersLost} follower!";
            }
            else if (followersLost > 1)
            {
                message = $"You have lost {followersLost} followers!";
            }

            return message;
        }
    }
}
