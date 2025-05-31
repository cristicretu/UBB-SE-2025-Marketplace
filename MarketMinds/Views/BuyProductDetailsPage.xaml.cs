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

namespace MarketMinds.Views
{
    public sealed partial class BuyProductDetailsPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // ViewModels
        public ShoppingCartViewModel ShoppingCartViewModel { get; set; }
        public BuyerWishlistItemViewModel BuyerWishlistItemViewModel { get; set; }

        // Product data
        private BuyProduct product;
        public BuyProduct Product
        {
            get => product;
            set => SetProperty(ref product, value);
        }

        // Image handling properties
        private string mainImageSource;
        public string MainImageSource
        {
            get => mainImageSource;
            set => SetProperty(ref mainImageSource, value);
        }

        private int currentImageIndex = 1;
        public int CurrentImageIndex
        {
            get => currentImageIndex;
            set => SetProperty(ref currentImageIndex, value);
        }

        public int TotalImages => Product?.Images?.Count ?? 0;
        public bool HasMultipleImages => TotalImages > 1;

        // Product information properties
        public string ProductDescription => !string.IsNullOrWhiteSpace(Product?.Description) 
            ? Product.Description 
            : "No description available for this product.";

        public string SellerName
        {
            get
            {
                if (Product?.Seller is User user)
                {
                    return user.Username ?? "Unknown seller";
                }
                return "Unknown seller";
            }
        }

        public int SellerId
        {
            get
            {
                if (Product?.Seller is User user)
                {
                    return user.Id;
                }
                return 0;
            }
        }

        // Stock-related properties
        public bool HasStock => Product?.Stock > 0;
        public bool IsLowStock => Product?.Stock > 0 && Product.Stock <= 5;
        public string LowStockMessage => $"Only {Product?.Stock} items left in stock!";
        public string StockText => Product?.Stock == 1 ? " item in stock" : " items in stock";
        public SolidColorBrush StockTextColor => IsLowStock 
            ? new SolidColorBrush(Microsoft.UI.Colors.Orange) 
            : new SolidColorBrush(Microsoft.UI.Colors.Gray);

        // Wishlist properties
        public string WishlistGlyph => IsInWishlist ? "\uE00B" : "\uE006"; // Filled heart vs outline heart
        public SolidColorBrush WishlistIconColor => IsInWishlist 
            ? new SolidColorBrush(Microsoft.UI.Colors.Red) 
            : new SolidColorBrush(Microsoft.UI.Colors.Gray);

        private bool isInWishlist;
        public bool IsInWishlist
        {
            get => isInWishlist;
            set
            {
                if (SetProperty(ref isInWishlist, value))
                {
                    OnPropertyChanged(nameof(WishlistGlyph));
                    OnPropertyChanged(nameof(WishlistIconColor));
                }
            }
        }

        // Tags property
        public bool HasTags => Product?.Tags?.Any() == true;

        // User role properties
        public bool IsCurrentUserBuyer => App.CurrentUser?.UserType == 2;
        public bool IsCurrentUserSeller => App.CurrentUser?.UserType == 3;
        public bool ShowLoginPrompt => !IsCurrentUserBuyer;

        public BuyProductDetailsPage()
        {
            this.InitializeComponent();

            // Get ViewModels from App
            this.ShoppingCartViewModel = App.ShoppingCartViewModel ?? throw new InvalidOperationException("ShoppingCartViewModel not initialized");
            this.BuyerWishlistItemViewModel = App.BuyerWishlistItemViewModel ?? throw new InvalidOperationException("BuyerWishlistItemViewModel not initialized");

            this.ShoppingCartViewModel.BuyerId = App.CurrentUser?.Id ?? 0;
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
            if (Product == null) return;

            // Initialize main image
            if (Product.Images?.Any() == true)
            {
                MainImageSource = Product.Images.First().Url;
                CurrentImageIndex = 1;
            }
            else
            {
                MainImageSource = "ms-appx:///Assets/Products/default-product.png";
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
            OnPropertyChanged(nameof(HasStock));
            OnPropertyChanged(nameof(IsLowStock));
            OnPropertyChanged(nameof(LowStockMessage));
            OnPropertyChanged(nameof(StockText));
            OnPropertyChanged(nameof(StockTextColor));
            OnPropertyChanged(nameof(HasTags));

            Debug.WriteLine($"Initialized product details for: {Product.Title}");
        }

        private void UpdateWishlistStatus()
        {
            if (Product != null && BuyerWishlistItemViewModel != null)
            {
                IsInWishlist = BuyerWishlistItemViewModel.IsInWishlist(Product.Id);
            }
        }

        private void ThumbnailImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string imageUrl)
            {
                MainImageSource = imageUrl;
                
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
        }

        private async void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            if (Product == null || !HasStock)
            {
                Debug.WriteLine("Cannot add to cart: Product is null or out of stock");
                return;
            }

            try
            {
                // Disable button during operation
                if (sender is Button button)
                {
                    button.IsEnabled = false;
                }

                await ShoppingCartViewModel.AddToCartAsync(Product, 1);
                Debug.WriteLine($"Added {Product.Title} to cart");

                // Show success message
                await ShowInfoDialog("Added to Cart", $"{Product.Title} has been added to your cart.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding to cart: {ex.Message}");
                await ShowErrorDialog("Error", $"Failed to add product to cart: {ex.Message}");
            }
            finally
            {
                // Re-enable button
                if (sender is Button button)
                {
                    button.IsEnabled = true;
                }
            }
        }

        private void WishlistButton_Click(object sender, RoutedEventArgs e)
        {
            if (Product == null || BuyerWishlistItemViewModel == null)
            {
                Debug.WriteLine("Cannot toggle wishlist: Product or ViewModel is null");
                return;
            }

            try
            {
                if (IsInWishlist)
                {
                    BuyerWishlistItemViewModel.RemoveFromWishlist(Product.Id);
                    Debug.WriteLine($"Removed {Product.Title} from wishlist");
                }
                else
                {
                    BuyerWishlistItemViewModel.AddToWishlist(Product.Id);
                    Debug.WriteLine($"Added {Product.Title} to wishlist");
                }

                // Update wishlist status
                UpdateWishlistStatus();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error toggling wishlist: {ex.Message}");
            }
        }

        private void LeaveReviewButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement review functionality
            // This would typically navigate to a review creation page or show a review dialog
            Debug.WriteLine($"Leave review clicked for seller: {SellerName}");
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to MarketMindsPage (home)
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

        private async System.Threading.Tasks.Task ShowInfoDialog(string title, string message)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing info dialog: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task ShowErrorDialog(string title, string message)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing error dialog: {ex.Message}");
            }
        }

        private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 