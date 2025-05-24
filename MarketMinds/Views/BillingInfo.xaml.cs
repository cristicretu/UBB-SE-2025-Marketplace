using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MarketMinds.ViewModels;
using MarketMinds.Shared.Models;

namespace MarketMinds.Views
{
    public sealed partial class BillingInfo : Page
    {
        public BillingInfoViewModel ViewModel { get; private set; }
        private bool initialLoadComplete = false;

        public BillingInfo(int orderHistoryId = 1)
        {
            this.InitializeComponent();
            this.ViewModel = new BillingInfoViewModel(orderHistoryId);
            this.DataContext = this.ViewModel;

            this.Loaded += BillingInfo_Loaded;
        }

        private async void BillingInfo_Loaded(object sender, RoutedEventArgs e)
        {
            if (!initialLoadComplete)
            {
                try
                {
                    // Only initialize from order history if we don't have cart data
                    if (ViewModel.ProductList == null || ViewModel.ProductList.Count == 0)
                    {
                        await ViewModel.InitializeViewModelAsync();
                    }

                    initialLoadComplete = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error initializing BillingInfo: {ex.Message}");
                }
            }
        }

        public void SetCartItems(List<Product> cartItems)
        {
            // Only set cart items if they aren't already set to prevent data loss
            if (!initialLoadComplete && (ViewModel.ProductList == null || ViewModel.ProductList.Count == 0))
            {
                ViewModel.SetCartItems(cartItems);
            }
        }

        public void SetCartTotal(double cartTotal)
        {
            // Only set cart total if it isn't already set to prevent data loss
            if (!initialLoadComplete && ViewModel.Total <= 0)
            {
                ViewModel.SetCartTotal(cartTotal);
                Debug.WriteLine($"Setting cart total to: {cartTotal}");
            }
        }

        public void SetBuyerId(int buyerId)
        {
            if (!initialLoadComplete)
            {
                ViewModel.SetBuyerId(buyerId);
            }
        }

        private async void OnFinalizeButtonClickedAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                // Make a local copy of the total for debugging
                double totalBeforeFinalize = ViewModel.Total;
                Debug.WriteLine($"Total before finalizing purchase: {totalBeforeFinalize}");

                await ViewModel.OnFinalizeButtonClickedAsync();

                Debug.WriteLine($"Purchase finalized, total was: {totalBeforeFinalize}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during purchase finalization: {ex.Message}");

                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "There was a problem processing your order. Please try again.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await errorDialog.ShowAsync();
            }
        }

        // Updated event handlers for the hidden date pickers
        private void OnStartDateChanged(DatePicker sender, DatePickerSelectedValueChangedEventArgs e)
        {
            if (sender != null && e.NewDate.HasValue)
            {
                ViewModel.UpdateStartDate(e.NewDate.Value);
            }
        }

        private void OnEndDateChanged(DatePicker sender, DatePickerSelectedValueChangedEventArgs e)
        {
            if (sender != null && e.NewDate.HasValue)
            {
                ViewModel.UpdateEndDate(e.NewDate.Value);
            }
        }
    }
}
