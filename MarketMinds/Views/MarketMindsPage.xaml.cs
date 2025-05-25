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

        // observable collections
        public ObservableCollection<BuyProduct> BuyProductsCollection { get; private set; }

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

        public MarketMindsPage()
        {
            this.InitializeComponent();
            this.BuyProductsViewModel = App.BuyProductsViewModel;
            this.BuyProductsCollection = new ObservableCollection<BuyProduct>();

            // Set Buy Products as the default selection
            ProductsPivot.SelectedIndex = 0;

            // Load data when page is initialized
            LoadDataAsync();
        }

        /// <summary>
        /// Loads data asynchronously
        /// </summary>
        private async void LoadDataAsync()
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
                    break;
                case 1:
                    // Load auction products if needed
                    break;
                case 2:
                    // Load borrow products if needed
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
            LoadDataAsync();
        }

        private void BuyProductCard_Click(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is BuyProduct product)
            {
                // Navigate to product details page
                // Frame.Navigate(typeof(ProductDetailsPage), product);
            }
        }
    }
}