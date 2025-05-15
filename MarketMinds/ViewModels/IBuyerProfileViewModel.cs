// <copyright file="IBuyerProfileViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.Service;

    /// <summary>
    /// Interface for managing buyer profile view model operations and linkage updates.
    /// </summary>
    public interface IBuyerProfileViewModel : INotifyPropertyChanged, IOnBuyerLinkageUpdatedCallback
    {
        /// <summary>
        /// Gets or sets the buyer service instance.
        /// </summary>
        IBuyerService BuyerService { get; set; }

        /// <summary>
        /// Gets or sets the wishlist item details provider.
        /// </summary>
        IBuyerWishlistItemDetailsProvider WishlistItemDetailsProvider { get; set; }

        /// <summary>
        /// Gets or sets the user associated with the profile.
        /// </summary>
        User User { get; set; }

        /// <summary>
        /// Gets the buyer associated with the profile.
        /// </summary>
        Buyer? Buyer { get; }

        /// <summary>
        /// Gets or sets the wishlist view model.
        /// </summary>
        IBuyerWishlistViewModel? Wishlist { get; set; }

        /// <summary>
        /// Gets or sets the family sync view model.
        /// </summary>
        IBuyerFamilySyncViewModel? FamilySync { get; set; }

        /// <summary>
        /// Gets or sets the billing address view model.
        /// </summary>
        IBuyerAddressViewModel? BillingAddress { get; set; }

        /// <summary>
        /// Gets or sets the shipping address view model.
        /// </summary>
        IBuyerAddressViewModel? ShippingAddress { get; set; }

        /// <summary>
        /// Gets or sets the buyer badge view model.
        /// </summary>
        IBuyerBadgeViewModel? BuyerBadge { get; set; }

        /// <summary>
        /// Gets a value indicating whether the shipping address can be edited.
        /// </summary>
        bool ShippingAddressEnabled { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the shipping address is disabled.
        /// </summary>
        bool ShippingAddressDisabled { get; set; }

        /// <summary>
        /// Saves the buyer profile information.
        /// </summary>
        void SaveInfo();

        /// <summary>
        /// Resets the buyer profile information to its original state.
        /// </summary>
        void ResetInfo();

        /// <summary>
        /// Updates the profile after a purchase is made.
        /// </summary>
        void AfterPurchase();

        /// <summary>
        /// Adds a purchase to the buyer's profile.
        /// </summary>
        /// <param name="purchaseAmount">The amount of money for the purchase.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddPurchase(string purchaseAmount);

        /// <summary>
        /// Loads the buyer profile data.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LoadBuyerProfile();
    }
}
