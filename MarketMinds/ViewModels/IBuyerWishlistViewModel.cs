// <copyright file="IBuyerWishlistViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.Service;

    /// <summary>
    /// Interface for managing buyer wishlist view model operations.
    /// </summary>
    public interface IBuyerWishlistViewModel : INotifyPropertyChanged, IOnBuyerWishlistItemRemoveCallback
    {
        /// <summary>
        /// Gets or sets the buyer associated with the wishlist.
        /// </summary>
        Buyer Buyer { get; set; }

        /// <summary>
        /// Gets or sets the buyer service instance.
        /// </summary>
        IBuyerService BuyerService { get; set; }

        /// <summary>
        /// Gets or sets the provider for wishlist item details.
        /// </summary>
        IBuyerWishlistItemDetailsProvider ItemDetailsProvider { get; set; }

        /// <summary>
        /// Gets the collection of wishlist items.
        /// </summary>
        ObservableCollection<IBuyerWishlistItemViewModel> Items { get; }

        /// <summary>
        /// Gets or sets the search text for filtering items.
        /// </summary>
        string SearchText { get; set; }

        /// <summary>
        /// Gets the collection of available sort options.
        /// </summary>
        ObservableCollection<string> SortOptions { get; }

        /// <summary>
        /// Gets or sets the currently selected sort option.
        /// </summary>
        string? SelectedSort { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether family sync is active.
        /// </summary>
        bool FamilySyncActive { get; set; }

        /// <summary>
        /// Creates a copy of the current wishlist view model.
        /// </summary>
        /// <returns>A new instance of IBuyerWishlistViewModel with copied data.</returns>
        IBuyerWishlistViewModel Copy();
    }
}
