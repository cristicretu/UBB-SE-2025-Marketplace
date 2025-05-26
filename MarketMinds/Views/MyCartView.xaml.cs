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
            // Initialize value converters
            Resources.Add("BoolToVisibilityConverter", new BoolToVisibilityConverter());
            Resources.Add("InverseBoolToVisibilityConverter", new InverseBoolToVisibilityConverter());
            Resources.Add("CurrencyConverter", new CurrencyConverter());

            this.InitializeComponent();

            // Get the current user ID and ensure it's valid
            int userId = UserSession.CurrentUserId ?? 10; // Fallback to user ID 10 for testing
            Debug.WriteLine($"Creating ShoppingCartViewModel with buyer ID: {userId}");

            this.ViewModel = App.ShoppingCartViewModel;
            this.DataContext = this.ViewModel;

            // Load cart items when the page is initialized
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("MyCartView loaded, loading cart items...");
                await this.ViewModel.LoadCartItemsAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading cart items: {ex.Message}");
                await ShowErrorDialog("Failed to load cart items", ex.Message);
            }
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
                var billingInfoWindow = new BillingInfoWindow();

                // Create a new BillingInfo page with the appropriate order history ID
                var billingInfoPage = new BillingInfo();

                // Pass the cart information to the BillingInfo page
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
                // Close this window and return to the marketplace
                if (Window.Current != null)
                {
                    Window.Current.Close();
                }
                // else if (this.XamlRoot?.Content is Window window)
                // {
                //    window.Close();
                // }
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

                // Ensure cart items are loaded
                _ = this.ViewModel.LoadCartItemsAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnNavigatedTo: {ex.Message}");
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

    // Value converters
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is bool boolValue && boolValue) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (value is Visibility visibility && visibility == Visibility.Visible);
        }
    }

    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is bool boolValue && !boolValue) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (value is Visibility visibility && visibility == Visibility.Collapsed);
        }
    }

    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double amount)
            {
                return $"{amount:0.00} €";
            }
            return "0.00 €";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
