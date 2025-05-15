// <copyright file="AdminService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.Service
{
    using System;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// Provides administrative operations related to user management.
    /// </summary>
    public class AdminService : IAdminService
    {
        private readonly IUserRepository userRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminService"/> class.
        /// </summary>
        /// <param name="userRepository">The user repository to be used by the service.</param>
        public AdminService(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        /// <summary>
        /// Bans or unbans a user.
        /// </summary>
        /// <param name="user">The user to be banned or unbanned.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task ToggleUserBanStatus(User user)
        {
            if (user.IsBanned)
            {
                user.BannedUntil = null;
                user.IsBanned = false;
            }
            else
            {
                user.BannedUntil = DateTime.Now.AddYears(10);
                user.IsBanned = true;
            }

            await this.userRepository.UpdateUser(user);
        }

        /// <summary>
        /// Sets the role of a user to Admin.
        /// </summary>
        /// <param name="user">The user whose role is to be updated.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task SetUserAdmin(User user)
        {
            user.Role = UserRole.Admin;
            await this.userRepository.UpdateUser(user);
        }
    }
}
