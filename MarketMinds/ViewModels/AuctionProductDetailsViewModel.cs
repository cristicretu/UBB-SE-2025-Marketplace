using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MarketMinds.Shared.Helper;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.Interfaces;
using Microsoft.UI.Dispatching;

namespace MarketMinds.ViewModels
{
    public class AuctionProductDetailsViewModel : INotifyPropertyChanged
    {
        private readonly IAuctionProductService auctionProductService;
        private readonly DispatcherQueue dispatcherQueue;
        private AuctionProduct? product;
        private string bidAmount = string.Empty;
        private string timeLeft = string.Empty;
        private bool isLoading = true;
        private bool isPlacingBid = false;
        private string errorMessage = string.Empty;
        private string successMessage = string.Empty;
        private bool canPlaceBid = false;
        private DispatcherQueueTimer? countdownTimer;
        private DateTime newEndDate = DateTime.Now.AddDays(1);
        private bool isUpdatingEndDate = false;

        public AuctionProductDetailsViewModel(IAuctionProductService auctionProductService)
        {
            this.auctionProductService = auctionProductService;
            dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            // Simplify command - let XAML IsEnabled binding handle the enabled state
            PlaceBidCommand = new RelayCommand(async () => await PlaceBidAsync());
            UpdateEndDateCommand = new RelayCommand(async () => await UpdateEndDateAsync());
            BidHistory = new ObservableCollection<Bid>();

            // Start countdown timer
            StartCountdownTimer();

            // Ensure command state is properly initialized
            UpdateCanPlaceBid();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        #region Properties

        public AuctionProduct? Product
        {
            get => product;
            set
            {
                product = value;
                OnPropertyChanged();

                // Update all related properties when product changes
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(Description));
                OnPropertyChanged(nameof(CurrentPrice));
                OnPropertyChanged(nameof(StartPrice));
                OnPropertyChanged(nameof(SellerName));
                OnPropertyChanged(nameof(CategoryName));
                OnPropertyChanged(nameof(ConditionName));
                OnPropertyChanged(nameof(IsAuctionEnded));
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(Images));
                OnPropertyChanged(nameof(Tags));
                OnPropertyChanged(nameof(MinimumBid));
                OnPropertyChanged(nameof(CanCurrentUserBid));
                OnPropertyChanged(nameof(CanUserPlaceBids));
                OnPropertyChanged(nameof(IsOwner));

                // Load bid history and update UI state
                LoadBidHistory();
                UpdateCanPlaceBid();

                // Start countdown timer if product is loaded
                if (product != null)
                {
                    countdownTimer?.Stop();
                    StartCountdownTimer();
                }
            }
        }

        public string Title => Product?.Title ?? string.Empty;

        public string Description => Product?.Description ?? "No description available for this product.";

        public double CurrentPrice => Product?.CurrentPrice ?? 0;

        public double StartPrice => Product?.StartPrice ?? 0;

        public string SellerName => Product?.Seller?.Username ?? "Unknown seller";

        public string CategoryName => Product?.Category?.Name ?? "Uncategorized";

        public string ConditionName => Product?.Condition?.Name ?? "Unknown";

        public bool IsAuctionEnded => Product != null && auctionProductService.IsAuctionEnded(Product);

        public string StatusText => IsAuctionEnded ? "Ended" : "Live Auction";

        public ObservableCollection<ProductImage> Images => Product?.Images != null
            ? new ObservableCollection<ProductImage>(Product.Images)
            : new ObservableCollection<ProductImage>();

        public ObservableCollection<ProductTag> Tags => Product?.Tags != null
            ? new ObservableCollection<ProductTag>(Product.Tags)
            : new ObservableCollection<ProductTag>();

        public string BidAmount
        {
            get => bidAmount;
            set
            {
                bidAmount = value;
                OnPropertyChanged();
                UpdateCanPlaceBid();
            }
        }

        public string TimeLeft
        {
            get => timeLeft;
            set
            {
                timeLeft = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                OnPropertyChanged();

                // Update button state when loading finishes
                if (!isLoading)
                {
                    UpdateCanPlaceBid();
                }
            }
        }

