using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using MarketMinds.Shared.Models;
using MarketMinds.ViewModels;
using MarketMinds.Views;

namespace MarketMinds.Views.Pages
{
    public sealed partial class AuctionProductDetailsView : UserControl
    {
        public AuctionProductDetailsViewModel ViewModel { get; private set; }

        public AuctionProductDetailsView()
        {
            this.InitializeComponent();
            
            // Initialize ViewModel with the existing service from App
            if (App.AuctionProductsService != null)
            {
                ViewModel = new AuctionProductDetailsViewModel(App.AuctionProductsService);
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        public AuctionProductDetailsView(int productId) : this()
        {
            LoadProductAsync(productId);
        }

        private async void LoadProductAsync(int productId)
        {
            if (ViewModel != null)
            {
                await ViewModel.LoadProductAsync(productId);
                // Update UI after loading
                UpdateAllUI();
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.Product):
                case nameof(ViewModel.Images):
                    UpdateImageDisplay();
                    UpdatePrices();
                    UpdateMinimumBid();
                    break;
                case nameof(ViewModel.IsAuctionEnded):
                    UpdateStatusDisplay();
                    UpdateVisibilityStates();
                    break;
                case nameof(ViewModel.BidHistory):
                    UpdateBidHistoryVisibility();
                    break;
                case nameof(ViewModel.CanUserPlaceBids):
                case nameof(ViewModel.IsUserSeller):
                    UpdateVisibilityStates();
                    break;
            }
        }

        private void UpdateAllUI()
        {
            UpdateImageDisplay();
            UpdatePrices();
            UpdateMinimumBid();
            UpdateStatusDisplay();
            UpdateVisibilityStates();
            UpdateBidHistoryVisibility();
        }

        private void UpdateImageDisplay()
        {
            if (ViewModel?.Images != null && ViewModel.Images.Any())
            {
                var firstImage = ViewModel.Images.First();
                MainImage.Source = new BitmapImage(new Uri(firstImage.Url));
                
                NoImagePanel.Visibility = Visibility.Collapsed;
                ImageCounterBorder.Visibility = ViewModel.Images.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
                ThumbnailScroller.Visibility = ViewModel.Images.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
                
                ImageCounter.Text = $"1 / {ViewModel.Images.Count}";
            }
            else
            {
                MainImage.Source = null;
                NoImagePanel.Visibility = Visibility.Visible;
                ImageCounterBorder.Visibility = Visibility.Collapsed;
                ThumbnailScroller.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateStatusDisplay()
        {
            var isEnded = ViewModel?.IsAuctionEnded ?? false;
            
            // Update status border colors
            StatusBorder.Background = isEnded 
                ? new SolidColorBrush(Microsoft.UI.Colors.Red) { Opacity = 0.1 }
                : new SolidColorBrush(Microsoft.UI.Colors.Purple) { Opacity = 0.1 };
            
            // Update status icon and text colors
            var statusIcon = (FontIcon)((StackPanel)StatusBorder.Child).Children[0];
            var statusText = (TextBlock)((StackPanel)StatusBorder.Child).Children[1];
            
            var foregroundColor = isEnded 
                ? new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 220, 38, 38))
                : new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 147, 51, 234));
            
            statusIcon.Foreground = foregroundColor;
            statusText.Foreground = foregroundColor;
        }

        private void UpdatePrices()
        {
            if (ViewModel?.Product != null)
            {
                CurrentPriceText.Text = $"€{ViewModel.CurrentPrice:F2}";
                StartingPriceText.Text = $"Starting price: €{ViewModel.StartPrice:F2}";
            }
        }

        private void UpdateMinimumBid()
        {
            if (ViewModel?.Product != null)
            {
                MinimumBidText.Text = $"Minimum bid: €{ViewModel.MinimumBid:F2}";
            }
        }

