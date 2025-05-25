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
using Microsoft.UI.Windowing;

namespace MarketMinds.Views
{
    /// <summary>
    /// Main window for the MarketMinds application. Contains a header and a Frame for content navigation.
    /// </summary>
    public sealed partial class HomePageView : Window
    {
        private const int BUYER_TYPE = 2;
        private const int SELLER_TYPE = 3;
        private bool isBuyer;
        private bool isSeller;
        private DispatcherTimer resizeTimer;

        public HomePageView()
        {
            this.InitializeComponent();

            // Initialize resize timer
            resizeTimer = new DispatcherTimer();
            resizeTimer.Interval = TimeSpan.FromMilliseconds(300); // Wait 300ms after resize stops
            resizeTimer.Tick += ResizeTimer_Tick;

            // Set initial window size to 2/3 of screen and handle minimum size constraints
            SetInitialWindowSize();
            this.SizeChanged += HomePageView_SizeChanged;

            // Navigate to the MarketMindsPage by default
            ContentFrame.Navigate(typeof(MarketMindsPage));

            // Configure UI based on user type
            ConfigureUIForUserType();
        }

        private void SetInitialWindowSize()
        {
            // Get the display area of the primary monitor
            var displayArea = DisplayArea.Primary;
            var workArea = displayArea.WorkArea;

            // Calculate 2/3 of screen size for initial window
            int initialWidth = (int)(workArea.Width * 2.0 / 3.0);
            int initialHeight = (int)(workArea.Height * 2.0 / 3.0);

            // Set the window size
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(initialWidth, initialHeight));

            // Center the window on screen
            int x = (workArea.Width - initialWidth) / 2;
            int y = (workArea.Height - initialHeight) / 2;
            this.AppWindow.Move(new Windows.Graphics.PointInt32(x, y));
        }

        private void HomePageView_SizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            // Reset the timer each time the window is resized
            // This ensures we only check minimum size after resizing stops
            resizeTimer.Stop();
            resizeTimer.Start();
        }

        private void ResizeTimer_Tick(object sender, object e)
        {
            // Stop the timer
            resizeTimer.Stop();

            // Enforce minimum window size (1/3 of screen)
            var displayArea = DisplayArea.Primary;
            var workArea = displayArea.WorkArea;

            int minWidth = (int)(workArea.Width / 3.0);
            int minHeight = (int)(workArea.Height / 3.0);

            var currentSize = this.AppWindow.Size;
            bool needsResize = false;
            int newWidth = currentSize.Width;
            int newHeight = currentSize.Height;

            if (currentSize.Width < minWidth)
            {
                newWidth = minWidth;
                needsResize = true;
            }

            if (currentSize.Height < minHeight)
            {
                newHeight = minHeight;
                needsResize = true;
            }

            if (needsResize)
            {
                this.AppWindow.Resize(new Windows.Graphics.SizeInt32(newWidth, newHeight));
            }
        }

        /// <summary>
        /// Configures the UI elements based on the current user type
        /// </summary>
        private void ConfigureUIForUserType()
        {
            // Get the current user type from the App
            int userType = App.CurrentUser.UserType;
            string username = App.CurrentUser.Username;

            isBuyer = userType == BUYER_TYPE;
            isSeller = userType == SELLER_TYPE;

            // Configure buyer-specific UI elements
            ChatSupportButton.Visibility = isBuyer ? Visibility.Visible : Visibility.Collapsed;
            NotificationsButton.Visibility = isBuyer ? Visibility.Visible : Visibility.Collapsed;
            WishlistButton.Visibility = isBuyer ? Visibility.Visible : Visibility.Collapsed;
            CartButton.Visibility = isBuyer ? Visibility.Visible : Visibility.Collapsed;

            // Configure seller-specific UI elements
            CreateListingButton.Visibility = isSeller ? Visibility.Visible : Visibility.Collapsed;

            // Configure Profile button text
            string userRole = isBuyer ? "(buyer)" : (isSeller ? "(seller)" : string.Empty);
            ProfileButton.Label = $"{username} {userRole}";
        }

        private void ConfigureBackend()
        {
            // some additional configs after we have the user session available
            if (isBuyer)
            {
                return;
            }
        }

        private void MarketMinds_Title_Click(object sender, RoutedEventArgs e)
        {
            if (ContentFrame.Content is MarketMindsPage)
            {
                return; // if the MarketMindsPage is already in the frame, do not navigate to it again
            }
            // Navigate to the MarketMindsPage when the MarketMinds title is clicked
            ContentFrame.Navigate(typeof(MarketMindsPage));
        }

        /// <summary>
        /// Navigate to the appropriate product listing page when a button is clicked
        /// </summary>
        private void AppNavBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is AppBarButton button)
            {
                switch (button.Tag)
                {
                    // Only buyers' cases
                    case "ChatSupport":
                        ContentFrame.Navigate(typeof(HelpPage));
                        break;
                    case "Notifications":
                        // Show notifications page in frame
                        break;
                    case "Wishlist":
                        // Show wishlist page in frame
                        break;
                    case "Cart":
                        ContentFrame.Navigate(typeof(MyCartView));
                        break;
                    case "MyAccount":
                        // This is handled in the ProfileMenuItem_Click method because it has a submenu of buttons
                        break;
                    // Only sellers' cases
                    case "CreateListing":
                        ContentFrame.Navigate(typeof(CreateListingView));
                        break;
                }
            }
        }

        /// <summary>
        /// Handle profile menu item selections
        /// </summary>
        private void ProfileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem)
            {
                switch (menuItem.Tag)
                {
                    case "MyAccount":
                        if (isBuyer)
                        {
                            // Show buyer profile page in frame
                            ContentFrame.Navigate(typeof(BuyerProfileView));
                        }
                        else if (isSeller)
                        {
                            // Show seller profile page in frame
                            ContentFrame.Navigate(typeof(SellerProfileView));
                        }
                        break;
                    case "OrderHistory":
                        // Navigate to orders history page
                        // Example: ContentFrame.Navigate(typeof(OrderHistoryView));
                        break;
                    case "MyReviews":
                        // Navigate to user reviews page
                        // Could create a dedicated page for this
                        break;
                    case "SignOut":
                        App.CloseHomePageWindow();
                        break;
                }
            }
        }
    }
}
