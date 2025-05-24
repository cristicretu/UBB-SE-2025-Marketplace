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

namespace MarketMinds.Views
{
    /// <summary>
    /// Main window for the MarketMinds application. Contains a header and a Frame for content navigation.
    /// </summary>
    public sealed partial class HomePageView : Window
    {
        private const int BUYER_TYPE = 2;
        private const int SELLER_TYPE = 3;
        
        public HomePageView()
        {
            this.InitializeComponent();

            // Navigate to the MarketMindsPage by default
            ContentFrame.Navigate(typeof(MarketMindsPage));

            // Set up window size and position
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(1700, 1000));
            
            // Configure UI based on user type
            ConfigureUIForUserType();
        }
        
        /// <summary>
        /// Configures the UI elements based on the current user type
        /// </summary>
        private void ConfigureUIForUserType()
        {
            // Get the current user type from the App
            int userType = App.CurrentUser.UserType;
            string username = App.CurrentUser.Username;
            
            bool isBuyer = userType == BUYER_TYPE;
            bool isSeller = userType == SELLER_TYPE;
            
            // Configure buyer-specific UI elements
            ChatBotButton.Visibility = isBuyer ? Visibility.Visible : Visibility.Collapsed;
            NotificationsButton.Visibility = isBuyer ? Visibility.Visible : Visibility.Collapsed;
            WishlistButton.Visibility = isBuyer ? Visibility.Visible : Visibility.Collapsed;
            CartButton.Visibility = isBuyer ? Visibility.Visible : Visibility.Collapsed;
            
            // Configure seller-specific UI elements
            CreateButton.Visibility = isSeller ? Visibility.Visible : Visibility.Collapsed;

            // Configure Profile button text
            string userRole = isBuyer ? "(buyer)" : (isSeller ? "(seller)" : "");
            ProfileButton.Label = $"{username} {userRole}";
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
                switch (button.Label)
                {
                    // Only buyers' cases
                    case "ChatBot":
                        // Show to chatbot page in frame
                        break;
                    case "Notifications":
                        // Show notifications page in frame
                        break;
                    case "Wishlist":
                        // Show wishlist page in frame
                        break;
                    case "Cart":
                        // Show cart page in frame
                        break;
                    // Both buyers and sellers' cases
                    case "Profile":
                        // Show profile page in frame
                        break;
                    // Only sellers' cases
                    case "Create":
                        // Show product creation page in frame
                        break;
                        
                }
            }
        }

        /// <summary>
        /// Handles the Create button click for sellers
        /// </summary>
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to product creation page for sellers
            // Example: ContentFrame.Navigate(typeof(ProductCreationView));
            
            // You should implement the appropriate navigation to your product creation page
            // If you have different types of products (Buy, Auction, Borrow), you might want to show
            // a dialog asking which type of product the seller wants to create
            
            // ContentFrame.Navigate(typeof(CreateProductView));
        }

        /// <summary>
        /// Handle profile menu item selections
        /// </summary>
        private void ProfileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem)
            {
                string tag = menuItem.Tag?.ToString();

                switch (tag)
                {
                    case "MyAccount":
                        // Navigate to user profile page
                        // Example: ContentFrame.Navigate(typeof(BuyerProfileView));
                        break;

                    case "MyOrders":
                        // Navigate to orders history page
                        // Example: ContentFrame.Navigate(typeof(OrderHistoryView));
                        break;

                    case "MyReviews":
                        // Navigate to user reviews page
                        // Could create a dedicated page for this
                        break;

                    case "Logout":
                        // Handle logout logic
                        // Could show a confirmation dialog first
                        // Then navigate to login page or reset app state
                        break;
                }
            }
        }
    }
}
