// -----------------------------------------------------------------------
// <copyright file="ISellerProfileViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace MarketPlace924.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using SharedClassLibrary.Domain;

    /// <summary>
    /// Interface defining the contract for the seller profile view model.
    /// </summary>
    public interface ISellerProfileViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the seller entity.
        /// </summary>
        Seller Seller { get; }

        /// <summary>
        /// Gets or sets the filtered products collection.
        /// </summary>
        ObservableCollection<Product> FilteredProducts { get; set; }

        /// <summary>
        /// Gets or sets the notifications collection.
        /// </summary>
        ObservableCollection<string> Notifications { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the expander is expanded.
        /// </summary>
        bool IsExpanderExpanded { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        string Username { get; set; }

        /// <summary>
        /// Gets or sets the followers count.
        /// </summary>
        string FollowersCount { get; set; }

        /// <summary>
        /// Gets or sets the store name.
        /// </summary>
        string StoreName { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        string Email { get; set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        string Address { get; set; }

        /// <summary>
        /// Gets or sets the trust score.
        /// </summary>
        double TrustScore { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Gets or sets the products collection.
        /// </summary>
        ObservableCollection<Product> Products { get; set; }

        /// <summary>
        /// Gets or sets the store name error message.
        /// </summary>
        string StoreNameError { get; set; }

        /// <summary>
        /// Gets or sets the email error message.
        /// </summary>
        string EmailError { get; set; }

        /// <summary>
        /// Gets or sets the phone number error message.
        /// </summary>
        string PhoneNumberError { get; set; }

        /// <summary>
        /// Gets or sets the address error message.
        /// </summary>
        string AddressError { get; set; }

        /// <summary>
        /// Gets or sets the description error message.
        /// </summary>
        string DescriptionError { get; set; }

        /// <summary>
        /// Loads notifications asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LoadNotifications();

        /// <summary>
        /// Filters products based on search text.
        /// </summary>
        /// <param name="searchText">The text to filter products by.</param>
        void FilterProducts(string searchText);

        /// <summary>
        /// Sorts products by price.
        /// </summary>
        void SortProducts();

        /// <summary>
        /// Updates the seller profile asynchronously.
        /// </summary>
        void UpdateProfile();

        /// <summary>
        /// Validates all fields and returns error messages.
        /// </summary>
        /// <returns>A list of error messages.</returns>
        List<string> ValidateFields();
    }
}
