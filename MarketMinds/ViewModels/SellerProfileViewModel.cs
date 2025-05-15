// -----------------------------------------------------------------------
// <copyright file="SellerProfileViewModel.cs" company="PlaceholderCompany">
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
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.Service;
    using Microsoft.Identity.Client;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Windows.Networking.NetworkOperators;

    /// <summary>
    /// View model for the seller profile page.
    /// </summary>
    public class SellerProfileViewModel : ISellerProfileViewModel
    {
        private const int maxNotifications = 6;
        private const int HighestInvalidSellerId = 0;
        private const double MultiplierForTrustScoreFromAverageReview = 100.0 / 5.0;

        // Private fields
        private readonly IUserService userService;
        private readonly ISellerService sellerService;
        private readonly User user;
        private Seller seller;
        private ObservableCollection<Product> allProducts;
        private ObservableCollection<string> notifications = new ObservableCollection<string>();
        private bool isExpanderExpanded = false;
        private bool isNotificationsLoaded = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="SellerProfileViewModel"/> class.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <param name="userService">The user service.</param>
        /// <param name="sellerService">The seller service.</param>
        public SellerProfileViewModel(User user, IUserService userService, ISellerService sellerService)
        {
            this.seller = new Seller(user);
            this.userService = userService;
            this.sellerService = sellerService;
            this.user = user;
            this.allProducts = new ObservableCollection<Product>();
            this.FilteredProducts = new ObservableCollection<Product>();
            this.LoadSellerData();
        }

        /// <summary>
        /// Event raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets the seller entity.
        /// </summary>
        public Seller Seller => this.seller;

        /// <summary>
        /// Gets or sets the filtered products collection.
        /// </summary>
        public ObservableCollection<Product> FilteredProducts { get; set; }

        /// <summary>
        /// Gets or sets the notifications collection.
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
        /// Gets or sets a value indicating whether the expander is expanded.
        /// </summary>
        public bool IsExpanderExpanded
        {
            get => this.isExpanderExpanded;
            set
            {
                if (this.isExpanderExpanded != value)
                {
                    this.isExpanderExpanded = value;
                    this.OnPropertyChanged();
                    if (this.isExpanderExpanded && !this.isNotificationsLoaded)
                    {
                        // Load notifications only once when expander is expanded.
                        _ = this.LoadNotifications();
                        this.isNotificationsLoaded = true;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the followers count.
        /// </summary>
        public string FollowersCount { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the store name.
        /// </summary>
        public string StoreName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the trust score.
        /// </summary>
        public double TrustScore { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the products collection.
        /// </summary>
        public ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>();

        /// <summary>
        /// Gets or sets the store name error message.
        /// </summary>
        public string StoreNameError { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email error message.
        /// </summary>
        public string EmailError { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the phone number error message.
        /// </summary>
        public string PhoneNumberError { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the address error message.
        /// </summary>
        public string AddressError { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description error message.
        /// </summary>
        public string DescriptionError { get; set; } = string.Empty;

        /// <summary>
        /// Filters products based on search text.
        /// </summary>
        /// <param name="searchText">The text to filter products by.</param>
        public void FilterProducts(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                this.FilteredProducts = new ObservableCollection<Product>(this.allProducts);
            }
            else
            {
                var filtered = this.allProducts.Where(product => product.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
                this.FilteredProducts = new ObservableCollection<Product>(filtered);
            }

            this.OnPropertyChanged(nameof(this.FilteredProducts));
        }

        /// <summary>
        /// Sorts products by price.
        /// </summary>
        public void SortProducts()
        {
            var sortedProducts = this.allProducts.OrderBy(product => product.Price).ToList();
            this.FilteredProducts = new ObservableCollection<Product>(sortedProducts);
            this.OnPropertyChanged(nameof(this.FilteredProducts));
        }

        /// <summary>
        /// Validates all fields and returns error messages.
        /// </summary>
        /// <returns>A list of error messages.</returns>
        public List<string> ValidateFields()
        {
            List<string> errorMessages = new List<string>();

            if (string.IsNullOrWhiteSpace(this.StoreName))
            {
                this.StoreNameError = "Store name is required.";
                errorMessages.Add(this.StoreNameError);
            }
            else
            {
                this.StoreNameError = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(this.Email) || !this.Email.Contains("@"))
            {
                this.EmailError = "Valid email is required.";
                errorMessages.Add(this.EmailError);
            }
            else
            {
                this.EmailError = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(this.PhoneNumber))
            {
                this.PhoneNumberError = "Phone number is required.";
                errorMessages.Add(this.PhoneNumberError);
            }
            else
            {
                this.PhoneNumberError = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(this.Address))
            {
                this.AddressError = "Address is required.";
                errorMessages.Add(this.AddressError);
            }
            else
            {
                this.AddressError = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(this.Description))
            {
                this.DescriptionError = "Description is required.";
                errorMessages.Add(this.DescriptionError);
            }
            else
            {
                this.DescriptionError = string.Empty;
            }

            this.OnPropertyChanged(nameof(this.StoreNameError));
            this.OnPropertyChanged(nameof(this.EmailError));
            this.OnPropertyChanged(nameof(this.PhoneNumberError));
            this.OnPropertyChanged(nameof(this.AddressError));
            this.OnPropertyChanged(nameof(this.DescriptionError));

            return errorMessages;
        }

        /// <summary>
        /// Loads notifications asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadNotifications()
        {
            await this.sellerService.GenerateFollowersChangedNotification(this.seller.Id, this.seller.FollowersCount);
            var notifications = await this.sellerService.GetNotifications(this.seller.Id, maxNotifications);
            this.notifications.Clear();
            foreach (var notification in notifications)
            {
                this.notifications.Add(notification);
            }

            this.OnPropertyChanged(nameof(this.Notifications));
        }

        /// <summary>
        /// Updates the seller profile asynchronously.
        /// </summary>
        public async void UpdateProfile()
        {
            if (this.seller != null)
            {
                this.seller.StoreName = this.StoreName;
                this.seller.StoreAddress = this.Address;
                this.seller.StoreDescription = this.Description;
                this.seller.User.Username = this.Username;
                this.seller.User.Email = this.Email;
                this.seller.User.PhoneNumber = this.PhoneNumber;

                if (this.seller.Id > HighestInvalidSellerId)
                {
                    await this.sellerService.UpdateSeller(this.seller);
                    await this.ShowDialog("Success", "Your seller has been updated successfully!");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Seller ID not found. Cannot update seller information in the database.");
                    await this.ShowDialog("Error", "Seller ID not found. Cannot update seller information in the database.");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("User property is null. Cannot update seller information.");
            }
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Loads seller data asynchronously.
        /// </summary>
        private async void LoadSellerData()
        {
            this.seller = await this.sellerService.GetSellerByUser(this.user);
            this.OnPropertyChanged(nameof(this.Seller));

            await this.LoadSellerProfile();
            await this.LoadSellerProducts();
        }

        /// <summary>
        /// Loads the seller profile asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task LoadSellerProfile()
        {
            if (this.seller != null)
            {
                if (this.seller.StoreName == null)
                {
                    this.StoreName = string.Empty;
                }
                else
                {
                    this.StoreName = this.seller.StoreName;
                }

                this.Username = this.seller.Username;
                this.Email = this.seller.Email;
                this.PhoneNumber = this.seller.PhoneNumber;
                this.Address = this.seller.StoreAddress;
                this.FollowersCount = this.seller.FollowersCount.ToString();
                this.TrustScore = await this.sellerService.CalculateAverageReviewScore(this.seller.Id) * MultiplierForTrustScoreFromAverageReview;
                this.Description = this.seller.StoreDescription;
                this.OnPropertyChanged(nameof(this.DisplayName));

                this.OnPropertyChanged(nameof(this.StoreName));
                this.OnPropertyChanged(nameof(this.Email));
                this.OnPropertyChanged(nameof(this.PhoneNumber));
                this.OnPropertyChanged(nameof(this.Address));
                this.OnPropertyChanged(nameof(this.Description));
                this.OnPropertyChanged(nameof(this.FollowersCount));
                this.OnPropertyChanged(nameof(this.TrustScore));
            }
        }

        /// <summary>
        /// Loads the seller products asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task LoadSellerProducts()
        {
            if (this.seller != null)
            {
                var products = await this.sellerService.GetAllProducts(this.seller.Id);
                if (products != null)
                {
                    this.allProducts.Clear();
                    foreach (var product in products)
                    {
                        this.allProducts.Add(product);
                    }

                    this.FilterProducts(string.Empty);
                }
            }
        }

        /// <summary>
        /// Shows a dialog with the specified title and message.
        /// </summary>
        /// <param name="title">The dialog title.</param>
        /// <param name="message">The dialog message.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowDialog(string title, string message)
        {
            if (App.MainWindow == null)
            {
                return;
            }

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
