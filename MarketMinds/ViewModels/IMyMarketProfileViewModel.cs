// <copyright file="IMyMarketProfileViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.ViewModel
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Input;
    using SharedClassLibrary.Domain;

/// <summary>
    /// Interface for MyMarketProfileViewModel.
    /// </summary>
    public interface IMyMarketProfileViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        string Address { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        string Email { get; set; }

        /// <summary>
        /// Gets the follow button color.
        /// </summary>
        string FollowButtonColor { get; }

        /// <summary>
        /// Gets the follow button text.
        /// </summary>
        string FollowButtonText { get; }

        /// <summary>
        /// Gets the follow command.
        /// </summary>
        ICommand FollowCommand { get; }

        /// <summary>
        /// Gets or sets the followers count.
        /// </summary>
        string FollowersCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is following.
        /// </summary>
        bool IsFollowing { get; set; }

        /// <summary>
        /// Gets or sets the notifications.
        /// </summary>
        ObservableCollection<string> Notifications { get; set; }

        /// <summary>
        /// Gets the command for adding a product to the shopping cart.
        /// </summary>
        ICommand AddToCartCommand { get; }


        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the seller products.
        /// </summary>
        ObservableCollection<Product> SellerProducts { get; set; }

        /// <summary>
        /// Gets or sets the store name.
        /// </summary>
        string StoreName { get; set; }

        /// <summary>
        /// Gets or sets the trust score.
        /// </summary>
        double TrustScore { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        string Username { get; set; }

        /// <summary>
        /// Filters the products based on the search text.
        /// </summary>
        /// <param name="searchText">The search text.</param>
        void FilterProducts(string searchText);
    }
}