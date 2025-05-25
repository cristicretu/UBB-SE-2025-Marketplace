using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MarketMinds.Shared.Models;
using MarketMinds.ViewModels;
using MarketMinds.Shared.Services.BorrowProductsService;
using ViewModelLayer.ViewModel;

namespace MarketMinds.Views
{
    [ExcludeFromCodeCoverage]
    public sealed partial class BorrowProductWindow : Window
    {
        /// <summary>
        /// The borrow product window view
        /// </summary>
        private readonly int currentProductId;
        private readonly IWaitListViewModel waitListViewModel;
        private readonly NotificationViewModel notificationVM;
        private readonly BorrowProductsViewModel productsVm;

        public BorrowProductWindow(int productId)
        {
            this.InitializeComponent();
            this.currentProductId = productId;
            this.productsVm = new BorrowProductsViewModel(new BorrowProductsService());
            this.waitListViewModel = new WaitListViewModel();
            this.notificationVM = new NotificationViewModel(this.GetCurrentUserId());
            this.Activated += this.Window_Activated;
        }

        /// <summary>
        /// Handles the window activated event, loads details for the product
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            await this.LoadProductDetails();
        }

        /// <summary>
        /// Loads the product details from the database
        /// </summary>
        /// <returns></returns>
        private async Task LoadProductDetails()
        {
            try
            {
                // ← use the borrow-products VM instead of the waitlist VM
                var product = await this.productsVm.GetBorrowProductByIdAsync(this.currentProductId);
                if (product != null)
                {
                    var sellerName = product.Seller?.Username ?? "Unknown Seller";
                    this.DisplayProduct(product, sellerName);

                    bool isOnWaitlist = await this.waitListViewModel
                                               .IsUserInWaitlist(this.GetCurrentUserId(), this.currentProductId);
                    this.UpdateWaitlistUI(isOnWaitlist);
                }
                else
                {
                    await this.ShowMessageAsync("Error", "Product not found");
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error", $"Failed to load product: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the UI based on whether the user is on the waitlist
        /// </summary>
        /// <param name="isOnWaitlist">State of the user that we want to check for</param>
        private void UpdateWaitlistUI(bool isOnWaitlist)
        {
            this.ButtonJoinWaitList.Visibility = isOnWaitlist ? Visibility.Collapsed : Visibility.Visible;
            this.waitlistActionsPanel.Visibility = isOnWaitlist ? Visibility.Visible : Visibility.Collapsed;
            this.txtPositionInQueue.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Displays the product details in the UI
        /// </summary>
        /// <param name="product">The product for which we want to display the details</param>
        /// <param name="sellerName">The name of the seller of that specific product</param>
        private void DisplayProduct(BorrowProduct product, string sellerName)
        {
            this.txtProductName.Text = product.Title;
            this.txtPrice.Text = $"Rate: ${product.DailyRate:F2}/day";
            this.txtSeller.Text = $"Seller: {sellerName}";
            this.txtType.Text = $"Category: {product.Category?.Name ?? "–"}";

            if (!product.IsBorrowed)
            {
                this.txtDates.Text = product.StartDate.HasValue
                    ? $"Available from: {product.StartDate:yyyy-MM-dd}"
                    : "Available: Now";

                this.ButtonBorrow.Visibility = Visibility.Visible;
                this.ButtonJoinWaitList.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.txtDates.Text = product.EndDate.HasValue
                    ? $"Due back: {product.EndDate:yyyy-MM-dd}"
                    : "Currently unavailable";

                this.ButtonBorrow.Visibility = Visibility.Collapsed;
                this.ButtonJoinWaitList.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Shows a message dialog with the specified title and message
        /// </summary>
        /// <param name="title">The title of the window</param>
        /// <param name="message">The message to be displayed in the window</param>
        /// <returns></returns>
        private async Task ShowMessageAsync(string title, string message)
        {
            try
            {
                if (this.Content.XamlRoot == null)
                {
                    throw new InvalidOperationException("XamlRoot is not initialized.");
                }

                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot,
                };

                await dialog.ShowAsync();
            }
            catch (Exception e)
            {
                // Handle the exception here
                Debug.WriteLine($"Failed to show dialog: {e.Message}");
            }
        }

        /// <summary>
        /// Handles the click event for the join wait list button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonJoinWaitList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int currentUserId = this.GetCurrentUserId();

                await this.waitListViewModel.AddUserToWaitlist(currentUserId, this.currentProductId);

                this.UpdateWaitlistUI(true);

                await this.ShowMessageAsync("Success", "You've joined the waitlist!");
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error", $"Failed to join waitlist: {ex.Message}");
            }
        }

        /// <summary>
        /// Returns the id of the user currently logged in
        /// </summary>
        /// <returns></returns>
        private int GetCurrentUserId()
        {
            return UserSession.CurrentUserId ?? 2;
        }

        /// <summary>
        /// Handles the click event for the leave wait list button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonLeaveWaitList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int currentUserId = this.GetCurrentUserId();

                await this.waitListViewModel.RemoveUserFromWaitlist(currentUserId, this.currentProductId);

                this.UpdateWaitlistUI(false);

                await this.ShowMessageAsync("Success", "You've left the waitlist");
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error", $"Failed to leave waitlist: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the click event for the view position button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonViewPosition_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int currentUserId = this.GetCurrentUserId();
                int position = await this.waitListViewModel.GetUserWaitlistPosition(currentUserId, this.currentProductId);

                if (position > 0)
                {
                    this.txtPositionInQueue.Text = $"Your position: #{position}";
                    this.txtPositionInQueue.Visibility = Visibility.Visible;
                }
                else
                {
                    await this.ShowMessageAsync("Position", "You are not currently on the waitlist");
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error", $"Failed to get waitlist position: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the click event for the notifications button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonNotifications_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create and show a simple notifications popup
                var notificationPopup = new ContentDialog
                {
                    Title = "Your Notifications",
                    Content = new ScrollViewer
                    {
                        Content = new StackPanel
                        {
                            Children =
                {
                    new TextBlock { Text = this.notificationVM.UnReadNotificationsCountText },
                },
                        },
                    },
                    CloseButtonText = "Close",
                    XamlRoot = this.Content.XamlRoot,
                };

                await notificationPopup.ShowAsync();
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error", $"Couldn't load notifications: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles clicking the “Borrow” button: calls the API to borrow the product.
        /// </summary>
        private async void ButtonBorrow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int userId = this.GetCurrentUserId();       // your logged-in user
                int productId = this.currentProductId;

                // Call your service / API to borrow:
                // assuming you have a WaitlistService or BorrowProductsService with a method:
                var borrowService = new BorrowProductsService();
                // If your API expects start/end dates as query params, send them too:
                DateTime start = DateTime.UtcNow;
                DateTime end = start.AddDays(7);

                // Fire the HTTP request to your new endpoint
                await borrowService.BorrowProductAsync(userId, productId, start, end);

                // Let the user know:
                await this.ShowMessageAsync("Success", "You have borrowed the product!");

                // Refresh the UI (hide button, show waitlist options, update dates)
                await this.LoadProductDetails();
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error", $"Failed to borrow: {ex.Message}");
            }
        }
    }
}