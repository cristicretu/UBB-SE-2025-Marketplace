using System;
using System.Threading.Tasks;
using SharedClassLibrary.ProxyRepository;
using SharedClassLibrary.Shared;
using SharedClassLibrary.IRepository;
using SharedClassLibrary.Helper;

namespace SharedClassLibrary.Service
{
    /// <summary>
    /// Service for managing wallet operations.
    /// </summary>
    public class DummyWalletService : IDummyWalletService
    {
        private readonly IDummyWalletRepository dummyWalletRepository;


        /// <summary>
        /// Initializes a new instance of the <see cref="DummyWalletService"/> class with a specified database provider.
        /// </summary>
        public DummyWalletService()
        {
            this.dummyWalletRepository = new DummyWalletProxyRepository(AppConfig.GetBaseApiUrl());
        }

        /// <inheritdoc/>
        public async Task<double> GetWalletBalanceAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User ID must be positive", nameof(userId));
            }

            return await dummyWalletRepository.GetWalletBalanceAsync(userId);
        }

        /// <inheritdoc/>
        public async Task UpdateWalletBalance(int userId, double newBalance)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User ID must be positive", nameof(userId));
            }

            if (newBalance < 0)
            {
                throw new ArgumentException("Wallet balance cannot be negative", nameof(newBalance));
            }

            await dummyWalletRepository.UpdateWalletBalance(userId, newBalance);
        }
    }
}