        public bool IsPlacingBid
        {
            get => isPlacingBid;
            set
            {
                isPlacingBid = value;
                OnPropertyChanged();
                UpdateCanPlaceBid();
            }
        }

        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                errorMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasErrorMessage));
            }
        }

        public string SuccessMessage
        {
            get => successMessage;
            set
            {
                successMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSuccessMessage));
            }
        }

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public bool HasSuccessMessage => !string.IsNullOrEmpty(SuccessMessage);

        public bool CanPlaceBid
        {
            get
            {
                // Always return true - let users click and show validation errors instead
                return true;
            }
            private set
            {
                // Keep the setter for backward compatibility but it won't be used
                if (canPlaceBid != value)
                {
                    canPlaceBid = value;
                    System.Diagnostics.Debug.WriteLine($"CanPlaceBid setter called: {canPlaceBid}");
                    OnPropertyChanged();

                    // Force additional property change notifications
                    OnPropertyChanged(nameof(CanPlaceBid));
                }
            }
        }

        public bool CanCurrentUserBid => IsUserBuyer && !IsAuctionEnded;

        public bool IsUserSeller => App.CurrentUser?.UserType == 3; // Seller

        public bool IsUserBuyer => App.CurrentUser?.UserType == 2; // Buyer

        public bool IsUserAdmin => App.CurrentUser?.UserType == 1; // Admin

        public bool CanUserPlaceBids => (IsUserBuyer || IsUserAdmin) && !IsUserSeller;

        public double MinimumBid => CurrentPrice + 0.01;

        public bool IsOwner => App.CurrentUser?.UserType == 3 && App.CurrentUser?.Id == Product?.Seller?.Id;

        public DateTime NewEndDate
        {
            get => newEndDate;
            set
            {
                newEndDate = value;
                OnPropertyChanged();
            }
        }

        public bool IsUpdatingEndDate
        {
            get => isUpdatingEndDate;
            set
            {
                isUpdatingEndDate = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Bid> BidHistory { get; }

        #endregion

        #region Commands

        public ICommand PlaceBidCommand { get; }
        public ICommand UpdateEndDateCommand { get; }

        #endregion

        #region Methods

        public async Task LoadProductAsync(int productId)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                var product = await auctionProductService.GetAuctionProductByIdAsync(productId);
                if (product != null)
                {
                    Product = product;
                    UpdateTimeLeft();

                    // Add comprehensive debugging for user permissions
                    DebugUserPermissions();

                    // Ensure command state is updated after product loads
                    UpdateCanPlaceBid();
                }
                else
                {
                    ErrorMessage = "Product not found.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading product: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task PlaceBidAsync()
        {
            try
            {
                IsPlacingBid = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                // Validation 1: Check if product exists
                if (Product == null)
                {
                    ErrorMessage = "Product information not available. Please refresh the page.";
                    return;
                }

                // Validation 2: Check if user is logged in
                if (App.CurrentUser == null)
                {
                    ErrorMessage = "You must be logged in to place bids. Please log in first.";
                    return;
                }

                // Validation 3: Check user permissions
                if (!CanUserPlaceBids)
                {
                    if (IsUserSeller)
                    {
                        ErrorMessage = "Sellers cannot place bids on auctions. Only buyers can place bids.";
                    }
                    else
                    {
                        ErrorMessage = "Your account doesn't have permission to place bids. Only buyer accounts can place bids.";
                    }
                    return;
                }

                // Validation 4: Check if auction has ended
                if (IsAuctionEnded)
                {
                    ErrorMessage = "This auction has already ended. You can no longer place bids.";
                    return;
                }

                // Validation 5: Check if bid amount is provided
                if (string.IsNullOrWhiteSpace(BidAmount))
                {
                    ErrorMessage = "Please enter a bid amount.";
                    return;
                }

                // Validation 6: Parse bid amount
                if (!double.TryParse(BidAmount.Replace(",", "."), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double bidValue))
                {
                    ErrorMessage = "Please enter a valid bid amount (numbers only).";
                    return;
                }

                // Validation 7: Check minimum bid requirement
                if (bidValue <= CurrentPrice)
                {
                    ErrorMessage = $"Your bid must be higher than the current price. Please enter an amount greater than €{CurrentPrice:F2}.";
                    return;
                }

                // All validations passed - attempt to place the bid
                System.Diagnostics.Debug.WriteLine($"Placing bid: ProductId={Product.Id}, UserId={App.CurrentUser.Id}, BidValue={bidValue}");

                var success = await auctionProductService.PlaceBidAsync(Product.Id, App.CurrentUser.Id, bidValue);
                if (success)
                {
                    SuccessMessage = $"Your bid of €{bidValue:F2} was successfully placed!";
                    BidAmount = string.Empty;

                    // Reload the product to get updated data
                    await LoadProductAsync(Product.Id);
                }
                else
                {
                    ErrorMessage = "Failed to place bid. This might be due to network issues or the auction may have ended. Please try again.";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error placing bid: {ex}");

                // Handle specific exceptions with user-friendly messages
                if (ex.Message.Contains("permission") || ex.Message.Contains("buyer"))
                {
                    ErrorMessage = "Your account doesn't have permission to place bids. Only buyer accounts can place bids.";
                }
                else if (ex.Message.Contains("network") || ex.Message.Contains("timeout"))
                {
                    ErrorMessage = "Network error occurred. Please check your connection and try again.";
                }
                else if (ex.Message.Contains("auction") && ex.Message.Contains("ended"))
                {
                    ErrorMessage = "This auction has ended while you were placing your bid.";
                }
                else
                {
                    ErrorMessage = $"An unexpected error occurred: {ex.Message}. Please try again.";
                }
            }
            finally
            {
                IsPlacingBid = false;
            }
        }

        private void UpdateCanPlaceBid()
        {
            // Button is always enabled now - validation happens when user clicks
            System.Diagnostics.Debug.WriteLine($"UpdateCanPlaceBid called - Button always enabled");
            System.Diagnostics.Debug.WriteLine($"Current state: IsPlacingBid={IsPlacingBid}, IsAuctionEnded={IsAuctionEnded}, CanUserPlaceBids={CanUserPlaceBids}, BidAmount='{BidAmount}', CurrentPrice={CurrentPrice}");

            // Set to true (always enabled)
            CanPlaceBid = true;
        }

        private void LoadBidHistory()
        {
            BidHistory.Clear();
            if (Product?.Bids != null)
            {
                var sortedBids = Product.Bids.OrderByDescending(b => b.Timestamp);
                foreach (var bid in sortedBids)
                {
                    BidHistory.Add(bid);
                }
            }
            OnPropertyChanged(nameof(BidHistory));
        }

        private void StartCountdownTimer()
        {
            countdownTimer = dispatcherQueue.CreateTimer();
            countdownTimer.Interval = TimeSpan.FromSeconds(1);
            countdownTimer.IsRepeating = true;
            countdownTimer.Tick += (sender, args) => UpdateTimeLeft();
            countdownTimer.Start();
        }

        private void UpdateTimeLeft()
        {
            if (Product == null)
            {
                return;
            }

            var timeLeft = Product.EndTime - DateTime.Now;
            if (timeLeft <= TimeSpan.Zero)
            {
                TimeLeft = "Auction Ended";
                OnPropertyChanged(nameof(IsAuctionEnded));
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(CanCurrentUserBid));
                UpdateCanPlaceBid();
            }
            else
            {
                TimeLeft = $"{timeLeft.Days}d {timeLeft.Hours}h {timeLeft.Minutes}m {timeLeft.Seconds}s";
            }
        }

        private void DebugUserPermissions()
        {
            var currentUser = App.CurrentUser;
            System.Diagnostics.Debug.WriteLine("=== USER PERMISSION DEBUG ===");
            System.Diagnostics.Debug.WriteLine($"Current User: {(currentUser != null ? "EXISTS" : "NULL")}");
            if (currentUser != null)
            {
                System.Diagnostics.Debug.WriteLine($"User ID: {currentUser.Id}");
                System.Diagnostics.Debug.WriteLine($"Username: {currentUser.Username}");
                System.Diagnostics.Debug.WriteLine($"UserType: {currentUser.UserType}");
                System.Diagnostics.Debug.WriteLine($"UserType Type: {currentUser.UserType.GetType().Name}");
            }

            System.Diagnostics.Debug.WriteLine($"IsUserBuyer: {IsUserBuyer}");
            System.Diagnostics.Debug.WriteLine($"IsUserSeller: {IsUserSeller}");
            System.Diagnostics.Debug.WriteLine($"IsUserAdmin: {IsUserAdmin}");
            System.Diagnostics.Debug.WriteLine($"IsAuctionEnded: {IsAuctionEnded}");
            System.Diagnostics.Debug.WriteLine($"CanCurrentUserBid: {CanCurrentUserBid}");
            System.Diagnostics.Debug.WriteLine($"CanUserPlaceBids: {CanUserPlaceBids}");
            System.Diagnostics.Debug.WriteLine("=== END USER PERMISSION DEBUG ===");
        }

        // Public method to test user permissions manually
        public void TestUserPermissions()
        {
            DebugUserPermissions();
            UpdateCanPlaceBid();
        }

        // Public method to force refresh the CanPlaceBid property
        public void RefreshCanPlaceBid()
        {
            UpdateCanPlaceBid();
            OnPropertyChanged(nameof(CanPlaceBid));
        }

        private async Task UpdateEndDateAsync()
        {
            try
            {
                IsUpdatingEndDate = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                System.Diagnostics.Debug.WriteLine($"=== UPDATE END DATE DEBUG ===");
                System.Diagnostics.Debug.WriteLine($"Product: {(Product != null ? "EXISTS" : "NULL")}");
                System.Diagnostics.Debug.WriteLine($"Product ID: {Product?.Id}");
                System.Diagnostics.Debug.WriteLine($"Current User: {(App.CurrentUser != null ? "EXISTS" : "NULL")}");
                System.Diagnostics.Debug.WriteLine($"Current User ID: {App.CurrentUser?.Id}");
                System.Diagnostics.Debug.WriteLine($"Product Seller ID: {Product?.Seller?.Id}");
                System.Diagnostics.Debug.WriteLine($"IsOwner: {IsOwner}");
                System.Diagnostics.Debug.WriteLine($"IsUserSeller: {IsUserSeller}");
                System.Diagnostics.Debug.WriteLine($"New End Date: {newEndDate}");
                System.Diagnostics.Debug.WriteLine($"Current Time: {DateTime.Now}");

                // Validation 1: Check if product exists
                if (Product == null)
                {
                    ErrorMessage = "Product information not available. Please refresh the page.";
                    return;
                }

                // Validation 2: Check if user is logged in
                if (App.CurrentUser == null)
                {
                    ErrorMessage = "You must be logged in to update the end date. Please log in first.";
                    return;
                }

                // Validation 3: Check if user is the owner
                if (!IsOwner)
                {
                    ErrorMessage = "Only the seller who owns this auction can update the end date.";
                    return;
                }

                // Validation 4: Check if new end date is provided and valid
                if (newEndDate <= DateTime.Now)
                {
                    ErrorMessage = "Please select a future date for the auction end date.";
                    return;
                }

                // All validations passed - attempt to update the end date
                System.Diagnostics.Debug.WriteLine($"Updating end date: ProductId={Product.Id}, NewEndDate={newEndDate}");

                var originalEndTime = Product.EndTime;
                System.Diagnostics.Debug.WriteLine($"Original EndTime: {originalEndTime}");

                System.Diagnostics.Debug.WriteLine($"About to call UpdateAuctionProductAsync...");

                // Update the product's end date
                Product.EndTime = newEndDate;

                // Ensure foreign key fields are properly populated from navigation properties
                if (Product.Seller != null)
                {
                    Product.SellerId = Product.Seller.Id;
                }
                if (Product.Condition != null)
                {
                    Product.ConditionId = Product.Condition.Id;
                }
                if (Product.Category != null)
                {
                    Product.CategoryId = Product.Category.Id;
                }

                System.Diagnostics.Debug.WriteLine($"Before update - SellerId: {Product.SellerId}, ConditionId: {Product.ConditionId}, CategoryId: {Product.CategoryId}");

                var success = await auctionProductService.UpdateAuctionProductAsync(Product);

                System.Diagnostics.Debug.WriteLine($"UpdateAuctionProductAsync returned: {success}");

                if (success)
                {
                    SuccessMessage = $"The auction end date has been successfully updated to {newEndDate:yyyy-MM-dd HH:mm}!";
                    // Refresh the time left display
                    UpdateTimeLeft();
                    System.Diagnostics.Debug.WriteLine($"Update successful! New EndTime: {Product.EndTime}");
                }
                else
                {
                    // Restore original end time on failure
                    Product.EndTime = originalEndTime;
                    ErrorMessage = "Failed to update the end date. The server rejected the request. Please try again or contact support.";
                    System.Diagnostics.Debug.WriteLine($"Update failed! Restored EndTime to: {Product.EndTime}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in UpdateEndDateAsync: {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                }

                // Handle specific exceptions with user-friendly messages
                if (ex.Message.Contains("permission") || ex.Message.Contains("seller") || ex.Message.Contains("owner"))
                {
                    ErrorMessage = "You don't have permission to update this auction's end date.";
                }
                else if (ex.Message.Contains("network") || ex.Message.Contains("timeout") || ex.Message.Contains("HttpRequest"))
                {
                    ErrorMessage = "Network error occurred. Please check your connection and try again.";
                }
                else if (ex.Message.Contains("authentication") || ex.Message.Contains("login"))
                {
                    ErrorMessage = "Please log in again and try updating the end date.";
                }
                else
                {
                    ErrorMessage = $"An error occurred while updating the end date: {ex.Message}";
                }
            }
            finally
            {
                IsUpdatingEndDate = false;
                System.Diagnostics.Debug.WriteLine($"=== END UPDATE END DATE DEBUG ===");
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}