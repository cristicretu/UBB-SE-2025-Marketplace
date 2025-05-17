using System.Collections.ObjectModel;

using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ReviewService;
using MarketMinds.Shared.Services.ReviewCalculationService;

namespace BusinessLogicLayer.ViewModel
{
    public class SeeSellerReviewsViewModel
    {
        public User Seller { get; set; }
        public User Viewer { get; set; }
        public ReviewsService ReviewsService { get; set; }
        public ObservableCollection<Review> Reviews { get; set; }
        public double Rating { get; set; }
        public bool IsReviewsEmpty { get; set; }
        public int ReviewCount { get; set; }
        private readonly ReviewCalculationService reviewCalculationService;

        public SeeSellerReviewsViewModel(ReviewsService reviewsService, User seller, User viewer)
        {
            this.Seller = seller;
            this.Viewer = viewer;
            this.ReviewsService = reviewsService;
            this.reviewCalculationService = new ReviewCalculationService();
            Reviews = reviewsService.GetReviewsBySeller(seller);
            ReviewCount = reviewCalculationService.GetReviewCount(Reviews);
            Rating = reviewCalculationService.CalculateAverageRating(Reviews);
            IsReviewsEmpty = reviewCalculationService.AreReviewsEmpty(Reviews);
        }

        public void MessageReviewer()
        {
            // if seller == viewer -> message with "About the product I sold you"
            // if seller != viewer -> message with "Could you tell me more about what you bought"
        }

        public void RefreshData()
        {
            Reviews = ReviewsService.GetReviewsBySeller(Seller);
            ReviewCount = reviewCalculationService.GetReviewCount(Reviews);
            Rating = reviewCalculationService.CalculateAverageRating(Reviews);
            IsReviewsEmpty = reviewCalculationService.AreReviewsEmpty(Reviews);
        }
    }
}