        private void UpdateVisibilityStates()
        {
            var isEnded = ViewModel?.IsAuctionEnded ?? false;
            var canUserPlaceBids = ViewModel?.CanUserPlaceBids ?? false;
            var isUserSeller = ViewModel?.IsUserSeller ?? false;

            // Update bidding section visibility
            BiddingSection.Visibility = isEnded ? Visibility.Collapsed : Visibility.Visible;
            BidForm.Visibility = (canUserPlaceBids && !isUserSeller) ? Visibility.Visible : Visibility.Collapsed;
            SellerWarning.Visibility = isUserSeller ? Visibility.Visible : Visibility.Collapsed;
            
            // Update auction ended message
            AuctionEndedMessage.Visibility = isEnded ? Visibility.Visible : Visibility.Collapsed;
            
            // Update leave review button visibility
            LeaveReviewButton.Visibility = (canUserPlaceBids && ViewModel?.Product?.Seller?.Id > 0) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
            
            // Update tags section visibility
            TagsSection.Visibility = (ViewModel?.Tags != null && ViewModel.Tags.Any()) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        private void UpdateBidHistoryVisibility()
        {
            var hasBids = ViewModel?.BidHistory != null && ViewModel.BidHistory.Any();
            
            BidHistoryTable.Visibility = hasBids ? Visibility.Visible : Visibility.Collapsed;
            NoBidsState.Visibility = hasBids ? Visibility.Collapsed : Visibility.Visible;
        }

        #region Helper Methods for XAML Bindings

        public string GetMainImageSource(ObservableCollection<ProductImage> images)
        {
            return images?.FirstOrDefault()?.Url ?? string.Empty;
        }

        public Visibility HasNoImages(ObservableCollection<ProductImage> images)
        {
            return (images == null || !images.Any()) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility HasMultipleImages(ObservableCollection<ProductImage> images)
        {
            return (images != null && images.Count > 1) ? Visibility.Visible : Visibility.Collapsed;
        }

        public string GetImageCounterText(ObservableCollection<ProductImage> images)
        {
            if (images == null || !images.Any()) return "0 / 0";
            return $"1 / {images.Count}";
        }

        public SolidColorBrush GetStatusBackground(bool isAuctionEnded)
        {
            return isAuctionEnded 
                ? new SolidColorBrush(Microsoft.UI.Colors.Red) { Opacity = 0.1 }
                : new SolidColorBrush(Microsoft.UI.Colors.Purple) { Opacity = 0.1 };
        }

        public SolidColorBrush GetStatusForeground(bool isAuctionEnded)
        {
            return isAuctionEnded 
                ? new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 220, 38, 38)) // Red
                : new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 147, 51, 234)); // Purple
        }

        public SolidColorBrush GetTimeLeftColor(bool isAuctionEnded)
        {
            return isAuctionEnded 
                ? new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 220, 38, 38)) // Red
                : new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 217, 119, 6)); // Orange/Yellow
        }

        public string FormatPrice(double price)
        {
            return $"€{price:F2}";
        }

        public string FormatStartingPrice(double startPrice)
        {
            return $"Starting price: €{startPrice:F2}";
        }

        public string FormatMinimumBid(double minimumBid)
        {
            return $"{minimumBid:F2}";
        }

        public string FormatMinimumBidText(double minimumBid)
        {
            return $"Minimum bid: €{minimumBid:F2}";
        }

        public bool IsUserLoggedIn()
        {
            return App.CurrentUser == null;
        }

        public Visibility CanLeaveReview()
        {
            return (ViewModel?.CanUserPlaceBids == true && ViewModel?.Product?.Seller?.Id > 0) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        public Visibility HasTags(ObservableCollection<ProductTag> tags)
        {
            return (tags != null && tags.Any()) ? Visibility.Visible : Visibility.Collapsed;
        }

        public bool HasBidHistory(ObservableCollection<Bid> bidHistory)
        {
            return bidHistory != null && bidHistory.Any();
        }

        #endregion

        #region Event Handlers

        private void ThumbnailImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ProductImage image)
            {
                MainImage.Source = new BitmapImage(new Uri(image.Url));
                
                // Update image counter
                var index = ViewModel?.Images?.ToList().FindIndex(img => img.Id == image.Id) ?? 0;
                ImageCounter.Text = $"{index + 1} / {ViewModel?.Images?.Count ?? 0}";
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("HomeButton_Click: Starting navigation to home page");
                
                // Approach 1: Try to find Frame through visual tree traversal
                var frame = FindParent<Frame>(this);
                if (frame != null)
                {
                    System.Diagnostics.Debug.WriteLine("HomeButton_Click: Found frame via visual tree, navigating to MarketMindsPage");
                    frame.Navigate(typeof(MarketMindsPage), 0); // Navigate to Buy Products tab (index 0)
                    return;
                }
                
                // Approach 2: Try to get Frame from XamlRoot
                if (this.XamlRoot?.Content is FrameworkElement rootElement)
                {
                    var rootFrame = FindParent<Frame>(rootElement);
                    if (rootFrame != null)
                    {
                        System.Diagnostics.Debug.WriteLine("HomeButton_Click: Found frame via XamlRoot, navigating to MarketMindsPage");
                        rootFrame.Navigate(typeof(MarketMindsPage), 0); // Navigate to Buy Products tab (index 0)
                        return;
                    }
                }
                
                // Approach 3: Try HomePageWindow MainContentFrame (this is the most likely to work)
                if (App.HomePageWindow?.MainContentFrame != null)
                {
                    System.Diagnostics.Debug.WriteLine("HomeButton_Click: Using HomePageWindow.MainContentFrame");
                    App.HomePageWindow.MainContentFrame.Navigate(typeof(MarketMindsPage), 0); // Navigate to Buy Products tab (index 0)
                    return;
                }
                
                // Approach 4: Try MainWindow Frame (fallback)
                if (App.MainWindow?.Content is Frame mainFrame)
                {
                    System.Diagnostics.Debug.WriteLine("HomeButton_Click: Using MainWindow frame as fallback");
                    mainFrame.Navigate(typeof(MarketMindsPage), 0); // Navigate to Buy Products tab (index 0)
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine("HomeButton_Click: No navigation method worked");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HomeButton_Click: Error during navigation: {ex.Message}");
                
                // Final fallback - try HomePageWindow MainContentFrame
                try
                {
                    if (App.HomePageWindow?.MainContentFrame != null)
                    {
                        App.HomePageWindow.MainContentFrame.Navigate(typeof(MarketMindsPage), 0); // Navigate to Buy Products tab (index 0)
                    }
                }
                catch (Exception fallbackEx)
                {
                    System.Diagnostics.Debug.WriteLine($"HomeButton_Click: Final fallback also failed: {fallbackEx.Message}");
                }
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            if (parentObject is T parent)
                return parent;

            return FindParent<T>(parentObject);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Navigate back to auction products list
                if (this.XamlRoot?.Content is FrameworkElement element)
                {
                    // Find the Frame in the visual tree
                    var frame = FindParent<Frame>(element);
                    if (frame != null)
                    {
                        // Navigate to MarketMindsPage with Auction Products tab selected (index 1)
                        frame.Navigate(typeof(MarketMindsPage), 1);
                    }
                    else
                    {
                        // If no frame found, try to get the main window and navigate
                        if (App.MainWindow?.Content is Frame mainFrame)
                        {
                            mainFrame.Navigate(typeof(MarketMindsPage), 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Fallback: just navigate to marketplace
                if (App.MainWindow?.Content is Frame fallbackFrame)
                {
                    fallbackFrame.Navigate(typeof(MarketMindsPage), 1);
                }
            }
        }

        private void LeaveReviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.ReviewCreateViewModel != null)
            {
                var reviewWindow = new CreateReviewView(App.ReviewCreateViewModel);
                reviewWindow.Activate();
            }
        }

        private void BidAmountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Manually refresh the CanPlaceBid property when text changes
            ViewModel?.RefreshCanPlaceBid();
        }

        private void NewEndDatePicker_DateChanged(object sender, CalendarDatePickerDateChangedEventArgs e)
        {
            if (ViewModel != null && e.NewDate.HasValue)
            {
                ViewModel.NewEndDate = e.NewDate.Value.DateTime;
            }
        }

        #endregion
    }
} 