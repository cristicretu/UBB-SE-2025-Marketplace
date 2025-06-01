using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MarketMinds.Shared.Models;
using MarketMinds.ViewModels;
using MarketMinds.Shared.Services.BuyProductsService;
using MarketMinds.Shared.Helper;
using MarketMinds.Shared.ProxyRepository;

namespace MarketMinds.Views
{
    public sealed partial class BuyProductDetailsPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // ViewModels
        public ShoppingCartViewModel ShoppingCartViewModel { get; set; }
        public BuyerWishlistItemViewModel BuyerWishlistItemViewModel { get; set; }

        private BuyProduct product;
        public BuyProduct Product
        {
            get => product;
            set => SetProperty(ref product, value);
        }

        // Image handling properties
        private int currentImageIndex = 1;
        public int CurrentImageIndex
        {
            get => currentImageIndex;
            set => SetProperty(ref currentImageIndex, value);
        }

        // Wishlist properties
        private bool isInWishlist;
        public bool IsInWishlist
        {
            get => isInWishlist;
            set => SetProperty(ref isInWishlist, value);
        }

        public int TotalImages => Product?.Images?.Count() ?? 0;
        public bool HasMultipleImages => TotalImages > 1;
        public string ProductDescription => Product?.Description ?? "No description available.";
        public string SellerName => Product?.Seller?.Username ?? "Unknown Seller";
        public int SellerId => Product?.Seller?.Id ?? 0;
        public bool HasTags => Product?.Tags?.Any() == true;

        public bool HasStock => Product?.Stock > 0;
        public bool IsLowStock => Product?.Stock > 0 && Product?.Stock <= 5;
        public string StockText => Product?.Stock == 1 ? "item left" : "items left";
        public SolidColorBrush StockTextColor
        {
            get
            {
                try
                {
                    if (IsLowStock)
                    {
                        return (SolidColorBrush)Application.Current.Resources["SystemFillColorCriticalBrush"];
                    }
                    else
                    {
                        return (SolidColorBrush)Application.Current.Resources["SystemAccentColorBrush"];
                    }
                }
                catch
                {
                    return IsLowStock ?
                        new SolidColorBrush(Microsoft.UI.Colors.Orange) :
                        new SolidColorBrush(Microsoft.UI.Colors.Green);
                }
            }
        }
        public string LowStockMessage => $"Only {Product?.Stock} items remaining!";

        // User role properties
        public bool IsCurrentUserBuyer => App.CurrentUser?.UserType == 2;
        public bool IsCurrentUserSeller => App.CurrentUser?.UserType == 3;
        public bool ShowLoginPrompt => !IsCurrentUserBuyer;

        // Wishlist display properties
        public string WishlistGlyph => IsInWishlist ? "\uE006" : "\uE00A"; // Filled vs outline heart
        public SolidColorBrush WishlistIconColor => IsInWishlist ?
            new SolidColorBrush(Microsoft.UI.Colors.Red) :
            new SolidColorBrush(Microsoft.UI.Colors.Gray);

