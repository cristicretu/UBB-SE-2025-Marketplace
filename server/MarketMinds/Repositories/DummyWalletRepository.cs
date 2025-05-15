// <copyright file="DummyWalletRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Repository
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Server.DataModels;
    using Server.DBConnection;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// Provides database operations for wallet management.
    /// </summary>
    public class DummyWalletRepository : IDummyWalletRepository
    {
        private readonly MarketPlaceDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyWalletRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public DummyWalletRepository(MarketPlaceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task<double> GetWalletBalanceAsync(int userId)
        {
            DummyWalletEntity wallet = await this.dbContext.DummyWallets.FirstOrDefaultAsync(dummyWallet => dummyWallet.BuyerId == userId)
                                    ?? throw new Exception($"GetWalletBalanceAsync: Wallet not found for buyer with ID: {userId}");
            return wallet.Balance;
        }

        /// <inheritdoc/>
        public async Task UpdateWalletBalance(int userId, double newBalance)
        {
            DummyWalletEntity wallet = await this.dbContext.DummyWallets.FirstOrDefaultAsync(dummyWallet => dummyWallet.BuyerId == userId)
                                    ?? throw new Exception($"UpdateWalletBalance: Wallet not found for buyer with ID: {userId}");

            wallet.Balance = newBalance;
            await this.dbContext.SaveChangesAsync();
        }
    }
}