// <copyright file="MyMarketViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.Helper;
    using SharedClassLibrary.Service;
    using Microsoft.UI.Xaml;

    /// <summary>
    /// ViewModel for the "My Market" view, managing products, followed sellers, and UI state.
    /// </summary>
    public class MyMarketViewModel : IMyMarketViewModel
    {
        // Services and User Data
        private readonly IBuyerService buyerService;
        private readonly User user;
        private Buyer buyer;

        // Collections for Products and Followed Sellers
        private ObservableCollection<Product> allProducts = new ObservableCollection<Product>();
        private ObservableCollection<Product> filteredProducts = new ObservableCollection<Product>();
        private ObservableCollection<Seller> allFollowedSellers = new ObservableCollection<Seller>();
        private ObservableCollection<Seller> filteredFollowedSellers = new ObservableCollection<Seller>();
        private ObservableCollection<Seller> allSellers = new ObservableCollection<Seller>();
        private ObservableCollection<Seller> filteredAllSellers = new ObservableCollection<Seller>();

        // UI State Management
        private bool isFollowingListVisible;
        private int followedSellersCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyMarketViewModel"/> class.
        /// </summary>
        /// <param name="buyerService">Service for interacting with buyer-related data.</param>
        /// <param name="user">The current user.</param>
        public MyMarketViewModel(IBuyerService buyerService, User user)
        {
            this.buyerService = buyerService;
            this.user = user;

            this.BuyerService = this.buyerService;
            this.ShowFollowingCommand = new RelayCommand(this.ShowFollowingList);
            this.IsFollowingListVisible = false;

            _ = this.LoadData();
        }

        /// <summary>
        /// PropertyChanged event for UI updates.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        // Properties

        /// <summary>
        /// Gets the buyer service accessor.
        /// </summary>
        public IBuyerService BuyerService { get; }

        /// <summary>
        /// Gets the buyer information.
        /// </summary>
        public Buyer Buyer => this.buyer;

        /// <summary>
        /// Gets or sets the filtered products displayed in the "My Market" feed.
        /// </summary>
        public ObservableCollection<Product> MyMarketProducts
        {
            get => this.filteredProducts;
            set
            {
                this.filteredProducts = value;
                this.OnPropertyChanged(nameof(this.MyMarketProducts));
            }
        }

        /// <summary>
        /// Gets or sets the filtered followed sellers list.
        /// </summary>
        public ObservableCollection<Seller> MyMarketFollowing
        {
            get => this.filteredFollowedSellers;
            set
            {
                this.filteredFollowedSellers = value;
                this.OnPropertyChanged(nameof(this.MyMarketFollowing));
            }
        }

        /// <summary>
        /// Gets the number of followed sellers.
        /// </summary>
        public int FollowedSellersCount
        {
            get => this.followedSellersCount;
            private set
            {
                this.followedSellersCount = value;
                this.OnPropertyChanged(nameof(this.FollowedSellersCount));
            }
        }

        /// <summary>
        /// Gets or sets the filtered list of all sellers.
        /// </summary>
        public ObservableCollection<Seller> AllSellersList
        {
            get => this.filteredAllSellers;
            set
            {
                this.filteredAllSellers = value;
                this.OnPropertyChanged(nameof(this.AllSellersList));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the list of followers is visible.
        /// </summary>
        public bool IsFollowingListVisible
        {
            get => this.isFollowingListVisible;
            set
            {
                if (this.isFollowingListVisible != value)
                {
                    this.isFollowingListVisible = value;
                    this.OnPropertyChanged(nameof(this.IsFollowingListVisible));
                    this.OnPropertyChanged(nameof(this.FollowingListVisibility));
                    this.OnPropertyChanged(nameof(this.ShowFollowingVisibility));
                }
            }
        }

        /// <summary>
        /// Gets the visibility for the list of followers.
        /// </summary>
        public Visibility FollowingListVisibility => this.IsFollowingListVisible ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Gets the visibility for the button to show the list of followers.
        /// </summary>
        public Visibility ShowFollowingVisibility => this.IsFollowingListVisible ? Visibility.Collapsed : Visibility.Visible;

        /// <summary>
        /// Gets the command to toggle the visibility of the list of followers.
        /// </summary>
        public ICommand ShowFollowingCommand { get; }

        /// <summary>
        /// Filters products based on search query.
        /// </summary>
        /// <param name="searchText">The search query.</param>
        public void FilterProducts(string searchText)
        {
            this.filteredProducts.Clear();
            var filteredProducts = string.IsNullOrEmpty(searchText)
                ? this.allProducts
                : this.allProducts.Where(p => p.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));

            foreach (var product in filteredProducts)
            {
                this.filteredProducts.Add(product);
            }

            this.OnPropertyChanged(nameof(this.MyMarketProducts));
        }

        /// <summary>
        /// Filters followed sellers based on search query.
        /// </summary>
        /// <param name="searchText">The search query.</param>
        public void FilterFollowing(string searchText)
        {
            this.filteredFollowedSellers.Clear();
            var filteredSellers = string.IsNullOrEmpty(searchText)
                ? this.allFollowedSellers
                : this.allFollowedSellers.Where(s => s.StoreName.Contains(searchText, StringComparison.OrdinalIgnoreCase));

            foreach (var seller in filteredSellers)
            {
                this.filteredFollowedSellers.Add(seller);
            }

            this.OnPropertyChanged(nameof(this.MyMarketFollowing));
        }

        /// <summary>
        /// Filters all sellers based on search query.
        /// </summary>
        /// <param name="searchText">The search query.</param>
        public void FilterAllSellers(string searchText)
        {
            this.filteredAllSellers.Clear();
            var filteredSellers = string.IsNullOrEmpty(searchText)
                ? this.allSellers
                : this.allSellers.Where(s => s.StoreName.Contains(searchText, StringComparison.OrdinalIgnoreCase));

            foreach (var seller in filteredSellers)
            {
                this.filteredAllSellers.Add(seller);
            }

            this.OnPropertyChanged(nameof(this.AllSellersList));
        }

        /// <summary>
        /// Loads products from followed sellers.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadMyMarketData()
        {
            try
            {
                var products = await this.buyerService.GetProductsFromFollowedSellers(this.buyer.FollowingUsersIds);

                var sortedProducts = products.OrderByDescending(p => p.ProductId).ToList();

                this.allProducts.Clear();
                foreach (var product in sortedProducts)
                {
                    this.allProducts.Add(product);
                }

                this.FilterProducts(string.Empty); // Show all products initially
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading market data: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the list of followed sellers.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadFollowing()
        {
            try
            {
                var sellers = await this.buyerService.GetFollowedSellers(this.buyer.FollowingUsersIds);
                this.allFollowedSellers.Clear();
                foreach (var seller in sellers)
                {
                    this.allFollowedSellers.Add(seller);
                }

                this.FollowedSellersCount = this.allFollowedSellers.Count;
                this.FilterFollowing(string.Empty); // Show all followed sellers initially
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading followed sellers: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the list of all sellers.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadAllSellers()
        {
            try
            {
                var sellers = await this.buyerService.GetAllSellers();
                this.allSellers.Clear();
                foreach (var seller in sellers)
                {
                    this.allSellers.Add(seller);
                }

                this.FilterAllSellers(string.Empty); // Show all followed sellers initially
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading followed sellers: {ex.Message}");
            }
        }

        /// <summary>
        /// Refreshes buyer and market data.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RefreshData()
        {
            try
            {
                await this.LoadData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refreshing data: {ex.Message}");
            }
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Loads buyer and market data.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task LoadData()
        {
            this.buyer = await this.buyerService.GetBuyerByUser(this.user);
            this.OnPropertyChanged(nameof(this.Buyer));

            await this.LoadFollowing();
            await this.LoadAllSellers();
            await this.LoadMyMarketData();
        }

        /// <summary>
        /// Toggles followers list visibility and reloads data if shown.
        /// </summary>
        /// <param name="parameter">The command parameter (not used).</param>
        private async void ShowFollowingList(object parameter)
        {
            this.IsFollowingListVisible = !this.IsFollowingListVisible;
            if (this.IsFollowingListVisible)
            {
                await this.LoadFollowing();
            }
        }
    }
}
