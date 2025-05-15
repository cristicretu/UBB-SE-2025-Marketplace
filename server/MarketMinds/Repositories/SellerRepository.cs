// -----------------------------------------------------------------------
// <copyright file="SellerRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Server.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Server.DataModels;
    using Server.DBConnection;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// Repository for managing seller-related data operations.
    /// </summary>
    public class SellerRepository : ISellerRepository
    {
        private readonly MarketPlaceDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SellerRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context to be used.</param>
        public SellerRepository(MarketPlaceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Gets the seller information for a given user.
        /// </summary>
        /// <param name="user">The user whose seller information is to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the seller information.</returns>
        /// <exception cref="Exception">Thrown when the seller is not found.</exception>
        public async Task<Seller> GetSellerInfo(User user)
        {
            User userDb = await this.dbContext.Users.FindAsync(user.UserId)
                                    ?? throw new Exception("GetSellerInfo: User not found");
            Seller sellerDb = await this.dbContext.Sellers.FindAsync(userDb.UserId)
                                    ?? throw new Exception("GetSellerInfo: Seller not found");

            sellerDb.User = userDb;
            return sellerDb;
        }

        /// <summary>
        /// Gets the notifications for a given seller.
        /// </summary>
        /// <param name="sellerID">The ID of the seller whose notifications are to be retrieved.</param>
        /// <param name="maxNotifications">The maximum number of notifications to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of notifications.</returns>
        public async Task<List<string>> GetNotifications(int sellerID, int maxNotifications)
        {
            List<SellerNotificationEntity> sellerNotificationsDb = await this.dbContext.SellerNotifications
                .Where(sellerNotification => sellerNotification.SellerID == sellerID)
                .OrderByDescending(sellerNotification => sellerNotification.NotificationID)
                .Take(maxNotifications)
                .ToListAsync();

            return sellerNotificationsDb.Select(sellerNotification => sellerNotification.NotificationMessage).ToList();
        }

        /// <summary>
        /// Updates the seller information.
        /// </summary>
        /// <param name="seller">The seller whose information is to be updated.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task UpdateSeller(Seller seller)
        {
            this.dbContext.Sellers.Update(seller);
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Gets the products for a given seller.
        /// </summary>
        /// <param name="sellerID">The ID of the seller whose products are to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of products.</returns>
        public async Task<List<Product>> GetProducts(int sellerID)
        {
            return await this.dbContext.Products.Where(product => product.SellerId == sellerID).ToListAsync();
        }

        /// <summary>
        /// Adds a new seller.
        /// </summary>
        /// <param name="seller">The seller to be added.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task AddSeller(Seller seller)
        {
            this.dbContext.Sellers.Add(seller);
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Gets the reviews for a given seller.
        /// </summary>
        /// <param name="sellerId">The ID of the seller whose reviews are to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of reviews.</returns>
        public async Task<List<Review>> GetReviews(int sellerId)
        {
            return await this.dbContext.Reviews.Where(review => review.SellerId == sellerId).ToListAsync();
        }

        /// <summary>
        /// Updates the trust score for a given seller.
        /// </summary>
        /// <param name="sellerId">The ID of the seller whose trust score is to be updated.</param>
        /// <param name="trustScore">The new trust score.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when the seller is not found.</exception>
        public async Task UpdateTrustScore(int sellerId, double trustScore)
        {
            Seller seller = await this.dbContext.Sellers.FindAsync(sellerId)
                                    ?? throw new Exception("UpdateTrustScore: Seller not found");
            seller.TrustScore = trustScore;
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Gets the last follower count for a given seller.
        /// </summary>
        /// <param name="sellerId">The ID of the seller whose last follower count is to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the last follower count.</returns>
        public async Task<int> GetLastFollowerCount(int sellerId)
        {
            SellerNotificationEntity? sellerNotificationDb = await this.dbContext.SellerNotifications
                .Where(sellerNotification => sellerNotification.SellerID == sellerId)
                .OrderByDescending(sellerNotification => sellerNotification.NotificationID)
                .FirstOrDefaultAsync(); // equivalent to SELECT TOP 1 ... FROM ... ORDER BY ... DESC

            return sellerNotificationDb?.NotificationFollowerCount ?? 0;
        }

        /// <summary>
        /// Adds a new follower notification for a given seller.
        /// </summary>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <param name="currentFollowerCount">The current follower count.</param>
        /// <param name="message">The notification message.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task AddNewFollowerNotification(int sellerId, int currentFollowerCount, string message)
        {
            SellerNotificationEntity sellerNotification = new SellerNotificationEntity(sellerId, message, currentFollowerCount);
            this.dbContext.SellerNotifications.Add(sellerNotification);
            await this.dbContext.SaveChangesAsync();
        }
    }
}
