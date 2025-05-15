// <copyright file="IAdminService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.Service
{
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;

    /// <summary>
    /// Provides administrative operations related to user management.
    /// </summary>
    public interface IAdminService
    {
        /// <summary>
        /// Bans or unbans a user.
        /// </summary>
        /// <param name="user">The user to be banned or unbanned.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task ToggleUserBanStatus(User user);

        /// <summary>
        /// Sets the role of a user to Admin.
        /// </summary>
        /// <param name="user">The user whose role is to be updated.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task SetUserAdmin(User user);
    }
}
