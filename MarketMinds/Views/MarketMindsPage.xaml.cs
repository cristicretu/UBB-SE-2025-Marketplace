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

        // view models with backing fields for debugging
        private BuyProductsViewModel _buyProductsViewModel;
        public BuyProductsViewModel BuyProductsViewModel 
        { 
            get => _buyProductsViewModel;
            set 
            {
                Debug.WriteLine($"BuyProductsViewModel setter called. Old: {_buyProductsViewModel?.GetType().Name ?? "NULL"}, New: {value?.GetType().Name ?? "NULL"}");
                Debug.WriteLine($"Setter called from: {Environment.StackTrace}");
                _buyProductsViewModel = value;
            }
        }
        
        public AuctionProductsViewModel AuctionProductsViewModel { get; set; }
        public BorrowProductsViewModel BorrowProductsViewModel { get; set; }
        public BuyerWishlistItemViewModel BuyerWishlistItemViewModel { get; set; }

        // observable collections
        public ObservableCollection<BuyProduct> BuyProductsCollection { get; private set; }
        public ObservableCollection<AuctionProduct> AuctionProductsCollection { get; private set; }
        public ObservableCollection<BorrowProduct> BorrowProductsCollection { get; private set; }

        // User role properties
        public bool IsCurrentUserBuyer => App.CurrentUser?.UserType == 2;
        public bool IsCurrentUserSeller => App.CurrentUser?.UserType == 3;

        // Pagination properties for Buy Products
        private int currentPageIndex = 0;
        private bool isInitializing = true; // Flag to prevent data loading during initialization
        
        public int CurrentPageIndex
        {
            get => currentPageIndex;
            set 
            {
                if (SetProperty(ref currentPageIndex, value) && !isInitializing)
                {
                    // Only load data if we're not in the initialization phase
                    LoadBuyDataAsync();
                }
            }
        }

        private int totalPages = 1;
        public int TotalPages
        {
            get => totalPages;
            private set => SetProperty(ref totalPages, value);
        }

        private int itemsPerPage = 10;
        public int ItemsPerPage
        {
            get => itemsPerPage;
            private set 
            {
                if (SetProperty(ref itemsPerPage, value) && !isInitializing)
                {
                    // Reset to first page and always trigger data loading when items per page changes
                    CurrentPageIndex = 0;
                    // If CurrentPageIndex was already 0, the setter won't trigger LoadBuyDataAsync, so call it explicitly
                    LoadBuyDataAsync();
                }
            }
        }

        private string pageInfoText = "";
        public string PageInfoText
        {
            get => pageInfoText;
            private set => SetProperty(ref pageInfoText, value);
        }

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
            
            // Get ViewModels from App with null checks to ensure they're initialized
            this.BuyProductsViewModel = App.BuyProductsViewModel ?? throw new InvalidOperationException("BuyProductsViewModel not initialized");
            this.AuctionProductsViewModel = App.AuctionProductsViewModel ?? throw new InvalidOperationException("AuctionProductsViewModel not initialized");
            this.BorrowProductsViewModel = App.BorrowProductsViewModel ?? throw new InvalidOperationException("BorrowProductsViewModel not initialized");
            this.BuyerWishlistItemViewModel = App.BuyerWishlistItemViewModel ?? throw new InvalidOperationException("BuyerWishlistItemViewModel not initialized");
            
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

                // Retry mechanism with limit (safer than recursion)
                // Alex: I ran with debug and found out that this function is called before the constructor is finished, so I added a delay to the retry, 100% success rate
                const int maxRetries = 5;
                int retryCount = 0;
                
                while (this.BuyProductsViewModel == null && retryCount < maxRetries)
                {
                    // Try to get it again
                    this.BuyProductsViewModel = App.BuyProductsViewModel;
                    
                    if (this.BuyProductsViewModel == null)
                    {
                        retryCount++;
                        await Task.Delay(50); // Short delay between retries
                    }
                    else
                    {
                        Debug.WriteLine($"Successfully got ViewModel on retry {retryCount + 1}");
                    }
                }
                
                if (this.BuyProductsViewModel == null)
                {
                    Debug.WriteLine("BuyProductsViewModel is still null after all retry attempts");
                    ShowEmptyState = true;
                    return;
                }

                // Calculate offset for current page
                int offset = CurrentPageIndex * ItemsPerPage;

                // Get total count first
                var totalCount = await Task.Run(() => this.BuyProductsViewModel.GetProductCount());
                
                // Calculate total pages
                TotalPages = Math.Max(1, (int)Math.Ceiling((double)totalCount / ItemsPerPage));
                
                // Ensure current page is valid
                if (CurrentPageIndex >= TotalPages)
                {
                    CurrentPageIndex = Math.Max(0, TotalPages - 1);
                    offset = CurrentPageIndex * ItemsPerPage;
                }

                // Get products for current page
                var products = await Task.Run(() => this.BuyProductsViewModel.GetProducts(offset, ItemsPerPage));

                // Update UI on the UI thread
                BuyProductsCollection.Clear();
                foreach (var product in products)
                {
                    BuyProductsCollection.Add(product);
                }

                // Update pagination info
                UpdatePaginationInfo(totalCount);

                // Update empty state and pagination controls visibility
                ShowEmptyState = BuyProductsCollection.Count == 0 && totalCount == 0;
                
                Debug.WriteLine($"BuyProductsCollection: {BuyProductsCollection.Count}, Total: {totalCount}, Page: {CurrentPageIndex + 1}/{TotalPages}");
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
                isInitializing = false; // Allow future page changes to trigger data loading
            }
        }

        /// <summary>
        /// Updates pagination information text
        /// </summary>
        private void UpdatePaginationInfo(int totalCount)
        {
            int startItem = totalCount > 0 ? (CurrentPageIndex * ItemsPerPage) + 1 : 0;
            int endItem = Math.Min((CurrentPageIndex + 1) * ItemsPerPage, totalCount);
            PageInfoText = $"Showing {startItem}-{endItem} of {totalCount} products";
        }

        /// <summary>
        /// Handles items per page selection changes
        /// </summary>
        private void ItemsPerPageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                if (int.TryParse(selectedItem.Tag.ToString(), out int newItemsPerPage))
                {
                    ItemsPerPage = newItemsPerPage; // This will reset CurrentPageIndex to 0 and trigger LoadBuyDataAsync()
                }
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
        private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
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
                        CurrentPageIndex = 0; // Reset to first page
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

            // Reset pagination to first page - this will trigger LoadBuyDataAsync()
            CurrentPageIndex = 0;

            // Reload data with cleared filters for other tabs
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
            var button = sender as Button;
            if (button == null)
            {
                return;
            }

            var product = button.DataContext as BuyProduct;
            if (product == null)
            {
                Debug.WriteLine("[BuyProductCard_AddToCart_Click] Product is null");
                return;
            }

            this.BuyerWishlistItemViewModel.AddToCartCommand.Execute(product);
        }

        private void BuyProductCard_AddToWishlist_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
            {
                return;
            }

            var product = button.DataContext as BuyProduct;
            if (product == null)
            {
                Debug.WriteLine("[BuyProductCard_AddToWishlist_Click] Product is null");
                return;
            }

            if (this.BuyerWishlistItemViewModel.IsInWishlist(product.Id))
            {
                this.BuyerWishlistItemViewModel.RemoveFromWishlist(product.Id);
            }
            else
            {
                this.BuyerWishlistItemViewModel.AddToWishlist(product.Id);
            }
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