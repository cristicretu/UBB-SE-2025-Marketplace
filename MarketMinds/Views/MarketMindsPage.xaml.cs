using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MarketMinds.Shared.Models;
using MarketMinds.ViewModels;

namespace MarketMinds.Views
{
    public sealed partial class MarketMindsPage : Page, INotifyPropertyChanged
    {
        // Property changed event
        public event PropertyChangedEventHandler PropertyChanged;

        // view models
        public BuyProductsViewModel BuyProductsViewModel { get; set; }
        public AuctionProductsViewModel AuctionProductsViewModel { get; set; }
        public BorrowProductsViewModel BorrowProductsViewModel { get; set; }

        // observable collections
        public ObservableCollection<BuyProduct> BuyProductsCollection { get; private set; }
        public ObservableCollection<AuctionProduct> AuctionProductsCollection { get; private set; }
        public ObservableCollection<BorrowProduct> BorrowProductsCollection { get; private set; }

        // User role properties
        public bool IsCurrentUserBuyer => App.CurrentUser?.UserType == 2;
        public bool IsCurrentUserSeller => App.CurrentUser?.UserType == 3;

        // properties
        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            private set => SetProperty(ref isLoading, value);
        }

        private bool showEmptyState;
        public bool ShowEmptyState
        {
            get => showEmptyState;
            private set => SetProperty(ref showEmptyState, value);
        }

        private bool isAuctionLoading;
        public bool IsAuctionLoading
        {
            get => isAuctionLoading;
            private set => SetProperty(ref isAuctionLoading, value);
        }

        private bool showAuctionEmptyState;
        public bool ShowAuctionEmptyState
        {
            get => showAuctionEmptyState;
            private set => SetProperty(ref showAuctionEmptyState, value);
        }

        private bool isBorrowLoading;
        public bool IsBorrowLoading
        {
            get => isBorrowLoading;
            private set => SetProperty(ref isBorrowLoading, value);
        }

        private bool showBorrowEmptyState;
        public bool ShowBorrowEmptyState
        {
            get => showBorrowEmptyState;
            private set => SetProperty(ref showBorrowEmptyState, value);
        }

        public MarketMindsPage()
        {
            this.InitializeComponent();
            this.BuyProductsViewModel = App.BuyProductsViewModel;
            this.AuctionProductsViewModel = App.AuctionProductsViewModel;
            this.BorrowProductsViewModel = App.BorrowProductsViewModel;
            this.BuyProductsCollection = new ObservableCollection<BuyProduct>();
            this.AuctionProductsCollection = new ObservableCollection<AuctionProduct>();
            this.BorrowProductsCollection = new ObservableCollection<BorrowProduct>();

            // Set Buy Products as the default selection
            ProductsPivot.SelectedIndex = 0;

            // Load data when page is initialized
            LoadBuyDataAsync();
        }

        /// <summary>
        /// Loads data asynchronously
        /// </summary>
        private async void LoadBuyDataAsync()
        {
            try
            {
                IsLoading = true;

                // Get products
                var products = await Task.Run(() => this.BuyProductsViewModel.GetAllProducts());

                // Update UI on the UI thread
                BuyProductsCollection.Clear();
                foreach (var product in products)
                {
                    BuyProductsCollection.Add(product);
                }

                // Update empty state based on collection
                ShowEmptyState = BuyProductsCollection.Count == 0;
                Debug.WriteLine($"BuyProductsCollection: {BuyProductsCollection.Count}");
            }
            catch (Exception ex)
            {
                // TODO: Add proper error handling
                // Could show an error message to the user
                System.Diagnostics.Debug.WriteLine($"Error loading products: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Loads auction data asynchronously
        /// </summary>
        private async void LoadAuctionDataAsync()
        {
            try
            {
                IsAuctionLoading = true;

                // Get auction products
                var auctionProducts = await Task.Run(() => this.AuctionProductsViewModel.GetAllProducts());

                // Update UI on the UI thread
                AuctionProductsCollection.Clear();
                foreach (var product in auctionProducts)
                {
                    AuctionProductsCollection.Add(product);
                }

                // Update empty state based on collection
                ShowAuctionEmptyState = AuctionProductsCollection.Count == 0;
                Debug.WriteLine($"AuctionProductsCollection: {AuctionProductsCollection.Count}");
            }
            catch (Exception ex)
            {
                // TODO: Add proper error handling
                // Could show an error message to the user
                System.Diagnostics.Debug.WriteLine($"Error loading auction products: {ex.Message}");
            }
            finally
            {
                IsAuctionLoading = false;
            }
        }

        /// <summary>
        /// Loads borrow data asynchronously
        /// </summary>
        private async void LoadBorrowDataAsync()
        {
            try
            {
                IsBorrowLoading = true;

                // Get borrow products
                var borrowProducts = await Task.Run(() => this.BorrowProductsViewModel.GetAllProducts());

                // Update UI on the UI thread
                BorrowProductsCollection.Clear();
                foreach (var product in borrowProducts)
                {
                    BorrowProductsCollection.Add(product);
                }

                // Update empty state based on collection
                ShowBorrowEmptyState = BorrowProductsCollection.Count == 0;
                Debug.WriteLine($"BorrowProductsCollection: {BorrowProductsCollection.Count}");
            }
            catch (Exception ex)
            {
                // TODO: Add proper error handling
                // Could show an error message to the user
                System.Diagnostics.Debug.WriteLine($"Error loading borrow products: {ex.Message}");
            }
            finally
            {
                IsBorrowLoading = false;
            }
        }

        /// <summary>
        /// Helper method to set property and notify changes
        /// </summary>
        private void SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Handle pivot selection changes
        /// </summary>
        private void ProductsPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Update button styles based on pivot selection
            switch (ProductsPivot.SelectedIndex)
            {
                case 0:
                    // Load buy products if needed
                    if (BuyProductsCollection.Count == 0)
                    {
                        LoadBuyDataAsync();
                    }
                    break;
                case 1:
                    // Load auction products if needed
                    if (AuctionProductsCollection.Count == 0)
                    {
                        LoadAuctionDataAsync();
                    }
                    break;
                case 2:
                    // Load borrow products if needed
                    if (BorrowProductsCollection.Count == 0)
                    {
                        LoadBorrowDataAsync();
                    }
                    break;
            }
        }

        /// <summary>
        /// Handles the Clear Filters button click
        /// </summary>
        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            // Reset filter values here
            PriceRangeSlider.Value = PriceRangeSlider.Maximum;

            // Reload data with cleared filters
            LoadBuyDataAsync();
            LoadAuctionDataAsync();
            LoadBorrowDataAsync();
        }

        private void BuyProductCard_Click(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is BuyProduct product)
            {
                // Navigate to product details page
                // Frame.Navigate(typeof(ProductDetailsPage), product);
            }
        }

        private void BuyProductCard_AddToCart_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement add to cart functionality
        }

        private void AuctionProductCard_Click(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is AuctionProduct product)
            {
                // Navigate to auction product details page
                // Frame.Navigate(typeof(AuctionProductDetailsPage), product);
            }
        }

        private void BorrowProductCard_Click(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is BorrowProduct product)
            {
                // Navigate to borrow product details page
                // Frame.Navigate(typeof(BorrowProductDetailsPage), product);
            }
        }
    }
}