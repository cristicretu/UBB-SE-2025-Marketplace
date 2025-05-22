// -----------------------------------------------------------------------
// <copyright file="SellerService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace MarketMinds.Shared.Services
{
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using MarketMinds.Shared.Models;
    using MarketMinds.Shared.IRepository;
    using System;

    /// <summary>
    /// Service for managing seller-related operations.
    /// </summary>
    public class SellerService : ISellerService
    {
        private readonly ISellerRepository sellerRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SellerService"/> class.
        /// </summary>
        /// <param name="sellerRepository">The seller repository.</param>
        public SellerService(ISellerRepository sellerRepository)
        {
            Debug.WriteLine("SellerService constructor called");
            this.sellerRepository = sellerRepository ?? throw new ArgumentNullException(nameof(sellerRepository));
        }

        /// <inheritdoc/>
        public async Task<double> CalculateAverageReviewScore(int sellerId)
        {
            Debug.WriteLine($"CalculateAverageReviewScore called for seller ID: {sellerId}");
            try
            {
                var reviews = await this.sellerRepository.GetReviews(sellerId);

                if (reviews == null || reviews.Count == 0)
                {
                    Debug.WriteLine("No reviews found, returning 0");
                    return 0;
                }

                double totalRating = reviews.Sum(r => r.Rating);
                double averageScore = totalRating / reviews.Count;

                Debug.WriteLine($"Calculated average score: {averageScore} from {reviews.Count} reviews");
                return averageScore;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error calculating average review score: {ex.Message}");
                return 0;
            }
        }

        /// <inheritdoc/>
        public async Task<List<BuyProduct>> GetAllProducts(int sellerId)
        {
            Debug.WriteLine($"GetAllProducts called for seller ID: {sellerId}");
            try
            {
                var products = await this.sellerRepository.GetProducts(sellerId);
                Debug.WriteLine($"Retrieved {products.Count} products");
                return products;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting products: {ex.Message}");
                return new List<BuyProduct>();
            }
        }

        /// <inheritdoc/>
        public async Task GenerateFollowersChangedNotification(int sellerId, int followerCount)
        {
            Debug.WriteLine($"GenerateFollowersChangedNotification called for seller ID: {sellerId}, follower count: {followerCount}");
            try
            {
                int lastFollowerCount = await this.sellerRepository.GetLastFollowerCount(sellerId);

                if (lastFollowerCount != followerCount)
                {
                    string message = lastFollowerCount < followerCount
                        ? $"You gained a new follower! You now have {followerCount} followers."
                        : $"You lost a follower. You now have {followerCount} followers.";

                    await this.sellerRepository.AddNewFollowerNotification(sellerId, followerCount, message);
                    Debug.WriteLine("Notification generated");
                }
                else
                {
                    Debug.WriteLine("Follower count unchanged, no notification generated");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error generating follower notification: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public async Task<List<string>> GetNotifications(int sellerId, int maxNotifications)
        {
            Debug.WriteLine($"GetNotifications called for seller ID: {sellerId}, max: {maxNotifications}");
            try
            {
                var notifications = await this.sellerRepository.GetNotifications(sellerId, maxNotifications);
                Debug.WriteLine($"Retrieved {notifications.Count} notifications");
                return notifications;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting notifications: {ex.Message}");
                return new List<string>();
            }
        }

        /// <inheritdoc/>
        public async Task<Seller> GetSellerByUser(User user)
        {
            Debug.WriteLine($"GetSellerByUser called for User ID: {user?.Id ?? -1}");

            if (user == null)
            {
                Debug.WriteLine("User is null, returning null");
                return null;
            }

            try
            {
                var seller = await this.sellerRepository.GetSellerInfo(user);

                if (seller == null)
                {
                    Debug.WriteLine("Repository returned null Seller, creating new one with User");
                    return new Seller(user);
                }

                if (seller.User == null)
                {
                    seller.User = user;
                }

                Debug.WriteLine($"Retrieved seller: ID={seller.Id}, StoreName='{seller.StoreName}', Username='{seller.User?.Username}'");
                return seller;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting seller: {ex.Message}");
                return new Seller(user);
            }
        }

        /// <inheritdoc/>
        public async Task UpdateSeller(Seller seller)
        {
            Debug.WriteLine($"UpdateSeller called for seller ID: {seller?.Id ?? -1}");

            if (seller == null)
            {
                throw new ArgumentNullException(nameof(seller), "Seller is null, cannot update");
            }

            if (seller.Id <= 0)
            {
                throw new ArgumentException("Invalid seller ID", nameof(seller));
            }

            try
            {
                await this.sellerRepository.UpdateSeller(seller);
                Debug.WriteLine("Seller updated successfully");

                var reviewScore = await CalculateAverageReviewScore(seller.Id);
                await this.sellerRepository.UpdateTrustScore(seller.Id, reviewScore);
                Debug.WriteLine("Trust score updated");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating seller: {ex.Message}");
                throw;
            }
        }
    }
}
