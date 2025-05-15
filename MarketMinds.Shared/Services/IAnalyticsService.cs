// <copyright file="IAnalyticsService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.Service
{
    using System.Threading.Tasks;

    /// <summary>
    /// Provides methods for retrieving data related to users and buyers.
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>
        /// Retrieves the total number of users.
        /// </summary>
        /// <returns>A <see cref="Task{int}"/> representing the result of the asynchronous operation. The task result contains the total number of users.</returns>
        Task<int> GetTotalNumberOfUsers();

        /// <summary>
        /// Retrieves the total number of buyers.
        /// </summary>
        /// <returns>A <see cref="Task{int}"/> representing the result of the asynchronous operation. The task result contains the total number of buyers.</returns>
        Task<int> GetTotalNumberOfBuyers();
    }
}
