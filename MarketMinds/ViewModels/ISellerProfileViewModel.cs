// -----------------------------------------------------------------------
// <copyright file="ISellerProfileViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace MarketMinds.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using MarketMinds.Shared.Models;

    /// <summary>
    /// Interface defining the contract for the seller profile view model.
    /// </summary>
    public interface ISellerProfileViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the user entity.
        /// </summary>
        User User { get; set; }

        /// <summary>
        /// Gets the seller entity. Includes the linked User object.
        /// </summary>
        Seller Seller { get; }

        /// <summary>
        /// Gets or sets the filtered products collection shown in the UI.
        /// </summary>
        ObservableCollection<Product> FilteredProducts { get; set; }

        /// <summary>
        /// Gets or sets the full list of notifications shown on the profile.
        /// </summary>
        ObservableCollection<string> Notifications { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the details expander is open.
        /// </summary>
        bool IsExpanderExpanded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether data is currently being loaded.
        /// </summary>
        bool IsLoading { get; set; }

        /// <summary>
        /// Gets or sets the display name shown in the header of the profile.
        /// </summary>
        string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the seller's username (from User).
        /// </summary>
        string Username { get; set; }

        /// <summary>
        /// Gets or sets the name of the store (from Seller).
        /// </summary>
        string StoreName { get; set; }

        /// <summary>
        /// Gets or sets the email address (from User).
        /// </summary>
        string Email { get; set; }

        /// <summary>
        /// Gets or sets the phone number (from User).
        /// </summary>
        string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the store's address (from Seller).
        /// </summary>
        string StoreAddress { get; set; }

        /// <summary>
        /// Gets or sets the seller's calculated trust score (0–100).
        /// </summary>
        double TrustScore { get; set; }

        /// <summary>
        /// Gets the rating value for the RatingControl (0-5 range).
        /// </summary>
        double RatingValue { get; }

        /// <summary>
        /// Gets or sets the store's description.
        /// </summary>
        string StoreDescription { get; set; }

        /// <summary>
        /// Gets or sets the full list of products associated with this seller.
        /// </summary>
        ObservableCollection<Product> Products { get; set; }

        /// <summary>
        /// Gets or sets the list of followers for this seller.
        /// </summary>
        ObservableCollection<Buyer> FollowersList { get; set; }

        /// <summary>
        /// Gets the followers count from the actual followers list.
        /// </summary>
        string ActualFollowersCount { get; }

        // -----------------------------
        // Validation-related properties
        // -----------------------------

        /// <summary>
        /// Gets or sets the validation error message for the username field.
        /// </summary>
        string UsernameError { get; set; }

        /// <summary>
        /// Gets or sets the validation error message for the store name field.
        /// </summary>
        string StoreNameError { get; set; }

        /// <summary>
        /// Gets or sets the validation error message for the email field.
        /// </summary>
        string EmailError { get; set; }

        /// <summary>
        /// Gets or sets the validation error message for the phone number field.
        /// </summary>
        string PhoneNumberError { get; set; }

        /// <summary>
        /// Gets or sets the validation error message for the address field.
        /// </summary>
        string AddressError { get; set; }

        /// <summary>
        /// Gets or sets the validation error message for the store description field.
        /// </summary>
        string DescriptionError { get; set; }

        // -----------------------------
        // Methods
        // -----------------------------

        /// <summary>
        /// Loads the seller profile data asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LoadProfileAsync();

        /// <summary>
        /// Loads the latest seller notifications asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LoadNotifications();

        /// <summary>
        /// Loads the seller's followers asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LoadFollowers();

        /// <summary>
        /// Filters the list of products using a text search.
        /// </summary>
        /// <param name="searchText">Search query entered by the user.</param>
        void FilterProducts(string searchText);

        /// <summary>
        /// Sorts the currently filtered products by price in ascending or descending order.
        /// </summary>
        void SortProducts();

        /// <summary>
        /// Updates the seller's profile in the backend (name, address, user info).
        /// </summary>
        /// <returns>True if the update was successful, false otherwise.</returns>
        Task<bool> UpdateProfile();

        /// <summary>
        /// Validates all input fields in the seller profile and returns any error messages found.
        /// </summary>
        /// <returns>A list of validation error messages. Empty if all fields are valid.</returns>
        List<string> ValidateFields();
    }
}
