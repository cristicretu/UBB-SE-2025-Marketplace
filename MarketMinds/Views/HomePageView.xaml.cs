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
        public HomePageView()
        {
            this.InitializeComponent();

            // Navigate to the MarketMindsPage by default
            ContentFrame.Navigate(typeof(MarketMindsPage));

            // Set up window size and position
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(1700, 1000));
        }

        private void MarketMinds_Title_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to the MarketMindsPage when the MarketMinds title is clicked
            ContentFrame.Navigate(typeof(MarketMindsPage));
        }

        /// <summary>
        /// Navigate to the appropriate product listing page when a button is clicked
        /// </summary>
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is AppBarButton button)
            {
                switch (button.Label)
                {
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

        /// <summary>
        /// Handle tab selection changes to navigate to the appropriate product type page
        /// </summary>
        private void TabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TabView tabView && tabView.SelectedItem is TabViewItem selectedTab)
            {
                // Navigate to the appropriate product list view based on the selected tab
                switch (selectedTab.Tag.ToString())
                {
                    case "Buy":
                        // ContentFrame.Navigate(typeof(BuyProductListView));
                        break;
                    case "Auction":
                        // ContentFrame.Navigate(typeof(AuctionProductListView));
                        break;
                    case "Borrow":
                        // ContentFrame.Navigate(typeof(BorrowProductListView));
                        break;
                }
            }
        }
    }
}
