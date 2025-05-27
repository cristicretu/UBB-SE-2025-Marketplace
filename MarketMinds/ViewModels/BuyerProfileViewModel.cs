// <copyright file="BuyerProfileViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketMinds.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading.Tasks;
    using MarketMinds.Shared.Models;
    using MarketMinds.Shared.Services;
    using MarketMinds.Shared.Services.BuyProductsService;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// View model class for managing buyer profile data and operations.
    /// </summary>
    public class BuyerProfileViewModel : IBuyerProfileViewModel
    {
        private Address? previousAddress;

        /// <summary>
        /// Event that is raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc/>
        public IBuyerService BuyerService { get; set; } = null!;

        // Add this property to the BuyerProfileViewModel class
        public IBuyerLinkageService BuyerLinkageService { get; set; } = null!;

        /// <inheritdoc/>
        public IBuyerWishlistItemDetailsProvider WishlistItemDetailsProvider { get; set; } = null!;

        /// <summary>
        /// Gets or sets the product service instance.
        /// </summary>
        public IBuyProductsService ProductService { get; set; } = null!;

        /// <inheritdoc/>
        public User User { get; set; } = null!;

        /// <inheritdoc/>
        public Buyer? Buyer { get; private set; }

        /// <inheritdoc/>
        public IBuyerWishlistViewModel? Wishlist { get; set; }

        /// <inheritdoc/>
        public IBuyerFamilySyncViewModel? FamilySync { get; set; }

        /// <inheritdoc/>
        public IBuyerAddressViewModel? BillingAddress { get; set; }

        /// <inheritdoc/>
        public IBuyerAddressViewModel? ShippingAddress { get; set; }

        /// <inheritdoc/>
        public IBuyerBadgeViewModel? BuyerBadge { get; set; }

        /// <inheritdoc/>
        public bool ShippingAddressEnabled => !this.ShippingAddressDisabled;

        /// <inheritdoc/>
        public bool ShippingAddressDisabled
        {
            get => this.Buyer?.UseSameAddress ?? true;
            set
            {
                if (value)
                {
                    this.previousAddress = this.Buyer!.ShippingAddress;
                    this.Buyer.ShippingAddress = this.Buyer.BillingAddress;
                }
                else
                {
                    this.Buyer!.ShippingAddress = this.previousAddress ?? new Address();
                }

                this.ShippingAddress = new BuyerAddressViewModel(this.Buyer.ShippingAddress);
                this.Buyer.UseSameAddress = value;
                Debug.WriteLine($"Value of boolean tickbox is: {value}");
                this.OnPropertyChanged(nameof(this.ShippingAddressEnabled));
                this.OnPropertyChanged(nameof(this.ShippingAddressDisabled));
                this.OnPropertyChanged(nameof(this.ShippingAddress));
            }
        }

        public BuyerProfileViewModel(IBuyerService buyerService, IBuyProductsService productService, IBuyerLinkageService buyerLinkageService)
        {
            this.BuyerService = buyerService;
            this.ProductService = productService;
            this.BuyerLinkageService = buyerLinkageService;
            // the .User will be instantiated when the BuyerProfileView is loaded because only then we know for sure that we have a non null App.CurrentUser
        }

        /// <inheritdoc/>
        public async void SaveInfo()
        {
            try
            {
                // Sync the address data from ViewModels back to the Buyer object
                if (this.BillingAddress?.Address != null && this.Buyer?.BillingAddress != null)
                {
                    Debug.WriteLine($"Syncing Billing Address - ViewModel: Street='{this.BillingAddress.Address.StreetLine}', City='{this.BillingAddress.Address.City}'");
                    Debug.WriteLine($"Buyer.BillingAddress reference check: Same={object.ReferenceEquals(this.BillingAddress.Address, this.Buyer.BillingAddress)}");

                    this.Buyer.BillingAddress.StreetLine = this.BillingAddress.Address.StreetLine;
                    this.Buyer.BillingAddress.City = this.BillingAddress.Address.City;
                    this.Buyer.BillingAddress.Country = this.BillingAddress.Address.Country;
                    this.Buyer.BillingAddress.PostalCode = this.BillingAddress.Address.PostalCode;
                }

                if (!this.Buyer?.UseSameAddress == true && this.ShippingAddress?.Address != null && this.Buyer?.ShippingAddress != null)
                {
                    Debug.WriteLine($"Syncing Shipping Address - ViewModel: Street='{this.ShippingAddress.Address.StreetLine}', City='{this.ShippingAddress.Address.City}'");
                    Debug.WriteLine($"Buyer.ShippingAddress reference check: Same={object.ReferenceEquals(this.ShippingAddress.Address, this.Buyer.ShippingAddress)}");

                    this.Buyer.ShippingAddress.StreetLine = this.ShippingAddress.Address.StreetLine;
                    this.Buyer.ShippingAddress.City = this.ShippingAddress.Address.City;
                    this.Buyer.ShippingAddress.Country = this.ShippingAddress.Address.Country;
                    this.Buyer.ShippingAddress.PostalCode = this.ShippingAddress.Address.PostalCode;
                }

                // Debug: Log all buyer values before saving
                Debug.WriteLine("=== SAVING BUYER PROFILE ===");
                Debug.WriteLine($"Personal Info - FirstName: '{this.Buyer?.FirstName}', LastName: '{this.Buyer?.LastName}', Email: '{this.Buyer?.Email}', Phone: '{this.Buyer?.PhoneNumber}'");
                Debug.WriteLine($"Billing Address - Street: '{this.Buyer?.BillingAddress?.StreetLine}', City: '{this.Buyer?.BillingAddress?.City}', Country: '{this.Buyer?.BillingAddress?.Country}', PostalCode: '{this.Buyer?.BillingAddress?.PostalCode}'");
                if (!this.Buyer?.UseSameAddress == true)
                {
                    Debug.WriteLine($"Shipping Address - Street: '{this.Buyer?.ShippingAddress?.StreetLine}', City: '{this.Buyer?.ShippingAddress?.City}', Country: '{this.Buyer?.ShippingAddress?.Country}', PostalCode: '{this.Buyer?.ShippingAddress?.PostalCode}'");
                }

                await this.BuyerService.SaveInfo(this.Buyer!);
                // Don't reload the profile after saving to maintain UI binding
                // await this.LoadBuyerProfile();
                await this.ShowDialog("Success", "Profile saved successfully");
            }
            catch (ArgumentException ex)
            {
                // More specific error handling for validation errors
                Debug.WriteLine($"Validation error: {ex.Message}");
                await this.ShowDialog("Validation Error", $"Please check your information: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"General error: {ex.Message}");
                await this.ShowDialog("Error", ex.Message);
            }
        }

        /// <inheritdoc/>
        public async void ResetInfo()
        {
            await this.LoadBuyerProfile();
        }

        /// <inheritdoc/>
        public async Task OnBuyerLinkageUpdated()
        {
            await this.BuyerService.LoadBuyer(this.Buyer!, BuyerDataSegments.Linkages);
            this.Wishlist = this.Wishlist?.Copy();
            if (this.FamilySync != null)
            {
                await this.FamilySync.LoadLinkages();
            }

            this.OnPropertyChanged(nameof(this.Wishlist));
            this.OnPropertyChanged(nameof(this.FamilySync));
        }

        /// <inheritdoc/>
        public async Task LoadBuyerProfile()
        {
            this.Buyer = await this.BuyerService.GetBuyerByUser(this.User);
            Debug.WriteLine($"LoadBuyerProfile - Loaded Buyer: FirstName='{this.Buyer?.FirstName}', LastName='{this.Buyer?.LastName}', Email='{this.Buyer?.Email}', Phone='{this.Buyer?.PhoneNumber}'");

            // Ensure addresses are not null
            if (this.Buyer.BillingAddress == null)
            {
                this.Buyer.BillingAddress = new Address();
            }
            if (this.Buyer.ShippingAddress == null)
            {
                this.Buyer.ShippingAddress = new Address();
            }

            Debug.WriteLine($"LoadBuyerProfile - Creating BillingAddress ViewModel with Street: '{this.Buyer.BillingAddress?.StreetLine}'");
            this.BillingAddress = new BuyerAddressViewModel(this.Buyer.BillingAddress);
            Debug.WriteLine($"LoadBuyerProfile - BillingAddress ViewModel created, Address.Street: '{this.BillingAddress.Address?.StreetLine}'");
            Debug.WriteLine($"LoadBuyerProfile - Reference check: Same={object.ReferenceEquals(this.BillingAddress.Address, this.Buyer.BillingAddress)}");

            Debug.WriteLine($"LoadBuyerProfile - Creating ShippingAddress ViewModel with Street: '{this.Buyer.ShippingAddress?.StreetLine}'");
            this.ShippingAddress = new BuyerAddressViewModel(this.Buyer.ShippingAddress);
            Debug.WriteLine($"LoadBuyerProfile - ShippingAddress ViewModel created, Address.Street: '{this.ShippingAddress.Address?.StreetLine}'");
            this.FamilySync = new BuyerFamilySyncViewModel(this.BuyerService, this.Buyer, this, this.BuyerLinkageService);
            await this.FamilySync.LoadLinkages();
            this.Wishlist = new BuyerWishlistViewModel
            {
                BuyerService = this.BuyerService,
                Buyer = this.Buyer,
                ItemDetailsProvider = this.WishlistItemDetailsProvider,
                ProductService = this.ProductService
            };
            this.BuyerBadge = new BuyerBadgeViewModel(this.BuyerService) { Buyer = this.Buyer };
            this.OnPropertyChanged(nameof(this.Buyer));
            this.OnPropertyChanged(nameof(this.BillingAddress));
            this.OnPropertyChanged(nameof(this.ShippingAddress));
            this.OnPropertyChanged(nameof(this.ShippingAddressDisabled));
            this.OnPropertyChanged(nameof(this.ShippingAddressEnabled));
            this.OnPropertyChanged(nameof(this.Wishlist));
            this.OnPropertyChanged(nameof(this.FamilySync));
            this.OnPropertyChanged(nameof(this.BuyerBadge));
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
        /// Shows a dialog with the specified title and message.
        /// </summary>
        /// <param name="title">The dialog title.</param>
        /// <param name="message">The dialog message.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowDialog(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = App.HomePageWindow?.Content.XamlRoot,
            };

            await dialog.ShowAsync();
        }
    }
}