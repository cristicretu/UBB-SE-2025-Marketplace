// -----------------------------------------------------------------------
// <copyright file="SellerProfileViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace MarketMinds.ViewModels
{
    using System.Windows.Media.Animation;
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
    public class SellerProfileViewModel : ISellerProfileViewModel
    {
        private readonly ISellerService sellerService;
        public User User { get; set; } = null!;
        private Seller seller = null!;
        private ObservableCollection<Product> allProducts;
        private ObservableCollection<Product> filteredProducts;
        private bool isExpanderExpanded = false;
        private const double MultiplierForTrustScoreFromAverageReview = 20.0;
        // Add products property to fulfill interface
        public ObservableCollection<Product> Products { get; set; }
        // Add validation properties to fulfill interface
        public string StoreNameError { get; set; }
        public string EmailError { get; set; }
        public string PhoneNumberError { get; set; }
        public string AddressError { get; set; }
        public string DescriptionError { get; set; }
        public string DisplayName { get; set; }
        public Seller Seller => this.seller;
        public SellerProfileViewModel(ISellerService sellerService)
        {
            this.sellerService = sellerService ?? throw new ArgumentNullException(nameof(sellerService));

            this.StoreName = "Loading...";
            this.Username = string.Empty;
            this.Email = string.Empty;
            this.PhoneNumber = string.Empty;
            this.StoreAddress = string.Empty;
            this.StoreDescription = string.Empty;
            this.FollowersCount = "0";
            this.TrustScore = 0;

            this.allProducts = new ObservableCollection<Product>();
            this.Products = new ObservableCollection<Product>();
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

                // this.seller = await this.sellerService.GetSellerByUser(this.User);

                // if (this.seller == null)
                // {
                //     Debug.WriteLine("ERROR: GetSellerByUser returned NULL");
                //     this.Notifications.Add("Error: Failed to load seller data");

                // this.seller = new Seller(this.User);
                //     Debug.WriteLine("Created new Seller object with User");
                // }
                // else
                // {
                //     Debug.WriteLine($"Loaded seller with ID: {this.seller.Id}");
                //     this.Notifications.Add($"Debug: Loaded seller ID {this.seller.Id}");
                // }

                // Debug.WriteLine("SellerProfileViewModel initialization complete");
                this.Notifications.Add("Debug: Initialization complete");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR during SellerProfileViewModel initialization: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                this.Notifications.Add($"Error: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public void LoadProfileAsync()
        {
            _ = this.LoadSellerProfile();
            _ = this.LoadNotifications();
            _ = this.LoadProducts();
        }

        /// <summary>
        /// Loads seller information into ViewModel properties.
        /// </summary>
        private async Task LoadSellerProfile()
        {
            Debug.WriteLine("LoadSellerProfile called");

            this.seller = await this.sellerService.GetSellerByUser(this.User);

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
        /// Updates seller and User profile data via service.
        /// </summary>
        public async void UpdateProfile()
        {
            // add input validation here
            var errors = ValidateFields();
            if (errors.Count > 0)
            {
                string errorMessage = string.Join("\n", errors);
                await this.ShowDialog("Validation Errors", errorMessage);
                Debug.WriteLine("Dialog was shown");
                return;
            }
            Debug.WriteLine("UpdateProfile called");
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
                    // Reload the seller data from the database to ensure we have the latest values
                    this.seller = await this.sellerService.GetSellerByUser(this.User);
                    // Reload the profile data to update all UI bindings
                    await LoadSellerProfile();
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
            // Clear the current filtered products
            this.FilteredProducts.Clear();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                // If no search text, show all products
                foreach (var product in this.allProducts)
                {
                    this.FilteredProducts.Add(product);
                }
                Debug.WriteLine($"No search text provided. Showing all {this.FilteredProducts.Count} products.");
                return;
            }
            // Case insensitive search
            searchText = searchText.ToLower();
            // Filter products based on Title, Description, or Price containing the search text
            var filteredList = this.allProducts.Where(p =>
                (p.Title?.ToLower().Contains(searchText) == true) ||
                (p.Description?.ToLower().Contains(searchText) == true) ||
                p.Price.ToString().Contains(searchText)).ToList();
            // Add the filtered products to the observable collection
            foreach (var product in filteredList)
            {
                this.FilteredProducts.Add(product);
            }
            Debug.WriteLine($"Found {this.FilteredProducts.Count} products matching '{searchText}'");
            // Notify UI that the filtered products have changed
            this.OnPropertyChanged(nameof(this.FilteredProducts));
        }

        /// <summary>
        /// Loads notifications for the seller.
        /// </summary>
        public async Task LoadNotifications()
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
                    this.Products.Clear();

                    if (products != null && products.Count > 0)
                    {
                        foreach (var product in products)
                        {
                            this.allProducts.Add(product);
                            this.FilteredProducts.Add(product);
                            this.Products.Add(product);
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
        /// Sorts the currently filtered products by price.
        /// </summary>
        public void SortProducts()
        {
            Debug.WriteLine("SortProducts called");
            // Static variable to remember sort order (toggles between ascending and descending)
            bool sortAscending = true;
            // Toggle sort order
            sortAscending = !sortAscending;
            Debug.WriteLine($"Sorting products by price: {(sortAscending ? "ascending" : "descending")}");
            // Create a temporary list to hold all products
            var tempList = new List<Product>(this.FilteredProducts);
            // Sort by price
            if (sortAscending)
            {
                tempList = tempList.OrderBy(p => p.Price).ToList();
            }
            else
            {
                tempList = tempList.OrderByDescending(p => p.Price).ToList();
            }
            // Clear the collection and repopulate with sorted items
            this.FilteredProducts.Clear();
            foreach (var product in tempList)
            {
                this.FilteredProducts.Add(product);
            }
            // Notify UI that the filtered products have changed
            this.OnPropertyChanged(nameof(this.FilteredProducts));
        }

        /// <summary>
        /// Validates all input fields in the seller profile.
        /// </summary>
        public List<string> ValidateFields()
        {
            var errors = new List<string>();
            // Add validation logic
            if (string.IsNullOrWhiteSpace(this.StoreName))
            {
                errors.Add("Store name is required.");
                this.StoreNameError = "Store name is required.";
            }
            else
            {
                this.StoreNameError = string.Empty;
            }
            if (string.IsNullOrWhiteSpace(this.Email) || !this.Email.Contains("@"))
            {
                errors.Add("Valid email is required.");
                this.EmailError = "Valid email is required.";
            }
            else
            {
                this.EmailError = string.Empty;
            }
            // check if phone number is valid, starts with +40 and has 10 digits starting with 0
            if (string.IsNullOrWhiteSpace(this.PhoneNumber) || !this.PhoneNumber.StartsWith("+40") || this.PhoneNumber.Length != 12)
            {
                errors.Add("Valid phone number is required.");
                this.PhoneNumberError = "Valid phone number is required.";
            }
            else
            {
                this.PhoneNumberError = string.Empty;
            }
            if (string.IsNullOrWhiteSpace(this.StoreAddress))
            {
                errors.Add("Store address is required.");
                this.AddressError = "Store address is required.";
            }
            else
            {
                this.AddressError = string.Empty;
            }
            if (string.IsNullOrWhiteSpace(this.StoreDescription))
            {
                errors.Add("Store description is required.");
                this.DescriptionError = "Store description is required.";
            }
            else
            {
                this.DescriptionError = string.Empty;
            }
            // Notify UI of changes
            return errors;
        }

        /// <summary>
        /// Shows a content dialog message.
        /// </summary>
        private async Task ShowDialog(string title, string message)
        {
            try
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    CloseButtonText = "OK"
                };

                // Safely set XamlRoot only if it's available
                if (App.MainWindow?.Content?.XamlRoot != null)
                {
                    dialog.XamlRoot = App.MainWindow.Content.XamlRoot;
                }
                else
                {
                    Debug.WriteLine("Warning: XamlRoot is null, dialog might not display properly");
                }

                // Use ConfigureAwait(true) to ensure we stay on the UI thread
                await dialog.ShowAsync().AsTask().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing dialog: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
            }
        }
    }
}
