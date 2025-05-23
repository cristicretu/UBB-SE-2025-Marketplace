using System.Diagnostics;
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
    public sealed partial class BuyProductView : Window
    {
        private readonly BuyProduct buyProduct;
        private readonly User currentUser;
        private readonly BasketViewModel basketViewModel = App.BasketViewModel;

        private Window? seeSellerReviewsView;
        private const int IMAGE_HEIGHT = 250;
        private const int TEXT_BLOCK_MARGIN = 4;
        private const int TEXT_BLOCK_PADDING_LEFT = 8;
        private const int TEXT_BLOCK_PADDING_TOP = 4;
        private const int TEXT_BLOCK_PADDING_RIGHT = 8;
        private const int TEXT_BLOCK_PADDING_BOTTOM = 4;
        private Window? leaveReviewWindow;

        public BuyProductView(BuyProduct product)
        {
            this.InitializeComponent();
            buyProduct = product;
            currentUser = MarketMinds.App.CurrentUser;
            LoadProductDetails();
            LoadImages();
        }
        private void LoadProductDetails()
        {
            // Basic Info
            TitleTextBlock.Text = buyProduct.Title;
            CategoryTextBlock.Text = buyProduct.Category.DisplayTitle;
            ConditionTextBlock.Text = buyProduct.Condition.DisplayTitle;
            PriceTextBlock.Text = $"{buyProduct.Price:C}";
            StockTextBlock.Text = buyProduct.Stock.ToString();

            // Seller Info
            SellerTextBlock.Text = buyProduct.Seller.Username;
            DescriptionTextBox.Text = buyProduct.Description;

            TagsItemsControl.ItemsSource = buyProduct.Tags.Select(tag =>
            {
                return new TextBlock
                {
                    Text = tag.DisplayTitle,
                    Margin = new Thickness(TEXT_BLOCK_MARGIN),
                    Padding = new Thickness(TEXT_BLOCK_PADDING_LEFT, TEXT_BLOCK_PADDING_TOP, TEXT_BLOCK_PADDING_RIGHT, TEXT_BLOCK_PADDING_BOTTOM),
                };
            }).ToList();
        }

        private void LoadImages()
        {
            ImageCarousel.Items.Clear();
            foreach (var image in buyProduct.Images)
            {
                var newImage = new Microsoft.UI.Xaml.Controls.Image
                {
                    Source = new BitmapImage(new Uri(image.Url)),
                    Stretch = Stretch.Uniform,
                    Height = IMAGE_HEIGHT,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center
                };

                ImageCarousel.Items.Add(newImage);
            }
        }

        private void OnAddToBasketClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                basketViewModel.AddToBasket(buyProduct.Id);

                // Show success notification
                BasketNotificationTip.Title = "Success";
                BasketNotificationTip.Subtitle = "Product added to basket successfully!";
                BasketNotificationTip.IconSource = new SymbolIconSource() { Symbol = Symbol.Accept };
                BasketNotificationTip.IsOpen = true;
            }
            catch (Exception basketAdditionException)
            {
                Debug.WriteLine($"Failed to add product to basket: {basketAdditionException.Message}");

                // Show error notification
                BasketNotificationTip.Title = "Error";
                BasketNotificationTip.Subtitle = $"Failed to add product: {basketAdditionException.Message}";
                BasketNotificationTip.IconSource = new SymbolIconSource() { Symbol = Symbol.Accept };
                BasketNotificationTip.IsOpen = true;
            }
        }

        private void OnSeeReviewsClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            if (App.SeeSellerReviewsViewModel != null)
            {
                App.SeeSellerReviewsViewModel.Seller = buyProduct.Seller;
                // Create a window to host the SeeSellerReviewsView page
                seeSellerReviewsView = new Window();
                seeSellerReviewsView.Content = new SeeSellerReviewsView(App.SeeSellerReviewsViewModel);
                seeSellerReviewsView.Activate();
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
                    App.ReviewCreateViewModel.Seller = buyProduct.Seller;

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

        private async void ShowErrorDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}