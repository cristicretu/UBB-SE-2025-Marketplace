// <copyright file="BuyerProfileViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.ViewModel
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.Service;
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

        /// <inheritdoc/>
        public IBuyerWishlistItemDetailsProvider WishlistItemDetailsProvider { get; set; } = null!;

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

        /// <inheritdoc/>
        public async void SaveInfo()
        {
            try
            {
                await this.BuyerService.SaveInfo(this.Buyer!);
                await this.LoadBuyerProfile();
                await this.ShowDialog("Success", "Profile saved successfully");
            }
            catch (Exception ex)
            {
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
        public async Task AddPurchase(string purchaseAmount)
        {
            if (string.IsNullOrWhiteSpace(purchaseAmount))
            {
                return;
            }

            var decimalPurchaseAmount = 0m;
            try
            {
                decimalPurchaseAmount = decimal.Parse(purchaseAmount, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                Debug.WriteLine("Non decimal PurchaseAmount");
                return;
            }

            await this.BuyerService.UpdateAfterPurchase(this.Buyer!, decimalPurchaseAmount);
            this.AfterPurchase();
        }

        /// <inheritdoc/>
        public void AfterPurchase()
        {
            this.BuyerBadge?.Updated();
            this.OnPropertyChanged(nameof(this.BuyerBadge));
        }

        /// <inheritdoc/>
        public async Task LoadBuyerProfile()
        {
            this.Buyer = await this.BuyerService.GetBuyerByUser(this.User);

            this.BillingAddress = new BuyerAddressViewModel(this.Buyer.BillingAddress);
            this.ShippingAddress = new BuyerAddressViewModel(this.Buyer.ShippingAddress);
            this.FamilySync = new BuyerFamilySyncViewModel(this.BuyerService, this.Buyer, this);
            await this.FamilySync.LoadLinkages();
            this.Wishlist = new BuyerWishlistViewModel
            {
                BuyerService = this.BuyerService,
                Buyer = this.Buyer,
                ItemDetailsProvider = this.WishlistItemDetailsProvider,
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
                XamlRoot = App.MainWindow?.Content.XamlRoot,
            };

            await dialog.ShowAsync();
        }
    }
}