using System.Collections.ObjectModel;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services.ReviewCalculationService
{
    public class ReviewCalculationService : IReviewCalculationService
    {
        private const int NO_REVIEWS = 0;
        private const int NO_RATINGS = 0;

        public double CalculateAverageRating(ObservableCollection<Review> reviews)
        {
            if (reviews == null || reviews.Count == 0)
            {
                return 0;
            }

            double totalRating = 0;
            foreach (var review in reviews)
            {
                totalRating += review.Rating;
            }

            double averageRating = totalRating / reviews.Count;
            return averageRating;
        }

        public int GetReviewCount(ObservableCollection<Review> reviews)
        {
            int count = reviews?.Count ?? 0;
            return count;
        }

        public bool AreReviewsEmpty(ObservableCollection<Review> reviews)
        {
            bool isEmpty = reviews == null || reviews.Count == 0;
            return isEmpty;
        }
    }
}