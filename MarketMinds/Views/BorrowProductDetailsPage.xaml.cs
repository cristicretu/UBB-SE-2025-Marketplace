using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.ProxyRepository;
using MarketMinds.Shared.Services.BorrowProductsService;

namespace MarketMinds.Views
{
    public sealed partial class BorrowProductDetailsPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private BorrowProduct product;
        public BorrowProduct Product
        {
            get => product;
            set
            {
                if (SetProperty(ref product, value))
                {
                    // Notify all dependent properties when Product changes
                    OnPropertyChanged(nameof(TotalImages));
                    OnPropertyChanged(nameof(HasMultipleImages));
                    OnPropertyChanged(nameof(ProductDescription));
                    OnPropertyChanged(nameof(SellerName));
                    OnPropertyChanged(nameof(SellerId));
                    OnPropertyChanged(nameof(HasTags));
                    OnPropertyChanged(nameof(IsAvailable));
                    OnPropertyChanged(nameof(IsAvailableNow));
                    OnPropertyChanged(nameof(IsFutureAvailable));
                    OnPropertyChanged(nameof(IsCurrentlyBorrowed));
                    OnPropertyChanged(nameof(AvailabilityText));
                    OnPropertyChanged(nameof(AvailabilityColor));
                    OnPropertyChanged(nameof(CategoryName));
                    OnPropertyChanged(nameof(ConditionName));
                    OnPropertyChanged(nameof(RemainingTime));
                    OnPropertyChanged(nameof(DaysUntilAvailable));
                    OnPropertyChanged(nameof(MinBorrowDate));
                    OnPropertyChanged(nameof(MaxBorrowDate));
                    OnPropertyChanged(nameof(MinWaitlistDate));
                    OnPropertyChanged(nameof(MaxWaitlistDate));
                }
            }
        }

        // Image handling properties
        private int currentImageIndex = 1;
        public int CurrentImageIndex
        {
            get => currentImageIndex;
            set => SetProperty(ref currentImageIndex, value);
        }

        // Computed properties
        public int TotalImages => Product?.Images?.Count() ?? 0;
        public bool HasMultipleImages => TotalImages > 1;
        public string ProductDescription => Product?.Description ?? "No description available.";
        
        private string sellerName;
        public string SellerName 
        { 
            get
            {
                if (Product?.Seller?.Username != null)
                {
                    Debug.WriteLine($"SellerName: Found seller username: {Product.Seller.Username}");
                    return Product.Seller.Username;
                }
                Debug.WriteLine($"SellerName: No seller found. Product: {Product != null}, Seller: {Product?.Seller != null}, Username: {Product?.Seller?.Username}");
                return "Unknown Seller";
            }
        }
        
        public int SellerId => Product?.Seller?.Id ?? 0;
        public bool HasTags => Product?.Tags?.Any() == true;

        // Availability properties
        public bool IsAvailable => Product?.IsBorrowed != true;
        public bool IsAvailableNow => IsAvailable && (Product?.StartDate == null || Product.StartDate.Value <= DateTime.Now);
        public bool IsFutureAvailable => IsAvailable && Product?.StartDate.HasValue == true && Product.StartDate.Value > DateTime.Now;
        public bool IsCurrentlyBorrowed => Product?.IsBorrowed == true;

        public string AvailabilityText
        {
            get
            {
                if (IsAvailableNow) return "Available Now";
                if (IsFutureAvailable) return $"Available {Product?.StartDate?.ToString("MMM dd")}";
                return "Borrowed";
            }
        }

        public SolidColorBrush AvailabilityColor
        {
            get
            {
                try
                {
                    if (IsAvailableNow)
                        return (SolidColorBrush)Application.Current.Resources["SystemFillColorSuccessBrush"];
                    if (IsFutureAvailable)
                        return (SolidColorBrush)Application.Current.Resources["SystemFillColorCautionBrush"];
                    return (SolidColorBrush)Application.Current.Resources["SystemFillColorCriticalBrush"];
                }
                catch
                {
                    // Fallback to theme-aware colors
                    if (IsAvailableNow) 
                        return (SolidColorBrush)Application.Current.Resources["SystemAccentColor"];
                    if (IsFutureAvailable) 
                        return (SolidColorBrush)Application.Current.Resources["SystemFillColorAttentionBrush"];
                    return (SolidColorBrush)Application.Current.Resources["SystemFillColorCriticalBrush"];
                }
            }
        }

