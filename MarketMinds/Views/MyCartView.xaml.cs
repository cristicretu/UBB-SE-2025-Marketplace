using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MarketPlace924.ViewModel;
using SharedClassLibrary.ProxyRepository;
using SharedClassLibrary.Service;

namespace MarketPlace924.View
{
    /// <summary>
    /// A page that displays the shopping cart.
    /// </summary>
    public sealed partial class MyCartView : Page
    {
        public ShoppingCartViewModel ViewModel { get; set; }

        public MyCartView()
        {
            this.InitializeComponent();
            this.ViewModel = new ShoppingCartViewModel(new ShoppingCartService(), buyerId: UserSession.CurrentUserId ?? 0);
            this.DataContext = this.ViewModel;

            // Load cart items when the page is initialized
            _ = this.ViewModel.LoadCartItemsAsync();
        }

        /// <summary>
        /// Handles the click event for the Purchase button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void PurchaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel == null || this.ViewModel.CartItems.Count == 0)
            {
                // Show a message that the cart is empty
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Empty Cart",
                    Content = "Your shopping cart is empty. Please add items before proceeding to checkout.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                _ = dialog.ShowAsync();
                return;
            }

            try
            {
                // Get the cart items for checkout
                var productsForCheckout = this.ViewModel.GetProductsForCheckout();
                double cartTotal = this.ViewModel.GetCartTotal();
                int buyerId = this.ViewModel.BuyerId;

                // Create an order history record (this might require additional code in your application)
                int orderHistoryId = Constants.OrderHistoryID; // Default value for now

                // Create a new instance of the BillingInfoWindow
                var billingInfoWindow = new BillingInfoWindow();

                // Create a new BillingInfo page with the appropriate order history ID
                var billingInfoPage = new BillingInfo(orderHistoryId);

                // Pass the cart information to the BillingInfo page
                // This assumes BillingInfo has methods to set these properties
                // You might need to add these methods to your BillingInfo class
                billingInfoPage.SetCartItems(productsForCheckout);
                billingInfoPage.SetCartTotal(cartTotal);
                billingInfoPage.SetBuyerId(buyerId);

                // Set the BillingInfo page as the content of the window
                billingInfoWindow.Content = billingInfoPage;

                // Activate (show) the window
                billingInfoWindow.Activate();
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"An error occurred while processing your purchase: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                _ = dialog.ShowAsync();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ShoppingCartViewModel viewModel)
            {
                this.ViewModel = viewModel;
                this.DataContext = this.ViewModel;
            }

            // Ensure cart items are loaded
            _ = this.ViewModel.LoadCartItemsAsync();
        }
    }
}
