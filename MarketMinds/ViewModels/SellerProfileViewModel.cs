// -----------------------------------------------------------------------
// <copyright file="SellerProfileViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace MarketMinds.ViewModels
{
    using System.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using MarketMinds.Shared.Models;
    using MarketMinds.Shared.Services;
    using MarketMinds.Shared.Services.UserService;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// View model for the seller profile page.
    /// </summary>
    public class SellerProfileViewModel : INotifyPropertyChanged
    {
        private readonly ISellerService sellerService;
        private readonly User user;
        private Seller seller;
        private ObservableCollection<Product> allProducts;
        private ObservableCollection<Product> filteredProducts;
        private bool isExpanderExpanded = false;
        private const double MultiplierForTrustScoreFromAverageReview = 20.0;
        public Seller Seller => this.seller;
        public SellerProfileViewModel(ISellerService sellerService, User user)
        {
            Debug.WriteLine($"SellerProfileViewModel constructor called with User ID: {user?.Id ?? -1}");

            this.sellerService = sellerService ?? throw new ArgumentNullException(nameof(sellerService));
            this.user = user ?? throw new ArgumentNullException(nameof(user));

            this.StoreName = "Loading...";
            this.Username = string.Empty;
            this.Email = string.Empty;
            this.PhoneNumber = string.Empty;
            this.StoreAddress = string.Empty;
            this.StoreDescription = string.Empty;
            this.FollowersCount = "0";
            this.TrustScore = 0;

            this.allProducts = new ObservableCollection<Product>();
            this.FilteredProducts = new ObservableCollection<Product>();
            this.Notifications = new ObservableCollection<string>();

            this.Notifications.Add("Debug: ViewModel initialized");

            _ = this.Initialize();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Debug.WriteLine($"Property changed: {propertyName}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string DisplayName { get; private set; }
        public string StoreName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string StoreAddress { get; set; }
        public string StoreDescription { get; set; }
        public string FollowersCount { get; set; }
        public double TrustScore { get; set; }

        public bool IsExpanderExpanded
        {
            get => this.isExpanderExpanded;
            set
            {
                this.isExpanderExpanded = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<Product> FilteredProducts { get; set; }
        public ObservableCollection<string> Notifications { get; set; }

        /// <summary>
        /// Loads all seller data including profile, notifications, and products.
        /// </summary>
        private async Task Initialize()
        {
            try
            {
                Debug.WriteLine("SellerProfileViewModel: Beginning initialization");
                this.Notifications.Add("Debug: Loading seller data...");

                this.seller = await this.sellerService.GetSellerByUser(this.user);

                if (this.seller == null)
                {
                    Debug.WriteLine("ERROR: GetSellerByUser returned NULL");
                    this.Notifications.Add("Error: Failed to load seller data");

                    this.seller = new Seller(this.user);
                    Debug.WriteLine("Created new Seller object with User");
                }
                else
                {
                    Debug.WriteLine($"Loaded seller with ID: {this.seller.Id}");
                    this.Notifications.Add($"Debug: Loaded seller ID {this.seller.Id}");
                }

                await this.LoadSellerProfile();
                await this.LoadNotifications();
                await this.LoadProducts();

                Debug.WriteLine("SellerProfileViewModel initialization complete");
                this.Notifications.Add("Debug: Initialization complete");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR during SellerProfileViewModel initialization: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                this.Notifications.Add($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads seller information into ViewModel properties.
        /// </summary>
        private async Task LoadSellerProfile()
        {
            Debug.WriteLine("LoadSellerProfile called");

            if (this.seller == null)
            {
                Debug.WriteLine("ERROR: Seller is NULL");
                return;
            }

            // Seller table fields
            this.StoreName = this.seller.StoreName ?? string.Empty;
            this.StoreAddress = this.seller.StoreAddress ?? string.Empty;
            this.StoreDescription = this.seller.StoreDescription ?? string.Empty;
            this.FollowersCount = this.seller.FollowersCount.ToString();

            // User-linked fields
            this.Username = this.seller.User?.Username ?? string.Empty;
            this.Email = this.seller.User?.Email ?? string.Empty;
            this.PhoneNumber = this.seller.User?.PhoneNumber ?? string.Empty;

            try
            {
                var trustScore = await this.sellerService.CalculateAverageReviewScore(this.seller.Id);
                this.TrustScore = trustScore * MultiplierForTrustScoreFromAverageReview;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error calculating trust score: {ex.Message}");
                this.TrustScore = 0;
            }

            this.DisplayName = !string.IsNullOrEmpty(this.Username)
                ? this.Username
                : (!string.IsNullOrEmpty(this.StoreName) ? this.StoreName : "Seller");

            // Notify UI
            this.OnPropertyChanged(nameof(this.DisplayName));
            this.OnPropertyChanged(nameof(this.StoreName));
            this.OnPropertyChanged(nameof(this.Username));
            this.OnPropertyChanged(nameof(this.Email));
            this.OnPropertyChanged(nameof(this.PhoneNumber));
            this.OnPropertyChanged(nameof(this.StoreAddress));
            this.OnPropertyChanged(nameof(this.StoreDescription));
            this.OnPropertyChanged(nameof(this.FollowersCount));
            this.OnPropertyChanged(nameof(this.TrustScore));

            Debug.WriteLine("LoadSellerProfile completed");
        }

        /// <summary>
        /// Updates seller and user profile data via service.
        /// </summary>
        public async void UpdateProfile()
        {
            if (this.seller != null)
            {
                this.seller.StoreName = this.StoreName;
                this.seller.StoreAddress = this.StoreAddress;
                this.seller.StoreDescription = this.StoreDescription;

                if (this.seller.User != null)
                {
                    this.seller.User.Username = this.Username;
                    this.seller.User.Email = this.Email;
                    this.seller.User.PhoneNumber = this.PhoneNumber;
                }

                try
                {
                    await this.sellerService.UpdateSeller(this.seller);
                    await ShowDialog("Success", "Your profile has been updated successfully.");
                }
                catch (Exception ex)
                {
                    await ShowDialog("Error", $"Failed to update profile: {ex.Message}");
                }
            }
            else
            {
                await ShowDialog("Error", "Could not update profile: Seller information is missing.");
            }
        }

        /// <summary>
        /// Filters the product list by name.
        /// </summary>
        public void FilterProducts(string searchText)
        {
            Debug.WriteLine($"FilterProducts called with search text: '{searchText}'");
            // You can implement actual filtering here
        }

        /// <summary>
        /// Loads notifications for the seller.
        /// </summary>
        private async Task LoadNotifications()
        {
            try
            {
                if (this.seller != null && this.seller.Id > 0)
                {
                    var notifications = await this.sellerService.GetNotifications(this.seller.Id, 10);
                    this.Notifications.Clear();

                    if (notifications != null && notifications.Count > 0)
                    {
                        foreach (var notification in notifications)
                        {
                            this.Notifications.Add(notification);
                        }
                    }
                    else
                    {
                        this.Notifications.Add("No notifications found");
                    }
                }
            }
            catch (Exception ex)
            {
                this.Notifications.Add($"Error loading notifications: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the seller's listed products.
        /// </summary>
        private async Task LoadProducts()
        {
            try
            {
                if (this.seller != null && this.seller.Id > 0)
                {
                    var products = await this.sellerService.GetAllProducts(this.seller.Id);

                    this.allProducts.Clear();
                    this.FilteredProducts.Clear();

                    if (products != null && products.Count > 0)
                    {
                        foreach (var product in products)
                        {
                            this.allProducts.Add(product);
                            this.FilteredProducts.Add(product);
                        }

                        this.Notifications.Add($"Loaded {products.Count} products");
                    }
                    else
                    {
                        this.Notifications.Add("No products found");
                    }
                }
            }
            catch (Exception ex)
            {
                this.Notifications.Add($"Error loading products: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows a content dialog message.
        /// </summary>
        private async Task ShowDialog(string title, string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = App.MainWindow.Content.XamlRoot,
            };

            await dialog.ShowAsync();
        }
    }
}
