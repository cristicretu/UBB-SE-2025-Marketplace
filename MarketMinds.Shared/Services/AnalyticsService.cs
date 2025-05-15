// <copyright file="AnalyticsService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.Service
{
    using System.Threading.Tasks;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// Provides services related to users and buyers.
    /// </summary>
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IUserRepository userRepository;

        private readonly IBuyerRepository buyerRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsService"/> class.
        /// </summary>
        /// <param name="userRepository">The user repository to be used by the service.</param>
        /// <param name="buyerRepository">The buyer repository to be used by the service.</param>
        public AnalyticsService(IUserRepository userRepository, IBuyerRepository buyerRepository)
        {
            this.userRepository = userRepository;
            this.buyerRepository = buyerRepository;
        }

        /// <summary>
        /// Retrieves the total number of users.
        /// </summary>
        /// <returns>A <see cref="Task{int}"/> representing the result of the asynchronous operation. The task result contains the total number of users.</returns>
        public async Task<int> GetTotalNumberOfUsers()
        {
            return await this.userRepository.GetTotalNumberOfUsers();
        }

        /// <summary>
        /// Retrieves the total number of buyers.
        /// </summary>
        /// <returns>A <see cref="Task{int}"/> representing the result of the asynchronous operation. The task result contains the total number of buyers.</returns>
        public async Task<int> GetTotalNumberOfBuyers()
        {
            return await this.buyerRepository.GetTotalCount();
        }
    }
}