        public string CategoryName => Product?.Category?.Name ?? "Uncategorized";
        public string ConditionName => Product?.Condition?.Name ?? "Unknown";

        public string RemainingTime
        {
            get
            {
                if (Product?.EndDate.HasValue == true)
                {
                    var timeLeft = Product.EndDate.Value - DateTime.Now;
                    if (timeLeft <= TimeSpan.Zero)
                        return "Borrowing period ended";
                    return $"{timeLeft.Days}d {timeLeft.Hours}h {timeLeft.Minutes}m";
                }
                return "No end date specified";
            }
        }

        public int DaysUntilAvailable
        {
            get
            {
                if (Product?.StartDate.HasValue == true && Product.StartDate.Value > DateTime.Now)
                {
                    return (Product.StartDate.Value - DateTime.Now).Days;
                }
                return 0;
            }
        }

        // User role properties
        public bool IsCurrentUserBuyer => App.CurrentUser?.UserType == 2;
        public bool IsCurrentUserSeller => App.CurrentUser?.UserType == 3;
        public bool IsProductOwner => Product?.SellerId == App.CurrentUser?.Id;
        public bool CanInteractWithProduct => IsCurrentUserBuyer && !IsProductOwner;
        public bool ShowLoginPrompt => !IsCurrentUserBuyer;
        public bool CanLeaveReview => IsCurrentUserBuyer && SellerId > 0 && !IsProductOwner;

        // Borrowing properties
        public bool IsCurrentBorrower => Product?.BorrowerId == App.CurrentUser?.Id;
        public bool ShowWaitlistSection => CanInteractWithProduct && (IsCurrentlyBorrowed || IsFutureAvailable);
        public bool ShowBorrowingSection => CanInteractWithProduct;

        // Date properties for borrowing
        public DateTimeOffset MinBorrowDate => DateTime.Now.AddDays(1);
        public DateTimeOffset MaxBorrowDate => Product?.TimeLimit ?? DateTime.Now.AddDays(30);
        public DateTimeOffset MinWaitlistDate
        {
            get
            {
                if (IsCurrentlyBorrowed && Product?.EndDate.HasValue == true)
                    return Product.EndDate.Value.AddDays(1);
                if (IsFutureAvailable && Product?.StartDate.HasValue == true)
                    return Product.StartDate.Value.AddDays(1);
                return DateTime.Now.AddDays(1);
            }
        }
        public DateTimeOffset MaxWaitlistDate => Product?.TimeLimit ?? DateTime.Now.AddDays(30);

