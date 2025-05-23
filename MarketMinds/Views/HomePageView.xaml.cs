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
using MarketMinds.Views.Pages;

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

            // Navigate to the BuyProductListView by default
            // ContentFrame.Navigate(typeof(BuyProductListView));

            // Set up window size and position
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(1700, 1000));
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
                    case "Home":
                        // ContentFrame.Navigate(typeof(BuyProductListView));
                        break;
                    case "Search":
                        // Could show a search dialog or navigate to a search page
                        break;
                    case "Settings":
                        // Navigate to settings page if available
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