        public BuyProductDetailsPage()
        {
            this.InitializeComponent();

            // Initialize ViewModels
            ShoppingCartViewModel = new ShoppingCartViewModel();
            BuyerWishlistItemViewModel = new BuyerWishlistItemViewModel();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is BuyProduct buyProduct)
            {
                Product = buyProduct;
                InitializeProductData();
            }
            else
            {
                Debug.WriteLine("BuyProductDetailsPage: No product parameter provided");
                // Navigate back if no product is provided
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
            }
        }

        private void InitializeProductData()
        {
            if (Product == null)
            {
                return;
            }

            // Initialize image index
            if (Product.Images?.Any() == true)
            {
                CurrentImageIndex = 1;
            }
            else
            {
                CurrentImageIndex = 0;
            }

            // Check wishlist status
            UpdateWishlistStatus();

            // Notify all computed properties
            OnPropertyChanged(nameof(TotalImages));
            OnPropertyChanged(nameof(HasMultipleImages));
            OnPropertyChanged(nameof(ProductDescription));
            OnPropertyChanged(nameof(SellerName));
            OnPropertyChanged(nameof(SellerId));
            OnPropertyChanged(nameof(HasTags));
            OnPropertyChanged(nameof(HasStock));
            OnPropertyChanged(nameof(IsLowStock));
            OnPropertyChanged(nameof(StockText));
            OnPropertyChanged(nameof(StockTextColor));
            OnPropertyChanged(nameof(LowStockMessage));
            OnPropertyChanged(nameof(IsCurrentUserBuyer));
            OnPropertyChanged(nameof(IsCurrentUserSeller));
            OnPropertyChanged(nameof(ShowLoginPrompt));
        }

        private void UpdateWishlistStatus()
        {
            if (Product == null || !IsCurrentUserBuyer)
            {
                return;
            }

            try
            {
                // Check if product is in wishlist using the existing method
                IsInWishlist = BuyerWishlistItemViewModel.IsInWishlist(Product.Id);

                OnPropertyChanged(nameof(WishlistGlyph));
                OnPropertyChanged(nameof(WishlistIconColor));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking wishlist status: {ex.Message}");
            }
        }

        private void ThumbnailImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string imageUrl)
            {
                try
                {
                    // Update the main image source directly
                    MainImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(imageUrl));

                    // Update current image index
                    if (Product?.Images != null)
                    {
                        var index = Product.Images.ToList().FindIndex(img => img.Url == imageUrl);
                        if (index >= 0)
                        {
                            CurrentImageIndex = index + 1;
                        }
                    }

                    Debug.WriteLine($"Changed main image to: {imageUrl}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error changing main image: {ex.Message}");
                }
            }
        }

        private async void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            if (Product == null || !IsCurrentUserBuyer)
            {
                return;
            }

            try
            {
                await ShoppingCartViewModel.AddToCartAsync(Product, 1);

                // Show success message
                var dialog = new ContentDialog
                {
                    Title = "Added to Cart",
                    Content = $"{Product.Title} has been added to your cart.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding to cart: {ex.Message}");

                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "An error occurred while adding the item to your cart.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private async void UpdateStockButton_Click(object sender, RoutedEventArgs e)
        {
            if (Product == null || !IsCurrentUserSeller)
            {
                return;
            }

            // Read the new stock value from the NumberBox
            int newStock = (int)(StockNumberBox.Value);

            try
            {
                // Assume you have a BuyProductsService with an UpdateProductStockAsync method
                var service = new BuyProductsService(new BuyProductsProxyRepository(App.Configuration));
                await service.UpdateProductStockAsync(Product.Id, newStock);
                // If the backend call succeeded, update local Product.Stock and fire PropertyChanged
                Product.Stock = newStock;

                OnPropertyChanged(nameof(Product));
                OnPropertyChanged(nameof(HasStock));
                OnPropertyChanged(nameof(IsLowStock));
                OnPropertyChanged(nameof(StockText));
                OnPropertyChanged(nameof(StockTextColor));
                OnPropertyChanged(nameof(LowStockMessage));

                // Optional: show a small confirmation dialog
                var successDialog = new ContentDialog
                {
                    Title = "Stock Updated",
                    Content = $"Stock for '{Product.Title}' is now {newStock}.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await successDialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating stock: {ex.Message}");
                var errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "An error occurred while updating stock.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        private async void WishlistButton_Click(object sender, RoutedEventArgs e)
        {
            if (Product == null || !IsCurrentUserBuyer)
            {
                return;
            }

            try
            {
                string message;

                if (IsInWishlist)
                {
                    // Remove from wishlist
                    BuyerWishlistItemViewModel.RemoveFromWishlist(Product.Id);
                    IsInWishlist = false;
                    message = "Removed from wishlist";
                }
                else
                {
                    // Add to wishlist
                    BuyerWishlistItemViewModel.AddToWishlist(Product.Id);
                    IsInWishlist = true;
                    message = "Added to wishlist";
                }

                OnPropertyChanged(nameof(WishlistGlyph));
                OnPropertyChanged(nameof(WishlistIconColor));

                // Show feedback message
                var dialog = new ContentDialog
                {
                    Title = "Success",
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating wishlist: {ex.Message}");

                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "An error occurred while updating your wishlist.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private void LeaveReviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (Product?.Seller == null)
            {
                return;
            }

            // Navigate to review page or show review dialog
            // For now, just show a placeholder dialog
            var dialog = new ContentDialog
            {
                Title = "Leave Review",
                Content = $"Review functionality for seller '{SellerName}' will be implemented here.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            _ = dialog.ShowAsync();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to home/main page
            Frame.Navigate(typeof(MarketMindsPage));
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Go back to previous page
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
            else
            {
                // Fallback to MarketMindsPage if no back history
                Frame.Navigate(typeof(MarketMindsPage));
            }
        }

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}