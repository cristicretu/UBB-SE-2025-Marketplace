// -----------------------------------------------------------------------
// <copyright file="SellerRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Server.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::MarketMinds.Shared.IRepository;
    using global::MarketMinds.Shared.Models;
    using Microsoft.EntityFrameworkCore;
    using Server.DataAccessLayer;
    using Server.DataModels;

    /// <summary>
    /// Repository for managing seller-related data operations.
    /// </summary>
    public class SellerRepository : ISellerRepository
    {
        private readonly ApplicationDbContext dbContext;

        public SellerRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task<Seller> GetSellerInfo(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null");
            }

            // First, ensure we have a fully loaded User object
            var fullUser = await this.dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            if (fullUser == null)
            {
                throw new Exception("GetSellerInfo: User not found");
            }

            // Then load the Seller
            var sellerDb = await this.dbContext.Sellers
                .FirstOrDefaultAsync(s => s.Id == user.Id);

            if (sellerDb == null)
            {
                throw new Exception("GetSellerInfo: Seller not found");
            }

            // Explicitly set the User property with the complete User data
            sellerDb.User = fullUser;

            return sellerDb;
        }

        /// <inheritdoc/>
        public async Task<List<string>> GetNotifications(int sellerID, int maxNotifications)
        {
            var sellerNotificationsDb = await this.dbContext.SellerNotifications
                .Where(sn => sn.SellerID == sellerID)
                .OrderByDescending(sn => sn.NotificationID)
                .Take(maxNotifications)
                .ToListAsync();

            return sellerNotificationsDb.Select(sn => sn.NotificationMessage).ToList();
        }

        /// <inheritdoc/>
        public async Task UpdateSeller(Seller seller)
        {
            if (seller == null)
            {
                throw new ArgumentNullException(nameof(seller), "Seller cannot be null");
            }

            this.dbContext.Sellers.Update(seller);
            await this.dbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<List<BuyProduct>> GetProducts(int sellerID)
        {
            return await this.dbContext.BuyProducts
                .Where(p => p.SellerId == sellerID)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task AddSeller(Seller seller)
        {
            if (seller == null)
            {
                throw new ArgumentNullException(nameof(seller), "Seller cannot be null");
            }

            this.dbContext.Sellers.Add(seller);
            await this.dbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<List<Review>> GetReviews(int sellerId)
        {
            return await this.dbContext.Reviews
                .Where(r => r.SellerId == sellerId)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateTrustScore(int sellerId, double trustScore)
        {
            var seller = await this.dbContext.Sellers.FindAsync(sellerId)
                         ?? throw new Exception("UpdateTrustScore: Seller not found");

            seller.TrustScore = trustScore;
            await this.dbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<int> GetLastFollowerCount(int sellerId)
        {
            var latestNotification = await this.dbContext.SellerNotifications
                .Where(sn => sn.SellerID == sellerId)
                .OrderByDescending(sn => sn.NotificationID)
                .FirstOrDefaultAsync();

            return latestNotification?.NotificationFollowerCount ?? 0;
        }

        /// <inheritdoc/>
        public async Task AddNewFollowerNotification(int sellerId, int currentFollowerCount, string message)
        {
            var newNotification = new SellerNotificationEntity(sellerId, message, currentFollowerCount);
            this.dbContext.SellerNotifications.Add(newNotification);
            await this.dbContext.SaveChangesAsync();
        }
    }
}
