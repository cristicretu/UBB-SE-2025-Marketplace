// <copyright file="IUserRowViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.ViewModel.Admin
{
    using System;
    using System.Windows.Input;
    using SharedClassLibrary.Domain;

    /// <summary>
    /// The user row view model interface.
    /// </summary>
    public interface IUserRowViewModel
    {
        /// <summary>
        /// Gets the user.
        /// </summary>
        User User { get; }

        /// <summary>
        /// Gets the user id.
        /// </summary>
        int UserId { get; }

        /// <summary>
        /// Gets the username.
        /// </summary>
        string Username { get; }

        /// <summary>
        /// Gets the email.
        /// </summary>
        string Email { get; }

        /// <summary>
        /// Gets the role.
        /// </summary>
        string Role { get; }

        /// <summary>
        /// Gets the failed logins.
        /// </summary>
        int FailedLogins { get; }

        /// <summary>
        /// Gets the banned until.
        /// </summary>
        DateTime? BannedUntil { get; }

        /// <summary>
        /// Gets a value indicating whether the user is banned or not.
        /// </summary>
        bool IsBanned { get; }

        /// <summary>
        /// Gets the ban user command.
        /// </summary>
        ICommand BanUserCommand { get; }

        /// <summary>
        /// Gets the set admin command.
        /// </summary>
        ICommand SetAdminCommand { get; }
    }
}
