// <copyright file="MyMarketProfileViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.Helper;
    using SharedClassLibrary.Service;
    using Microsoft.UI.Xaml.Controls;
    using Windows.System;
    using Windows.UI.Notifications;

    /// <summary>
    /// ViewModel for managing and displaying the market profile of a seller, including products and follow status.
    /// </summary>
    public class MyMarketProfileViewModel : IMyMarketProfileViewModel
    {
        // Private fields to store buyer, seller, and service information
        private Buyer buyer;
        private Seller seller;
        private IBuyerService buyerService;

        // Collections to store all products and filtered products for display
        private ObservableCollection<Product> allProducts;
        private ObservableCollection<Product> filteredProducts;

        // Tracks whether the buyer is following the seller
        private bool isFollowing;

        // Collection to store notifications
        private ObservableCollection<string> notifications = new ObservableCollection<string>();

        /// <summary>
        /// Gets the command for adding a product to the shopping cart.
        /// </summary>
        public ICommand AddToCartCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MyMarketProfileViewModel"/> class.
        /// </summary>
        /// <param name="buyerService">The buyer service for data interaction.</param>
        /// <param name="buyer">The buyer object.</param>
        /// <param name="seller">The seller object.</param>
        public MyMarketProfileViewModel(IBuyerService buyerService, Buyer buyer, Seller seller)
        {
            this.buyerService = buyerService;
            this.buyer = buyer;
            this.seller = seller;

            this.allProducts = new ObservableCollection<Product>();
            this.SellerProducts = new ObservableCollection<Product>();
            this.filteredProducts = new ObservableCollection<Product>();

            this.FollowCommand = new RelayCommand(this.ToggleFollow);

            // Initialize the AddToCartCommand
            // Update the RelayCommand instantiation to use the correct type
            this.AddToCartCommand = new RelayCommand(async (product) => await this.AddProductToCartAsync((Product)product));

            _ = this.LoadMyMarketProfileData(); // Load initial data
        }


        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Adds a product to the shopping cart.
        /// </summary>
        /// <param name="product">The product to add to the cart.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task AddProductToCartAsync(Product product)
        {
            if (product == null)
            {
                return;
            }

            try
            {
                // Use the current buyer ID instead of a hardcoded value
                var shoppingCartViewModel = new ShoppingCartViewModel(new ShoppingCartService(), this.buyer.Id);

                await shoppingCartViewModel.AddToCartAsync(product, 1);

                // Optionally show a confirmation dialog
                await this.ShowDialog("Success", $"Added {product.Name} to your cart!");
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine($"Error adding product to cart: {ex.Message}");
                await this.ShowDialog("Error", "There was a problem adding the product to your cart.");
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether the seller is followed by the buyer.
        /// </summary>
        public bool IsFollowing
        {
            get => this.isFollowing;
            set
            {
                this.isFollowing = value;
                this.OnPropertyChanged(nameof(this.IsFollowing));
                this.OnPropertyChanged(nameof(this.FollowButtonText));
                this.OnPropertyChanged(nameof(this.FollowButtonColor)); // Update button color when follow status changes
            }
        }

        /// <summary>
        /// Gets the text displayed on the follow/unfollow button.
        /// </summary>
        public string FollowButtonText => this.IsFollowing ? "Unfollow" : "Follow";

        /// <summary>
        /// Gets the color of the follow/unfollow button.
        /// </summary>
        public string FollowButtonColor => this.IsFollowing ? "Red" : "White";

        /// <summary>
        /// Gets the Command for toggling the follow/unfollow state.
        /// </summary>
        public ICommand FollowCommand { get; }

        /// <summary>
        /// Gets or sets the collection of filtered products for display in the UI.
        /// </summary>
        public ObservableCollection<Product> SellerProducts
        {
            get => this.filteredProducts;
            set
            {
                this.filteredProducts = value;
                this.OnPropertyChanged(nameof(this.SellerProducts));
            }
        }

        /// <summary>
        /// Gets or sets the list of notifications.
        /// </summary>
        public ObservableCollection<string> Notifications
        {
            get => this.notifications;
            set
            {
                this.notifications = value;
                this.OnPropertyChanged(nameof(this.Notifications));
            }
        }

        /// <summary>
        /// Gets or sets the display name of the seller.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the username of the seller.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of followers the seller has.
        /// </summary>
        public string FollowersCount { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the store name of the seller.
        /// </summary>
        public string StoreName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email of the seller.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the phone number of the seller.
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the address of the seller's store.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the trust score of the seller.
        /// </summary>
        public double TrustScore { get; set; }

        /// <summary>
        /// Gets or sets the description of the seller's store.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Filters products based on a search query.
        /// </summary>
        /// <param name="searchText">The search text to filter products.</param>
        public void FilterProducts(string searchText)
        {
            this.filteredProducts.Clear();

            if (string.IsNullOrEmpty(searchText))
            {
                foreach (var product in this.allProducts)
                {
                    this.filteredProducts.Add(product);
                }
            }
            else
            {
                var filteredProducts = this.allProducts.Where(p => p.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
                foreach (var product in filteredProducts)
                {
                    this.filteredProducts.Add(product);
                }
            }

            this.OnPropertyChanged(nameof(this.SellerProducts));
        }

        /// <summary>
        /// Raises the PropertyChanged event for a specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Loads the seller's profile data asynchronously.
        /// </summary>
        private async Task LoadMyMarketProfileData()
        {
            await this.LoadSellerProducts();
            await this.CheckFollowStatus();
            this.LoadSellerProfile();
        }

        /// <summary>
        /// Toggles the follow/unfollow state for the seller asynchronously.
        /// </summary>
        private async Task ToggleFollow()
        {
            if (!await this.buyerService.CheckIfBuyerExists(this.buyer.Id))
            {
                await this.ShowDialog("Not allowed", "You need to enter the information in the Buyer section!");
                return;
            }

            if (this.IsFollowing)
            {
                await this.buyerService.UnfollowSeller(this.buyer.Id, this.seller.Id); // Unfollow seller
            }
            else
            {
                await this.buyerService.FollowSeller(this.buyer.Id, this.seller.Id); // Follow seller
            }

            this.IsFollowing = !this.IsFollowing; // Update follow status
        }

        /// <summary>
        /// Shows a content dialog with the specified title and message.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="message">The message of the dialog.</param>
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

        /// <summary>
        /// Loads seller profile details and updates the UI asynchronously.
        /// </summary>
        private void LoadSellerProfile()
        {
            if (this.seller != null)
            {
                this.StoreName = this.seller.StoreName;
                this.Username = this.seller.Username;
                this.Email = this.seller.Email;
                this.PhoneNumber = this.seller.PhoneNumber;
                this.Address = this.seller.StoreAddress;
                this.FollowersCount = this.seller.FollowersCount.ToString();
                this.TrustScore = this.seller.TrustScore * 100.0 / 5.0;
                this.Description = this.seller.StoreDescription;

                // Notify the UI of property changes
                this.OnPropertyChanged(nameof(this.StoreName));
                this.OnPropertyChanged(nameof(this.Username));
                this.OnPropertyChanged(nameof(this.Email));
                this.OnPropertyChanged(nameof(this.PhoneNumber));
                this.OnPropertyChanged(nameof(this.Address));
                this.OnPropertyChanged(nameof(this.FollowersCount));
                this.OnPropertyChanged(nameof(this.TrustScore));
                this.OnPropertyChanged(nameof(this.Description));
            }
        }

        /// <summary>
        /// Loads the seller's products asynchronously.
        /// </summary>
        private async Task LoadSellerProducts()
        {
            if (this.seller != null)
            {
                var products = await this.buyerService.GetProductsForViewProfile(this.seller.Id);
                if (products != null)
                {
                    this.allProducts.Clear();
                    foreach (var product in products)
                    {
                        this.allProducts.Add(product);
                    }

                    this.FilterProducts(string.Empty); // Update filtered products
                }
            }
        }

        /// <summary>
        /// Checks if the buyer follows the seller asynchronously.
        /// </summary>
        private async Task CheckFollowStatus()
        {
            var isFollowing = await this.buyerService.IsFollowing(this.buyer.Id, this.seller.Id);
            this.IsFollowing = isFollowing;
            this.OnPropertyChanged(nameof(this.IsFollowing));
        }
    }
}
