using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Data;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MarketMinds.ViewModels;
using MarketMinds.Shared.Models;

namespace MarketMinds.Views
{
    public sealed partial class BillingInfo : Page
    {
        // Changed from concrete implementation to interface
        public IBillingInfoViewModel ViewModel { get; private set; }
        private bool initialLoadComplete = false;
        private bool dataWasLoaded = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="BillingInfo"/> class.
        /// </summary>
        public BillingInfo()
        {
            Debug.WriteLine("Initializing BillingInfo page");
            this.InitializeComponent();

            // Create the view model without a hardcoded order history ID
            // Still using App.BillingInfoViewModel but now casting to the interface
            this.ViewModel = App.BillingInfoViewModel;
            this.DataContext = this.ViewModel;

            Debug.WriteLine("BillingInfo page initialized");
        }

        /// <summary>
        /// Called when the page is navigated to.
        /// </summary>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!initialLoadComplete)
            {
                try
                {
                    Debug.WriteLine("BillingInfo page navigated to");

                    // Initialize the checkbox to unchecked by default
                    if (UseSavedInfoCheckBox != null)
                    {
                        UseSavedInfoCheckBox.IsChecked = false; // do not autofill on navigation, only if user checks the checkbox
                    }

                    // If we have cart data, skip loading from order history
                    if (dataWasLoaded)
                    {
                        Debug.WriteLine("Data was previously loaded, skipping initialization");
                    }
                    else if (ViewModel.ProductList == null || ViewModel.ProductList.Count == 0)
                    {
                        Debug.WriteLine("Loading data from order history");
                        await ViewModel.InitializeViewModelAsync();
                    }

                    initialLoadComplete = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in OnNavigatedTo: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Sets the cart items to be displayed and used for order creation.
        /// </summary>
        /// <param name="cartItems">The list of products from the cart.</param>
        public void SetCartItems(List<Product> cartItems)
        {
            if (cartItems != null && cartItems.Count > 0)
            {
                Debug.WriteLine($"Setting {cartItems.Count} cart items on BillingInfo page");
                ViewModel.SetCartItems(cartItems);
                dataWasLoaded = true;
            }
            else
            {
                Debug.WriteLine("No cart items provided to BillingInfo page");
            }
        }

        /// <summary>
        /// Sets the cart total to be used for the order.
        /// </summary>
        /// <param name="cartTotal">The total price of the cart.</param>
        public void SetCartTotal(double cartTotal)
        {
            if (cartTotal > 0)
            {
                Debug.WriteLine($"Setting cart total: ${cartTotal}");
                ViewModel.SetCartTotal(cartTotal);
                dataWasLoaded = true;
            }
            else
            {
                Debug.WriteLine("Invalid cart total provided");
            }
        }

        /// <summary>
        /// Sets the buyer ID for the order.
        /// </summary>
        /// <param name="buyerId">The buyer ID.</param>
        public void SetBuyerId(int buyerId)
        {
            if (buyerId > 0)
            {
                Debug.WriteLine($"Setting buyer ID: {buyerId}");
                ViewModel.SetBuyerId(buyerId);
            }
            else
            {
                Debug.WriteLine($"Invalid buyer ID provided: {buyerId}");
            }
        }

        /// <summary>
        /// Handler for the Finalize Purchase button click.
        /// </summary>
        private async void OnFinalizeButtonClickedAsync(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Finalize purchase button clicked");

            try
            {
                // Make sure the button is disabled during processing
                Button button = sender as Button;
                if (button != null)
                {
                    button.IsEnabled = false;
                }

                // Make a local copy of the total for debugging
                double totalBeforeFinalize = ViewModel.Total;
                Debug.WriteLine($"Total before finalizing purchase: ${totalBeforeFinalize}");

                // Call the view model to process the order
                await ViewModel.OnFinalizeButtonClickedAsync();
                Debug.WriteLine($"Purchase finalized successfully. Total: ${totalBeforeFinalize}");
                this.Frame.Navigate(typeof(FinalisePurchase));
                // Process notifications on finalisation success
                App.FinalizePurchaseViewModel.HandleFinish();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during purchase finalization: {ex.Message}");

                // Show error dialog
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"There was a problem processing your order: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await errorDialog.ShowAsync();
            }
            finally
            {
                // Re-enable the button
                Button button = sender as Button;
                if (button != null)
                {
                    button.IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// Handler for when the "Use saved information" checkbox is checked.
        /// </summary>
        private async void OnUseSavedInfoChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("Use saved information checkbox checked");
                // Populate all user information when checkbox is checked
                await ((BillingInfoViewModel)ViewModel).AutofillUserInformationAsync();
                Debug.WriteLine("Auto-filled all user information");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnUseSavedInfoChecked: {ex.Message}");
            }
        }

        /// <summary>
        /// Handler for when the "Use saved information" checkbox is unchecked.
        /// </summary>
        private async void OnUseSavedInfoUnchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("Use saved information checkbox unchecked");
                // Clear all fields except email when checkbox is unchecked
                await ((BillingInfoViewModel)ViewModel).ClearUserInformationExceptEmailAsync();
                Debug.WriteLine("Cleared user information except email");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnUseSavedInfoUnchecked: {ex.Message}");
            }
        }
    }
}
