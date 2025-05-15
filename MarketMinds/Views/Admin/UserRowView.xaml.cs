// <copyright file="UserRowView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.View.Admin
{
    using System;
    using SharedClassLibrary.Domain;
    using MarketPlace924.ViewModel.Admin;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// A control that displays user information in a row format.
    /// </summary>
    public sealed partial class UserRowView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserRowView"/> class.
        /// </summary>
        /// <param name="user">The user to display.</param>
        public UserRowView(User user)
        {
            this.InitializeComponent();
            this.User = user;
        }

        /// <summary>
        /// Gets the user associated with this row view.
        /// </summary>
        public User User { get; }

        /// <summary>
        /// Gets the user's ID.
        /// </summary>
        public int UserId => this.User.UserId;

        /// <summary>
        /// Gets the user's username.
        /// </summary>
        public string Username => this.User.Username;

        /// <summary>
        /// Gets the user's email address.
        /// </summary>
        public string Email => this.User.Email;

        /// <summary>
        /// Gets the user's phone number.
        /// </summary>
        public string PhoneNumber => this.User.PhoneNumber;

        /// <summary>
        /// Gets the user's role as a string.
        /// </summary>
        public string Role => this.User.Role.ToString();

        /// <summary>
        /// Gets the count of failed login attempts.
        /// </summary>
        public int FailedLogins => this.User.FailedLogins;

        /// <summary>
        /// Gets the date until which the user is banned, if any.
        /// </summary>
        public DateTime? BannedUntil => this.User.BannedUntil;

        /// <summary>
        /// Gets a value indicating whether the user is banned.
        /// </summary>
        public bool IsBanned => this.User.IsBanned;

        /// <summary>
        /// Gets or sets the view model for this control.
        /// </summary>
        public IUserRowViewModel ViewModel
        {
            get => (IUserRowViewModel)this.DataContext;
            set => this.DataContext = value;
        }
    }
}
