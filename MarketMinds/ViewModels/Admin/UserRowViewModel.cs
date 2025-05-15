// <copyright file="UserRowViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace MarketPlace924.ViewModel.Admin
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.Helper;
    using SharedClassLibrary.Service;

    /// <summary>
    /// The user row view model.
    /// </summary>
    public class UserRowViewModel : IUserRowViewModel
    {
        /// <summary>
        /// The admin service.
        /// </summary>
        private readonly IAdminService adminService;

        /// <summary>
        /// The admin view model.
        /// </summary>
        private readonly IAdminViewModel adminViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRowViewModel"/> class.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="adminService">The admin service.</param>
        /// <param name="adminViewModel">The admin view model.</param>
        public UserRowViewModel(User user, IAdminService adminService, IAdminViewModel adminViewModel)
        {
            this.User = user;
            this.adminService = adminService;
            this.adminViewModel = adminViewModel;
            this.BanUserCommand = new RelayCommand(this.BanUser);
            this.SetAdminCommand = new RelayCommand(this.SetAdmin);
        }

        /// <summary>
        /// Gets the user.
        /// </summary>
        public User User { get; }

        /// <summary>
        /// Gets the user id.
        /// </summary>
        public int UserId => this.User.UserId;

        /// <summary>
        /// Gets the username.
        /// </summary>
        public string Username => this.User.Username;

        /// <summary>
        /// Gets the email.
        /// </summary>
        public string Email => this.User.Email;

        /// <summary>
        /// Gets the role.
        /// </summary>
        public string Role => this.User.Role.ToString();

        /// <summary>
        /// Gets the failed logins.
        /// </summary>
        public int FailedLogins => this.User.FailedLogins;

        /// <summary>
        /// Gets the banned until.
        /// </summary>
        public DateTime? BannedUntil => this.User.BannedUntil;

        /// <summary>
        /// Gets a value indicating whether the user is banned or not.
        /// </summary>
        public bool IsBanned => this.User.IsBanned;

        /// <summary>
        /// Gets the ban user command.
        /// </summary>
        public ICommand BanUserCommand { get; }

        /// <summary>
        /// Gets the set admin command.
        /// </summary>
        public ICommand SetAdminCommand { get; }

        /// <summary>
        /// Method to "ban" the user.
        /// </summary>
        private async Task BanUser()
        {
            await this.adminService.ToggleUserBanStatus(this.User);
            this.adminViewModel.RefreshUsers();
        }

        /// <summary>
        /// Method to set the user as admin.
        /// </summary>
        private async Task SetAdmin()
        {
            await this.adminService.SetUserAdmin(this.User);
            this.adminViewModel.RefreshUsers();
        }
    }
}
