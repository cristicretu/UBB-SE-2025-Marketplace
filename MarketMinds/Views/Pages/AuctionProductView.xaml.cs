using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MarketMinds.Shared.Models;
using Microsoft.UI.Xaml.Media.Imaging;
using ViewModelLayer.ViewModel;
using MarketMinds.Views.Pages;

namespace MarketMinds
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AuctionProductView : Window
    {
        private readonly AuctionProduct product;
        private readonly User
            currentUser;
        private readonly AuctionProductsViewModel auctionProductsViewModel;

        private DispatcherTimer? countdownTimer;
        private Window? seeSellerReviewsView;
        private const int COUNTDOWN_TIMER_INTERVAL_IN_SECONDS = 1;
        private const int IMAGE_HEIGHT = 250;
        private const int TAG_MARGIN = 4;
        private const int TAG_PADDING_LEFT = 8;
        private const int TAG_PADDING_RIGHT = 8;
        private const int TAG_PADDING_TOP = 4;
        private const int TAG_PADDING_BOTTOM = 4;
        private Window? leaveReviewWindow;

        public AuctionProductView(AuctionProduct product)
        {
            this.InitializeComponent();
            this.product = product;
            currentUser = MarketMinds.App.CurrentUser;
            auctionProductsViewModel = MarketMinds.App.AuctionProductsViewModel;
            LoadProductDetails();
            LoadImages();
            LoadBidHistory();
            StartCountdownTimer();
        }

        private void StartCountdownTimer()
        {
            countdownTimer = new DispatcherTimer();
            countdownTimer.Interval = TimeSpan.FromSeconds(COUNTDOWN_TIMER_INTERVAL_IN_SECONDS);
            countdownTimer.Tick += CountdownTimer_Tick;
            countdownTimer.Start();
        }
        private void CountdownTimer_Tick(object? sender, object eventArgs)
        {
            string timeText = GetTimeLeft();
            TimeLeftTextBlock.Text = timeText;

            if (auctionProductsViewModel.IsAuctionEnded(product) && countdownTimer != null)
            {
                countdownTimer.Stop();
                auctionProductsViewModel.ConcludeAuction(product);
            }
        }
        private void LoadProductDetails()
        {
            // Basic Info
            TitleTextBlock.Text = product.Title;
            CategoryTextBlock.Text = product.Category.DisplayTitle;
            ConditionTextBlock.Text = product.Condition.DisplayTitle;
            StartingPriceTextBlock.Text = $"{product.StartingPrice:C}";
            CurrentPriceTextBlock.Text = $"{product.CurrentPrice:C}"; // Just an example
            TimeLeftTextBlock.Text = GetTimeLeft();

            // Seller Info
            SellerTextBlock.Text = product.Seller.Username;
            DescriptionTextBox.Text = product.Description;

            TagsItemsControl.ItemsSource = product.Tags.Select(tag =>
            {
                return new TextBlock
                {
                    Text = tag.DisplayTitle,
                    Margin = new Thickness(TAG_MARGIN),
                    Padding = new Thickness(TAG_PADDING_LEFT, TAG_PADDING_TOP, TAG_PADDING_RIGHT, TAG_PADDING_BOTTOM),
                };
            }).ToList();
        }

        private void LoadImages()
        {
            ImageCarousel.Items.Clear();
            foreach (var image in product.Images)
            {
                var img = new Microsoft.UI.Xaml.Controls.Image
                {
                    Source = new BitmapImage(new Uri(image.Url)),
                    Stretch = Stretch.Uniform,
                    Height = IMAGE_HEIGHT,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center
                };

                ImageCarousel.Items.Add(img);
            }
        }
        private void LoadBidHistory()
        {
            BidHistoryListView.ItemsSource = product.BidHistory
                .OrderByDescending(bid => bid.Timestamp)
                .ToList();
        }
        private string GetTimeLeft()
        {
            return auctionProductsViewModel.GetTimeLeft(product);
        }
        private void OnPlaceBidClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                auctionProductsViewModel.PlaceBid(product, currentUser, BidTextBox.Text);

                // Update UI after successful bid
                CurrentPriceTextBlock.Text = $"{product.CurrentPrice:C}";
                LoadBidHistory(); // Refresh bid list
            }
            catch (Exception bidClickedException)
            {
                ShowErrorDialog(bidClickedException.Message);
            }
        }

        private async void ShowErrorDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Bid Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private void OnSeeReviewsClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            if (App.SeeSellerReviewsViewModel != null)
            {
                App.SeeSellerReviewsViewModel.Seller = product.Seller;
                // Create a window to host the SeeSellerReviewsView page
                var window = new Window();
                window.Content = new SeeSellerReviewsView(App.SeeSellerReviewsViewModel);
                window.Activate();
                // Store reference to window
                seeSellerReviewsView = window;
            }
            else
            {
                ShowErrorDialog("Cannot view reviews at this time. Please try again later.");
            }
        }

        private void OnLeaveReviewClicked(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser != null)
            {
                if (App.ReviewCreateViewModel != null)
                {
                    App.ReviewCreateViewModel.Seller = product.Seller;

                    leaveReviewWindow = new CreateReviewView(App.ReviewCreateViewModel);
                    leaveReviewWindow.Activate();
                }
                else
                {
                    ShowErrorDialog("Cannot create review at this time. Please try again later.");
                }
            }
            else
            {
                ShowErrorDialog("You must be logged in to leave a review.");
            }
        }
    }
}