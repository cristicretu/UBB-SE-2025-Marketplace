using System.Collections.ObjectModel;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ReviewService;
using MarketMinds.Shared.Services.ReviewCalculationService;
using MarketMinds.Shared.Services.ReviewCreationService;
using MarketMinds.Shared.Helper;

namespace BusinessLogicLayer.ViewModel
{
    public class SeeBuyerReviewsViewModel
    {
        public User User { get; set; }
        public ReviewsService ReviewsService { get; set; }
        public ObservableCollection<Review> Reviews { get; set; }
        public double Rating { get; set; }
        public bool IsReviewsEmpty { get; set; }
        public int ReviewCount { get; set; }
        private readonly ReviewCalculationService reviewCalculationService;
        private readonly ReviewCreationService reviewCreationService;

        public SeeBuyerReviewsViewModel(ReviewsService reviewsService, User user)
        {
            this.User = user;
            this.ReviewsService = reviewsService;
            this.reviewCalculationService = new ReviewCalculationService();
            this.reviewCreationService = new ReviewCreationService(reviewsService, AppConfig.Configuration);
            Reviews = reviewsService.GetReviewsByBuyer(user);
            ReviewCount = reviewCalculationService.GetReviewCount(Reviews);
            Rating = reviewCalculationService.CalculateAverageRating(Reviews);
            IsReviewsEmpty = reviewCalculationService.AreReviewsEmpty(Reviews);
        }

        public void EditReview(Review review, string description, float rating)
        {
            reviewCreationService.EditReview(review, description, rating);
        }

        public void DeleteReview(Review review)
        {
            reviewCreationService.DeleteReview(review);
            Reviews.Remove(review);
            // Update review statistics after deletion
            ReviewCount = reviewCalculationService.GetReviewCount(Reviews);
            Rating = reviewCalculationService.CalculateAverageRating(Reviews);
            IsReviewsEmpty = reviewCalculationService.AreReviewsEmpty(Reviews);
        }

        public void RefreshData()
        {
            Reviews = ReviewsService.GetReviewsByBuyer(User);
            ReviewCount = reviewCalculationService.GetReviewCount(Reviews);
            Rating = reviewCalculationService.CalculateAverageRating(Reviews);
            IsReviewsEmpty = reviewCalculationService.AreReviewsEmpty(Reviews);
        }
    }
}
