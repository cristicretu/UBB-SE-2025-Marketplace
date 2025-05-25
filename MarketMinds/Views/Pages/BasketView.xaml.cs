using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MarketMinds.ViewModels;
using MarketMinds.Shared.Models;

namespace MarketMinds.Views.Pages
{
    public sealed partial class BasketView : Page
    {
        private BasketViewModel basketViewModel;
        private ObservableCollection<BasketItem> basketItems;
        private User currentUser;
        private const int ZERO_QUANTITY = 0;
        private const int ONE_QUANTITY = 1;

        public BasketView()
        {
            Debug.WriteLine("[View] BasketView constructor called");
            this.InitializeComponent();

            // Get the current user from the app
            currentUser = MarketMinds.App.CurrentUser;

            // Get the BasketViewModel from the app
            basketViewModel = MarketMinds.App.BasketViewModel;

            // Initialize basket items as ObservableCollection for auto-UI updates
            basketItems = new ObservableCollection<BasketItem>();

            // Set the ListView's data source
            BasketItemsListView.ItemsSource = basketItems;

            // Load basket data
            LoadBasketData();
        }

        private async void ShowErrorMessage(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = Content.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private void LoadBasketData()
        {
            Debug.WriteLine("[View] LoadBasketData called");

            try
            {
                // Refresh basket data from the view model
                basketViewModel.LoadBasket();

                // Clear the current basket items
                basketItems.Clear();

                // Add all items from the view model
                foreach (var item in basketViewModel.BasketItems)
                {
                    basketItems.Add(item);
                }

                // Update UI elements
                UpdateUIElements();
            }
            catch (Exception basketDataLoadException)
            {
                Debug.WriteLine($"[View] ERROR in LoadBasketData: {basketDataLoadException.GetType().Name} - {basketDataLoadException.Message}");
                if (basketDataLoadException.InnerException != null)
                {
                    Debug.WriteLine($"[View] Inner exception: {basketDataLoadException.InnerException.Message}");
                }

                // Show error message to user
                ErrorMessageTextBlock.Text = $"Error loading basket: {basketDataLoadException.Message}";
                ErrorMessageTextBlock.Visibility = Visibility.Visible;
            }
        }

        private void UpdateUIElements()
        {
            // Update item count
            int itemCount = basketItems.Sum(item => item.Quantity);
            ItemCountTextBlock.Text = $"{itemCount} item{(itemCount != ONE_QUANTITY ? "s" : string.Empty)} in your basket";

            // Update price displays
            SubtotalTextBlock.Text = $"${basketViewModel.Subtotal:F2}";
            DiscountTextBlock.Text = $"-${basketViewModel.Discount:F2}";
            TotalTextBlock.Text = $"${basketViewModel.TotalAmount:F2}";

            // Show empty basket message if there are no items
            if (basketItems.Count == ZERO_QUANTITY)
            {
                EmptyBasketMessage.Visibility = Visibility.Visible;
                BasketItemsListView.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmptyBasketMessage.Visibility = Visibility.Collapsed;
                BasketItemsListView.Visibility = Visibility.Visible;
            }

            // Enable/disable checkout button based on whether checkout is possible
            CheckoutButton.IsEnabled = basketViewModel.CanCheckout();

            // Show any error messages
            if (!string.IsNullOrEmpty(basketViewModel.ErrorMessage))
            {
                ErrorMessageTextBlock.Text = basketViewModel.ErrorMessage;
                ErrorMessageTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                ErrorMessageTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        private void HandleRemoveItemButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                Button button = sender as Button;
                if (button != null && button.CommandParameter is int itemId)
                {
                    // Find the corresponding basket item
                    var item = basketItems.FirstOrDefault(basketItem => basketItem.Id == itemId);
                    if (item != null)
                    {
                        // Remove using product ID instead of item ID
                        basketViewModel.RemoveProductFromBasket(item.Product.Id);
                    }
                    LoadBasketData();
                }
            }
            catch (Exception itemRemovalException)
            {
                ShowErrorMessage($"Failed to remove item: {itemRemovalException.Message}");
            }
        }

        private void HandleIncreaseQuantityButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                Button button = sender as Button;
                if (button != null && button.CommandParameter is int itemId)
                {
                    // Find the corresponding basket item
                    var basketItem = basketItems.FirstOrDefault(item => item.Id == itemId);
                    if (basketItem != null)
                    {
                        // Call the view model to handle the logic
                        basketViewModel.IncreaseProductQuantity(basketItem.Product.Id);
                        // Reload the basket data - this is already done in the view model's method,
                        // but kept here for consistency and to ensure UI updates
                        LoadBasketData();
                    }
                }
            }
            catch (Exception quantityIncreaseException)
            {
                ShowErrorMessage($"Failed to increase quantity: {quantityIncreaseException.Message}");
            }
        }

        private void HandleDecreaseQuantityButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                Button button = sender as Button;
                if (button != null && button.CommandParameter is int itemId)
                {
                    // Find the corresponding basket item
                    var basketItem = basketItems.FirstOrDefault(item => item.Id == itemId);
                    if (basketItem != null)
                    {
                        // Call the view model to handle the logic
                        basketViewModel.DecreaseProductQuantity(basketItem.Product.Id);
                        // Reload the basket data - this is already done in the view model's method,
                        // but kept here for consistency and to ensure UI updates
                        LoadBasketData();
                    }
                }
            }
            catch (Exception quantityDecreaseException)
            {
                ShowErrorMessage($"Failed to decrease quantity: {quantityDecreaseException.Message}");
            }
        }

        private void UpdateQuantityFromTextBox(TextBox textBox)
        {
            if (textBox != null && textBox.Tag is int itemId)
            {
                string errorMessage;
                bool success = basketViewModel.UpdateQuantityFromText(itemId, textBox.Text, out errorMessage);

                if (!success)
                {
                    // Show error message if needed
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        ShowErrorMessage(errorMessage);
                    }

                    // Reset the text box to current value if validation failed
                    var basketItem = basketViewModel.GetBasketItemById(itemId);
                    if (basketItem != null)
                    {
                        textBox.Text = basketItem.Quantity.ToString();
                    }
                }
                else
                {
                    // Reload the basket data
                    LoadBasketData();
                }
            }
        }

        private void QuantityTextBox_LostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            UpdateQuantityFromTextBox(sender as TextBox);
        }

        private void QuantityTextBox_KeyDown(object sender, KeyRoutedEventArgs routedEventArgs)
        {
            if (routedEventArgs.Key == Windows.System.VirtualKey.Enter)
            {
                UpdateQuantityFromTextBox(sender as TextBox);
                routedEventArgs.Handled = true;
            }
        }

        private void HandleApplyPromoCodeButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            string promoCode = PromoCodeTextBox.Text?.Trim();
            if (!string.IsNullOrEmpty(promoCode))
            {
                try
                {
                    basketViewModel.ApplyPromoCode(promoCode);

                    // The promo code was applied successfully if it gets here
                    if (basketViewModel.Discount > 0)
                    {
                        ErrorMessageTextBlock.Text = $"Promo code '{promoCode}' applied successfully!";
                        ErrorMessageTextBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green);
                        ErrorMessageTextBlock.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ErrorMessageTextBlock.Text = "Promo code applied, but no discount was awarded.";
                        ErrorMessageTextBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange);
                        ErrorMessageTextBlock.Visibility = Visibility.Visible;
                    }
                    LoadBasketData();
                }
                catch (Exception promoCodeApplicationException)
                {
                    ErrorMessageTextBlock.Text = $"Error applying promo code: {promoCodeApplicationException.Message}";
                    ErrorMessageTextBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
                    ErrorMessageTextBlock.Visibility = Visibility.Visible;
                }
            }
            else
            {
                ErrorMessageTextBlock.Text = "Please enter a promo code.";
                ErrorMessageTextBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
                ErrorMessageTextBlock.Visibility = Visibility.Visible;
            }
        }

        private void HandleClearBasketButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Clear Basket",
                Content = "Are you sure you want to remove all items from your basket?",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No",
                XamlRoot = Content.XamlRoot
            };

            _ = dialog.ShowAsync().AsTask().ContinueWith(task =>
            {
                if (task.Result == ContentDialogResult.Primary)
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        basketViewModel.ClearBasket();
                        LoadBasketData();
                    });
                }
            });
        }

        private async void HandleCheckoutButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (basketViewModel.CanCheckout())
            {
                // Show loading indicator
                CheckoutButton.IsEnabled = false;
                CheckoutButton.Content = "Processing...";

                Debug.WriteLine($"DIAGNOSTIC: BasketView - Starting checkout with PromoCode='{basketViewModel.PromoCode}', Discount=${basketViewModel.Discount:F2}, Total=${basketViewModel.TotalAmount:F2}");

                // Call async checkout method
                bool success = await basketViewModel.CheckoutAsync();

                // Reset button
                CheckoutButton.Content = "Proceed to Checkout";
                CheckoutButton.IsEnabled = true;

                if (success)
                {
                    // Refresh the UI to show empty basket
                    LoadBasketData();

                    // Show success message
                    await ShowCheckoutSuccessMessage();
                }
                else
                {
                    // Show error message
                    if (!string.IsNullOrEmpty(basketViewModel.ErrorMessage))
                    {
                        ErrorMessageTextBlock.Text = basketViewModel.ErrorMessage;
                        ErrorMessageTextBlock.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ErrorMessageTextBlock.Text = "Unable to complete checkout. Please try again later.";
                        ErrorMessageTextBlock.Visibility = Visibility.Visible;
                    }
                }
            }
            else
            {
                ErrorMessageTextBlock.Text = "Unable to proceed to checkout. Please check your basket items.";
                ErrorMessageTextBlock.Visibility = Visibility.Visible;
            }
        }

        private async Task ShowCheckoutSuccessMessage()
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Order Placed Successfully",
                Content = "Your order has been placed successfully. You can view your orders in the Account page.",
                CloseButtonText = "OK",
                XamlRoot = Content.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private void HandleContinueShoppingButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            // Navigate back or to product listing page
            // Since this is a Page, we use Frame navigation instead of Close()
            if (this.Frame != null)
            {
                this.Frame.GoBack();
            }
        }
    }
}