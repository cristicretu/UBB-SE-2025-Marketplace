using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;
using MarketMinds.ViewModels;
using MarketMinds.Shared.Services;

namespace MarketMinds.Views
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

            // Get the current user ID and ensure it's valid
            int userId = App.CurrentUser.Id;
            if (userId == null)
            {
                throw new Exception("User ID is null");
            }
            Debug.WriteLine($"Creating ShoppingCartViewModel with buyer ID: {userId}");

            this.ViewModel = App.ShoppingCartViewModel;
            this.ViewModel.BuyerId = userId; // set the buyer id for the shopping cart view model only here
            this.DataContext = this.ViewModel;
        }

        /// <summary>
        /// Handles the click event for the Checkout button.
        /// </summary>
        private async void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel == null || this.ViewModel.CartItems.Count == 0)
            {
                // Show a message that the cart is empty
                await ShowInfoDialog("Empty Cart", "Your shopping cart is empty. Please add items before proceeding to checkout.");
                return;
            }

            try
            {
                // Get the cart items for checkout
                var productsForCheckout = this.ViewModel.GetProductsForCheckout();
                double cartTotal = this.ViewModel.GetCartTotal();
                int buyerId = this.ViewModel.BuyerId;

                // Create a new instance of the BillingInfoWindow
                // var billingInfoWindow = new BillingInfoWindow();

                // Create a new BillingInfo page with the appropriate order history ID
                var billingInfoPage = new BillingInfo();

                // Pass the cart information to the BillingInfo page
                billingInfoPage.SetCartItems(productsForCheckout);
                billingInfoPage.SetCartTotal(cartTotal);
                billingInfoPage.SetBuyerId(buyerId);

                // Set the BillingInfo page as the content of the window
                // billingInfoWindow.Content = billingInfoPage;
                this.Frame.Navigate(typeof(BillingInfo));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during checkout: {ex.Message}");
                await ShowErrorDialog("Checkout Error", ex.Message);
            }
        }

        /// <summary>
        /// Handles the click event for the Continue Shopping button.
        /// </summary>
        private void ContinueShoppingButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Navigate back to the MarketMindsPage (same as MarketMinds_Title_Click)
                if (this.Frame != null)
                {
                    this.Frame.Navigate(typeof(MarketMindsPage));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating back to marketplace: {ex.Message}");
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                if (e.Parameter is ShoppingCartViewModel viewModel)
                {
                    Debug.WriteLine("Received ShoppingCartViewModel via navigation parameter");
                    this.ViewModel = viewModel;
                    this.DataContext = this.ViewModel;
                }

                // Always load cart items when navigating to this page (similar to pivot selection pattern)
                Debug.WriteLine("MyCartView navigated to, loading cart items...");
                _ = LoadCartItemsWithErrorHandlingAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnNavigatedTo: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads cart items with proper error handling
        /// </summary>
        private async Task LoadCartItemsWithErrorHandlingAsync()
        {
            try
            {
                await this.ViewModel.LoadCartItemsAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading cart items: {ex.Message}");
                await ShowErrorDialog("Failed to load cart items", ex.Message);
            }
        }

        private async Task ShowErrorDialog(string title, string message)
        {
            try
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing dialog: {ex.Message}");
            }
        }

        private async Task ShowInfoDialog(string title, string message)
        {
            try
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing dialog: {ex.Message}");
            }
        }
    }
}
