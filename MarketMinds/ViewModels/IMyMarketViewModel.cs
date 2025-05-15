// <copyright file="IMyMarketViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.ViewModel
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.Service;
    using Microsoft.UI.Xaml;

    /// <summary>
    /// Interface for the "My Market" viewmodel, managing products, followed sellers, and UI state.
    /// </summary>
    public interface IMyMarketViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the filtered list of all sellers.
        /// </summary>
        ObservableCollection<Seller> AllSellersList { get; set; }

        /// <summary>
        /// Gets the buyer information.
        /// </summary>
        Buyer Buyer { get; }

        /// <summary>
        /// Gets the buyer service accessor.
        /// </summary>
        IBuyerService BuyerService { get; }

        /// <summary>
        /// Gets the number of followed sellers.
        /// </summary>
        int FollowedSellersCount { get; }

        /// <summary>
        /// Gets the visibility for the list of followers.
        /// </summary>
        Visibility FollowingListVisibility { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the list of followers is visible.
        /// </summary>
        bool IsFollowingListVisible { get; set; }

        /// <summary>
        /// Gets or sets the filtered followed sellers list.
        /// </summary>
        ObservableCollection<Seller> MyMarketFollowing { get; set; }

        /// <summary>
        /// Gets or sets the filtered products displayed in the "My Market" feed.
        /// </summary>
        ObservableCollection<Product> MyMarketProducts { get; set; }

        /// <summary>
        /// Gets the command to toggle the visibility of the list of followers.
        /// </summary>
        ICommand ShowFollowingCommand { get; }

        /// <summary>
        /// Gets the visibility for the button to show the list of followers.
        /// </summary>
        Visibility ShowFollowingVisibility { get; }

        /// <summary>
        /// Filters all sellers based on search query.
        /// </summary>
        /// <param name="searchText">The search query.</param>
        void FilterAllSellers(string searchText);

        /// <summary>
        /// Filters followed sellers based on search query.
        /// </summary>
        /// <param name="searchText">The search query.</param>
        void FilterFollowing(string searchText);

        /// <summary>
        /// Filters products based on search query.
        /// </summary>
        /// <param name="searchText">The search query.</param>
        void FilterProducts(string searchText);

        /// <summary>
        /// Loads the list of all sellers.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LoadAllSellers();

        /// <summary>
        /// Loads the list of followed sellers.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LoadFollowing();

        /// <summary>
        /// Loads products from followed sellers.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LoadMyMarketData();

        /// <summary>
        /// Refreshes buyer and market data.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RefreshData();
    }
}