        public BorrowProductDetailsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            if (e.Parameter is BorrowProduct borrowProduct)
            {
                Debug.WriteLine($"BorrowProductDetailsPage: Received product: {borrowProduct?.Title}");
                Debug.WriteLine($"BorrowProductDetailsPage: Product ID: {borrowProduct?.Id}");
                Debug.WriteLine($"BorrowProductDetailsPage: Seller ID: {borrowProduct?.SellerId}");
                Debug.WriteLine($"BorrowProductDetailsPage: Seller object: {borrowProduct?.Seller != null}");
                if (borrowProduct?.Seller != null)
                {
                    Debug.WriteLine($"BorrowProductDetailsPage: Seller Username: {borrowProduct.Seller.Username}");
                }
                
                Product = borrowProduct;
                InitializeProductData();
                
                // Load complete product data if seller information is missing
                LoadCompleteProductData();
            }
            else
            {
                Debug.WriteLine("BorrowProductDetailsPage: No product parameter provided");
                // Navigate back if no product is provided
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
            }
        }

        private async void LoadCompleteProductData()
        {
            if (Product == null) return;

            try
            {
                // If seller information is missing, try to load the complete product
                if (Product.Seller == null && Product.SellerId > 0)
                {
                    Debug.WriteLine($"Seller information missing, loading complete product data for ID: {Product.Id}");
                    
                    var service = new BorrowProductsService();
                    var completeProduct = await service.GetBorrowProductByIdAsync(Product.Id);
                    
                    if (completeProduct != null)
                    {
                        Debug.WriteLine($"Loaded complete product with seller: {completeProduct.Seller?.Username}");
                        Product = completeProduct;
                    }
                    else
                    {
                        Debug.WriteLine("Failed to load complete product data");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading complete product data: {ex.Message}");
            }
        }

        private void InitializeProductData()
        {
            if (Product == null) return;

            // Initialize image index
            if (Product.Images?.Any() == true)
            {
                CurrentImageIndex = 1;
            }
            else
            {
                CurrentImageIndex = 0;
            }

            // Notify all computed properties
            OnPropertyChanged(nameof(TotalImages));
            OnPropertyChanged(nameof(HasMultipleImages));
            OnPropertyChanged(nameof(ProductDescription));
            OnPropertyChanged(nameof(SellerName));
            OnPropertyChanged(nameof(SellerId));
            OnPropertyChanged(nameof(HasTags));
            OnPropertyChanged(nameof(IsAvailable));
            OnPropertyChanged(nameof(IsAvailableNow));
            OnPropertyChanged(nameof(IsFutureAvailable));
            OnPropertyChanged(nameof(IsCurrentlyBorrowed));
            OnPropertyChanged(nameof(AvailabilityText));
            OnPropertyChanged(nameof(AvailabilityColor));
            OnPropertyChanged(nameof(CategoryName));
            OnPropertyChanged(nameof(ConditionName));
            OnPropertyChanged(nameof(RemainingTime));
            OnPropertyChanged(nameof(DaysUntilAvailable));
            OnPropertyChanged(nameof(IsCurrentUserBuyer));
            OnPropertyChanged(nameof(IsCurrentUserSeller));
            OnPropertyChanged(nameof(IsProductOwner));
            OnPropertyChanged(nameof(CanInteractWithProduct));
            OnPropertyChanged(nameof(ShowLoginPrompt));
            OnPropertyChanged(nameof(CanLeaveReview));
            OnPropertyChanged(nameof(IsCurrentBorrower));
            OnPropertyChanged(nameof(ShowWaitlistSection));
            OnPropertyChanged(nameof(ShowBorrowingSection));
            OnPropertyChanged(nameof(MinBorrowDate));
            OnPropertyChanged(nameof(MaxBorrowDate));
            OnPropertyChanged(nameof(MinWaitlistDate));
            OnPropertyChanged(nameof(MaxWaitlistDate));
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

        private async void BorrowButton_Click(object sender, RoutedEventArgs e)
        {
            if (Product == null || !CanInteractWithProduct) return;

            var endDate = BorrowEndDatePicker.Date;
            if (endDate == null)
            {
                BorrowResult.Text = "Please select an end date first";
                BorrowResult.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
                return;
            }

            try
            {
                // Disable button and show loading
                BorrowButton.IsEnabled = false;
                BorrowResult.Text = "Processing your request...";
                BorrowResult.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Blue);

                // Here you would call the actual borrowing service
                // For now, just show a success message
                await System.Threading.Tasks.Task.Delay(1000); // Simulate API call

                BorrowResult.Text = "Success! Product borrowed successfully.";
                BorrowResult.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Green);

                // Show success dialog
                var dialog = new ContentDialog
                {
                    Title = "Borrowing Successful",
                    Content = $"You have successfully borrowed '{Product.Title}' until {endDate.Value.ToString("MMMM dd, yyyy")}.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();

                // Navigate back to borrow products page
                Frame.Navigate(typeof(MarketMindsPage), 2);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error borrowing product: {ex.Message}");
                
                BorrowResult.Text = "Error processing request";
                BorrowResult.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
                
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "An error occurred while processing your borrowing request.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
            finally
            {
                BorrowButton.IsEnabled = true;
            }
        }

        private async void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            if (Product == null || !IsCurrentBorrower) return;

            var confirmDialog = new ContentDialog
            {
                Title = "Confirm Return",
                Content = "Are you sure you want to return this product?",
                PrimaryButtonText = "Yes, Return",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();
            if (result != ContentDialogResult.Primary) return;

            try
            {
                // Disable button and show loading
                ReturnButton.IsEnabled = false;

                // Here you would call the actual return service
                // For now, just show a success message
                await System.Threading.Tasks.Task.Delay(1000); // Simulate API call

                var dialog = new ContentDialog
                {
                    Title = "Return Successful",
                    Content = "Product returned successfully.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();

                // Navigate back to borrow products page
                Frame.Navigate(typeof(MarketMindsPage), 2);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error returning product: {ex.Message}");
                
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "An error occurred while processing your return request.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
            finally
            {
                ReturnButton.IsEnabled = true;
            }
        }

        private async void JoinWaitlistButton_Click(object sender, RoutedEventArgs e)
        {
            if (Product == null || !CanInteractWithProduct) return;

            var endDate = WaitlistEndDatePicker.Date;
            if (endDate == null)
            {
                WaitlistResult.Text = "Please select your desired end date first";
                WaitlistResult.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
                return;
            }

            try
            {
                // Disable button and show loading
                JoinWaitlistButton.IsEnabled = false;
                WaitlistResult.Text = "Processing your request...";
                WaitlistResult.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Blue);

                // Here you would call the actual waitlist service
                // For now, just show a success message
                await System.Threading.Tasks.Task.Delay(1000); // Simulate API call

                WaitlistResult.Text = "Success! You have joined the waitlist.";
                WaitlistResult.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Green);

                // Show success dialog
                var dialog = new ContentDialog
                {
                    Title = "Joined Waitlist",
                    Content = $"You have successfully joined the waitlist for '{Product.Title}'. You will be notified when it becomes available.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error joining waitlist: {ex.Message}");
                
                WaitlistResult.Text = "Error processing request";
                WaitlistResult.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
                
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "An error occurred while joining the waitlist.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
            finally
            {
                JoinWaitlistButton.IsEnabled = true;
            }
        }

        private async void LeaveWaitlistButton_Click(object sender, RoutedEventArgs e)
        {
            if (Product == null || !CanInteractWithProduct) return;

            var confirmDialog = new ContentDialog
            {
                Title = "Leave Waitlist",
                Content = "Are you sure you want to leave the waitlist for this product?",
                PrimaryButtonText = "Yes, Leave",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();
            if (result != ContentDialogResult.Primary) return;

            try
            {
                // Disable button and show loading
                LeaveWaitlistButton.IsEnabled = false;

                // Here you would call the actual leave waitlist service
                // For now, just show a success message
                await System.Threading.Tasks.Task.Delay(1000); // Simulate API call

                var dialog = new ContentDialog
                {
                    Title = "Left Waitlist",
                    Content = "You have successfully left the waitlist.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error leaving waitlist: {ex.Message}");
                
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "An error occurred while leaving the waitlist.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
            finally
            {
                LeaveWaitlistButton.IsEnabled = true;
            }
        }

        private void LeaveReviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (Product?.Seller == null) return;

            // Navigate to review page or show review dialog
            // For now, just show a placeholder dialog
            var dialog = new ContentDialog
            {
                Title = "Leave Review",
                Content = $"Review functionality for lender '{SellerName}' will be implemented here.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            _ = dialog.ShowAsync();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to home/main page with Buy Products tab selected (index 0)
            Frame.Navigate(typeof(MarketMindsPage), 0);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to MarketMindsPage with Borrow Products tab selected (index 2)
            Frame.Navigate(typeof(MarketMindsPage), 2);
        }

        private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }
            return false;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void UpdateDailyRateButton_Click(object sender, RoutedEventArgs e)
        {
            if (Product == null || !IsCurrentUserSeller)
                return;

            // Read the new rate from NumberBox
            double newRate = DailyRateNumberBox.Value;

            // Store it on the model
            Product.DailyRate = newRate;

            try
            {
                // Use the "update entire product" method
                var service = new BorrowProductsService();
                bool success = await service.UpdateBorrowProductAsync(Product);

                if (success)
                {
                    // Notify UI-bindings that Product.DailyRate changed
                    OnPropertyChanged(nameof(Product));

                    var dialog = new ContentDialog
                    {
                        Title = "Rate Updated",
                        Content = $"Daily rate is now €{newRate:F2}.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await dialog.ShowAsync();
                }
                else
                {
                    var dialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = "Failed to save the new rate.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating daily rate: {ex.Message}");
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "An error occurred while updating the daily rate.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }
    }

} 