using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using ViewModelLayer.ViewModel;
using MarketMinds.Shared.Models;
using Microsoft.UI.Xaml.Media.Imaging;
using MarketMinds.Shared.Services;
using MarketMinds.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace MarketMinds.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CompareProductsView : Page
    {
        public CompareProductsViewModel ViewModel;
        private Window parentWindow;
        private readonly IProductViewNavigationService navigationService;

        private const int RightImageHeight = 250; // magic numbers removal
        private const int LeftImageHeight = 200;

        public CompareProductsView(CompareProductsViewModel viewModel)
        {
            ViewModel = viewModel;
            this.InitializeComponent();
            navigationService = new ProductViewNavigationService();
            LoadImages();
        }

        public void OnSeeReviewsLeftProductClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            App.SeeSellerReviewsViewModel.Seller = ViewModel.LeftProduct.Seller;
            // Create a window to host the SeeSellerReviewsView page
            var window = new Window();
            window.Content = new SeeSellerReviewsView(App.SeeSellerReviewsViewModel);
            window.Activate();
        }

        public void OnSeeReviewsRightProductClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            App.SeeSellerReviewsViewModel.Seller = ViewModel.RightProduct.Seller;
            // Create a window to host the SeeSellerReviewsView page
            var window = new Window();
            window.Content = new SeeSellerReviewsView(App.SeeSellerReviewsViewModel);
            window.Activate();
        }

        private void LoadImages()
        {
            LeftImageCarousel.Items.Clear();
            RightImageCarousel.Items.Clear();

            foreach (var image in ViewModel.LeftProduct.Images)
            {
                Debug.WriteLine("Loading image: " + image.Url);

                var newImage = new Microsoft.UI.Xaml.Controls.Image
                {
                    Source = new BitmapImage(new Uri(image.Url)),
                    Stretch = Stretch.Uniform, // ✅ shows full image without cropping
                    Height = LeftImageHeight,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center
                };

                LeftImageCarousel.Items.Add(newImage);
            }

            foreach (var image in ViewModel.RightProduct.Images)
            {
                Debug.WriteLine("Loading image: " + image.Url);

                var newImage = new Microsoft.UI.Xaml.Controls.Image
                {
                    Source = new BitmapImage(new Uri(image.Url)),
                    Stretch = Stretch.Uniform, // ✅ shows full image without cropping
                    Height = RightImageHeight,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center
                };

                RightImageCarousel.Items.Add(newImage);
            }
        }

        private void OnSelectLeftProductClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            if (parentWindow == null)
            {
                return;
            }

            if (ViewModel.LeftProduct is AuctionProduct auctionProduct)
            {
                var detailView = new AuctionProductView(auctionProduct);
                detailView.Activate();
                parentWindow.Close();
            }
            else if (ViewModel.LeftProduct is BorrowProduct borrowProduct)
            {
                var detailView = new BorrowProductView(borrowProduct);
                detailView.Activate();
                parentWindow.Close();
            }
            else if (ViewModel.LeftProduct is BuyProduct buyProduct)
            {
                var detailView = new BuyProductView(buyProduct);
                detailView.Activate();
                parentWindow.Close();
            }
        }

        private void OnSelectRightProductClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            if (parentWindow == null)
            {
                return;
            }

            if (ViewModel.RightProduct is AuctionProduct auctionProduct)
            {
                var detailView = new AuctionProductView(auctionProduct);
                detailView.Activate();
                parentWindow.Close();
            }
            else if (ViewModel.RightProduct is BorrowProduct borrowProduct)
            {
                var detailView = new BorrowProductView(borrowProduct);
                detailView.Activate();
                parentWindow.Close();
            }
            else if (ViewModel.RightProduct is BuyProduct buyProduct)
            {
                var detailView = new BuyProductView(buyProduct);
                detailView.Activate();
                parentWindow.Close();
            }
        }
        // Method to set the parent window
        public void SetParentWindow(Window window)
        {
            parentWindow = window;
        }

        private void ViewProduct_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (sender is Button button && button.DataContext is Product product)
            {
                var detailView = navigationService.CreateProductDetailView(product);
                detailView.Activate();
            }
        }

        private void ViewSeller_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (sender is Button button && button.DataContext is Product product)
            {
                var sellerReviewsView = navigationService.CreateSellerReviewsView(product.Seller);
                sellerReviewsView.Activate();
            }
        }
    }
}
