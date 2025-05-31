using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MarketMinds.Services;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ReviewService;
using MarketMinds.Shared.Services.ReviewCalculationService;
using MarketMinds.Shared.Services.UserService;

namespace MarketMinds.ViewModels
{
    public class SellerReviewViewModel : INotifyPropertyChanged
    {
        private readonly IReviewsService ReviewsService;
        private readonly IReviewCalculationService ReviewCalculationService;
        private readonly IUserService UserService;

        private ObservableCollection<Review> _reviews;
        private double _rating;
        private bool _isReviewsEmpty;
        private int _reviewCount;
        private string _statusMessage;
        private int _sellerId;
        private int _productId;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Review> Reviews
        {
            get => _reviews;
            set => SetProperty(ref _reviews, value);
        }

        public double Rating
        {
            get => _rating;
            set => SetProperty(ref _rating, value);
        }

        public bool IsReviewsEmpty
        {
            get => _isReviewsEmpty;
            set => SetProperty(ref _isReviewsEmpty, value);
        }

        public int ReviewCount
        {
            get => _reviewCount;
            set => SetProperty(ref _reviewCount, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public int SellerId
        {
            get => _sellerId;
            set => SetProperty(ref _sellerId, value);
        }

        public int ProductId
        {
            get => _productId;
            set => SetProperty(ref _productId, value);
        }

        public SellerReviewViewModel(IReviewsService reviewsService, 
                                   IReviewCalculationService reviewCalculationService,
                                   IUserService userService)
        {
            ReviewsService = reviewsService ?? throw new ArgumentNullException(nameof(reviewsService));
            ReviewCalculationService = reviewCalculationService ?? throw new ArgumentNullException(nameof(reviewCalculationService));
            UserService = userService ?? throw new ArgumentNullException(nameof(userService));
            _reviews = new ObservableCollection<Review>();
            _statusMessage = string.Empty;
        }

        // For DI through service provider
        public SellerReviewViewModel()
        {
            // For design-time support or when DI isn't available
            _reviews = new ObservableCollection<Review>();
            _statusMessage = string.Empty;
        }

        public async Task<bool> SubmitReviewAsync(int sellerId, int rating, string reviewText, int productId)
        {
            try
            {
                StatusMessage = "Submitting review...";
                
                // Get seller user object
                var sellerUser = await UserService.GetUserByIdAsync(sellerId);
                if (sellerUser == null)
                {
                    StatusMessage = "Seller not found";
                    return false;
                }

                // Get buyer user object (current user)
                var buyerUser = App.CurrentUser;
                
                // Create review
                ReviewsService.AddReview(
                    reviewText,             // description
                    new System.Collections.Generic.List<Image>(),      // images (empty for now)
                    rating,                 // rating
                    sellerUser,             // seller
                    buyerUser               // buyer
                );

                StatusMessage = "Review submitted successfully";
                
                // Refresh reviews if we're viewing the seller's reviews
                await RefreshSellerReviewsAsync(sellerId);
                return true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                return false;
            }
        }

        public async Task<bool> EditReviewAsync(Review review, string description, float rating)
        {
            try
            {
                StatusMessage = "Updating review...";
                
                // Call service to edit the review
                ReviewsService.EditReview(
                    review.Id,
                    review.Description,
                    review.Images,
                    review.Rating,
                    review.SellerId,
                    review.BuyerId,
                    description,
                    rating
                );
                
                StatusMessage = "Review updated successfully";
                await RefreshSellerReviewsAsync(review.SellerId);
                return true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                return false;
            }
        }

        public async Task<bool> DeleteReviewAsync(Review review)
        {
            try
            {
                StatusMessage = "Deleting review...";
                
                // Call service to delete the review
                ReviewsService.DeleteReview(
                    review.Id,
                    review.Description,
                    review.Images,
                    review.Rating,
                    review.SellerId,
                    review.BuyerId
                );
                
                StatusMessage = "Review deleted successfully";
                Reviews.Remove(review);
                
                // Update statistics
                ReviewCount = ReviewCalculationService.GetReviewCount(Reviews);
                Rating = ReviewCalculationService.CalculateAverageRating(Reviews);
                IsReviewsEmpty = ReviewCalculationService.AreReviewsEmpty(Reviews);
                return true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                return false;
            }
        }

        public async Task LoadSellerReviewsAsync(int sellerId)
        {
            try
            {
                StatusMessage = "Loading reviews...";
                
                // Get seller and their reviews
                User seller = await UserService.GetUserByIdAsync(sellerId);
                if (seller != null)
                {
                    Reviews = ReviewsService.GetReviewsBySeller(seller);
                    ReviewCount = ReviewCalculationService.GetReviewCount(Reviews);
                    Rating = ReviewCalculationService.CalculateAverageRating(Reviews);
                    IsReviewsEmpty = ReviewCalculationService.AreReviewsEmpty(Reviews);
                    StatusMessage = string.Empty;
                }
                else
                {
                    StatusMessage = "Seller not found";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading reviews: {ex.Message}";
            }
        }

        private async Task RefreshSellerReviewsAsync(int sellerId)
        {
            await LoadSellerReviewsAsync(sellerId);
        }

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    // Helper class for passing review data
    public class SellerReviewData
    {
        public int SellerId { get; set; }
        public int ProductId { get; set; }
        public string SellerName { get; set; }
        public string ProductTitle { get; set; } // Changed from ProductName to ProductTitle for consistency
    }
}