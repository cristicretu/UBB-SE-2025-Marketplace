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
    public class SellerProfileViewModel : ISellerProfileViewModel
    {
        private readonly ISellerService sellerService;
        public User User { get; set; } = null!;
        private Seller seller = null!;
        private ObservableCollection<Product> allProducts;
        private ObservableCollection<Product> filteredProducts;
        private bool isExpanderExpanded = false;

        // Backing fields for properties
        private string storeName = string.Empty;
        private string username = string.Empty;
        private string email = string.Empty;
        private string phoneNumber = string.Empty;
        private string storeAddress = string.Empty;
        private string storeDescription = string.Empty;
        private double trustScore = 0;
        private bool isLoading = true;

        // Add products property to fulfill interface
        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<Product> FilteredProducts { get; set; }
        public ObservableCollection<string> Notifications { get; set; }

        // Add followers list property
        public ObservableCollection<Buyer> FollowersList { get; set; }

        // Validation error properties
        private string usernameError = string.Empty;
        private string storeNameError = string.Empty;
        private string emailError = string.Empty;
        private string phoneNumberError = string.Empty;
        private string addressError = string.Empty;
        private string descriptionError = string.Empty;

        public string UsernameError
        {
            get => this.usernameError;
            set
            {
                this.usernameError = value;
                this.OnPropertyChanged();
            }
        }

        public string StoreNameError
        {
            get => this.storeNameError;
            set
            {
                this.storeNameError = value;
                this.OnPropertyChanged();
            }
        }

        public string EmailError
        {
            get => this.emailError;
            set
            {
                this.emailError = value;
                this.OnPropertyChanged();
            }
        }

        public string PhoneNumberError
        {
            get => this.phoneNumberError;
            set
            {
                this.phoneNumberError = value;
                this.OnPropertyChanged();
            }
        }

        public string AddressError
        {
            get => this.addressError;
            set
            {
                this.addressError = value;
                this.OnPropertyChanged();
            }
        }

        public string DescriptionError
        {
            get => this.descriptionError;
            set
            {
                this.descriptionError = value;
                this.OnPropertyChanged();
            }
        }

        public string DisplayName { get; set; }
        public Seller Seller => this.seller;
        public SellerProfileViewModel(ISellerService sellerService)
        {
            this.sellerService = sellerService ?? throw new ArgumentNullException(nameof(sellerService));

            // Initialize backing fields directly to avoid triggering PropertyChanged during construction
            this.storeName = "Loading...";
            this.username = string.Empty;
            this.email = string.Empty;
            this.phoneNumber = string.Empty;
            this.storeAddress = string.Empty;
            this.storeDescription = string.Empty;
            this.trustScore = 0;
            this.isLoading = true; // Start in loading state

            this.allProducts = new ObservableCollection<Product>();
            this.Products = new ObservableCollection<Product>();
            this.FilteredProducts = new ObservableCollection<Product>();
            this.Notifications = new ObservableCollection<string>();
            this.FollowersList = new ObservableCollection<Buyer>();

            _ = this.Initialize();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Debug.WriteLine($"Property changed: {propertyName}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string StoreName
        {
            get => this.storeName;
            set
            {
                this.storeName = value;
                this.OnPropertyChanged();
            }
        }
        public string Username
        {
            get => this.username;
            set
            {
                this.username = value;
                this.OnPropertyChanged();
            }
        }
        public string Email
        {
            get => this.email;
            set
            {
                this.email = value;
                this.OnPropertyChanged();
            }
        }
        public string PhoneNumber
        {
            get => this.phoneNumber;
            set
            {
                this.phoneNumber = value;
                this.OnPropertyChanged();
            }
        }
        public string StoreAddress
        {
            get => this.storeAddress;
            set
            {
                this.storeAddress = value;
                this.OnPropertyChanged();
            }
        }
        public string StoreDescription
        {
            get => this.storeDescription;
            set
            {
                this.storeDescription = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the followers count from the actual followers list.
        /// </summary>
        public string ActualFollowersCount => this.FollowersList?.Count.ToString() ?? "0";

        public double TrustScore
        {
            get => this.trustScore;
            set
            {
                Debug.WriteLine($"TrustScore being set to: {value}");
                this.trustScore = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.RatingValue)); // Also notify RatingValue
                Debug.WriteLine($"Both TrustScore and RatingValue PropertyChanged notifications sent");
            }
        }

        public bool IsExpanderExpanded
        {
            get => this.isExpanderExpanded;
            set
            {
                this.isExpanderExpanded = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => this.isLoading;
            set
            {
                Debug.WriteLine($"IsLoading being set to: {value}");
                this.isLoading = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the rating value specifically for the RatingControl (0-5 range).
        /// </summary>
        public double RatingValue
        {
            get 
            { 
                Debug.WriteLine($"RatingValue getter called, returning: {this.trustScore}");
                return this.trustScore; 
            }
        }

        /// <summary>
        /// Loads all seller data including profile, notifications, and products.
        /// </summary>
        private async Task Initialize()
        {
            try
            {
                Debug.WriteLine("SellerProfileViewModel: Beginning initialization");
                // Initialization logic can be added here if needed
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR during SellerProfileViewModel initialization: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                this.Notifications.Add($"Error: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public async Task LoadProfileAsync()
        {
            Debug.WriteLine("LoadProfileAsync started - setting IsLoading to true");
            this.IsLoading = true;
            
            try
            {
                await this.LoadSellerProfile();
                await this.LoadNotifications();
                await this.LoadProducts();
                await this.LoadFollowers();
                
                Debug.WriteLine("LoadProfileAsync completed - setting IsLoading to false");
                this.IsLoading = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in LoadProfileAsync: {ex.Message}");
                this.IsLoading = false;
                throw;
            }
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

            // User-linked fields
            this.Username = this.seller.User?.Username ?? string.Empty;
            this.Email = this.seller.User?.Email ?? string.Empty;
            this.PhoneNumber = this.seller.User?.PhoneNumber ?? string.Empty;

            try
            {
                var trustScore = await this.sellerService.CalculateAverageReviewScore(this.seller.Id);
                Debug.WriteLine($"Calculated trust score from service: {trustScore}");
                
                // Force update the property and notify UI multiple times to ensure RatingControl gets it
                this.trustScore = trustScore;
                Debug.WriteLine($"TrustScore backing field set to: {this.trustScore}");
                
                // Trigger property changed multiple ways
                this.OnPropertyChanged(nameof(this.TrustScore));
                Debug.WriteLine($"TrustScore property after manual OnPropertyChanged: {this.TrustScore}");
                
                // Also update through the property setter to ensure all notifications
                this.TrustScore = trustScore;
                Debug.WriteLine($"TrustScore property after property setter: {this.TrustScore}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error calculating trust score: {ex.Message}");
                this.TrustScore = 0;
                Debug.WriteLine($"TrustScore set to 0 due to error");
            }

            this.DisplayName = !string.IsNullOrEmpty(this.Username)
                ? this.Username
                : (!string.IsNullOrEmpty(this.StoreName) ? this.StoreName : "Seller");

            // Property setters now automatically notify UI of changes
            this.OnPropertyChanged(nameof(this.DisplayName));
            
            // Explicitly notify UI about TrustScore to ensure RatingControl updates
            this.OnPropertyChanged(nameof(this.TrustScore));

            Debug.WriteLine("LoadSellerProfile completed");
        }

        /// <summary>
        /// Updates seller and User profile data via service.
        /// </summary>
        /// <returns>True if the update was successful, false otherwise.</returns>
        public async Task<bool> UpdateProfile()
        {
            // add input validation here
            var errors = ValidateFields();
            if (errors.Count > 0)
            {
                string errorMessage = string.Join("\n", errors);
                await this.ShowDialog("Validation Errors", errorMessage);
                Debug.WriteLine("Dialog was shown");
                return false;
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
                    return true;
                }
                catch (Exception ex)
                {
                    await ShowDialog("Error", $"Failed to update profile: {ex.Message}");
                    return false;
                }
            }
            else
            {
                await ShowDialog("Error", "Could not update profile: Seller information is missing.");
                return false;
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
                    }
                }
            }
            catch (Exception ex)
            {
                this.Notifications.Add($"Error loading products: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the seller's followers.
        /// </summary>
        public async Task LoadFollowers()
        {
            try
            {
                if (this.seller != null && this.seller.Id > 0)
                {
                    var followers = await this.sellerService.GetFollowers(this.seller.Id);

                    this.FollowersList.Clear();

                    if (followers != null && followers.Count > 0)
                    {
                        foreach (var follower in followers)
                        {
                            this.FollowersList.Add(follower);
                        }

                        Debug.WriteLine($"Loaded {followers.Count} followers for seller {this.seller.Id}");
                    }
                    else
                    {
                        Debug.WriteLine($"No followers found for seller {this.seller.Id}");
                    }

                    // Notify UI that the followers count has changed
                    this.OnPropertyChanged(nameof(this.ActualFollowersCount));
                }
            }
            catch (Exception ex)
            {
                this.Notifications.Add($"Error loading followers: {ex.Message}");
                Debug.WriteLine($"Error loading followers: {ex.Message}");
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
            
            // Clear all error messages first
            this.UsernameError = string.Empty;
            this.StoreNameError = string.Empty;
            this.EmailError = string.Empty;
            this.PhoneNumberError = string.Empty;
            this.AddressError = string.Empty;
            this.DescriptionError = string.Empty;
            
            // Validate Username
            if (string.IsNullOrWhiteSpace(this.Username))
            {
                errors.Add("Username is required.");
                this.UsernameError = "Username is required.";
            }
            
            // Validate Store Name
            if (string.IsNullOrWhiteSpace(this.StoreName))
            {
                errors.Add("Store name is required.");
                this.StoreNameError = "Store name is required.";
            }
            
            // Validate Email
            if (string.IsNullOrWhiteSpace(this.Email) || !this.Email.Contains("@"))
            {
                errors.Add("Valid email is required.");
                this.EmailError = "Valid email is required.";
            }
            
            // Validate Phone Number (Romanian format: +40 followed by 9 digits)
            if (string.IsNullOrWhiteSpace(this.PhoneNumber) || !this.PhoneNumber.StartsWith("+40") || this.PhoneNumber.Length != 12)
            {
                errors.Add("Valid phone number is required (+40 followed by 9 digits).");
                this.PhoneNumberError = "Valid phone number is required (+40 followed by 9 digits).";
            }
            
            // Validate Address
            if (string.IsNullOrWhiteSpace(this.StoreAddress))
            {
                errors.Add("Store address is required.");
                this.AddressError = "Store address is required.";
            }
            
            // Validate Description
            if (string.IsNullOrWhiteSpace(this.StoreDescription))
            {
                errors.Add("Store description is required.");
                this.DescriptionError = "Store description is required.";
            }

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
