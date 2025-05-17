using System.Collections.ObjectModel;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services.ReviewCalculationService
{
    /// <summary>
    /// Interface for ReviewCalculationService to manage review-related calculations.
    /// </summary>
    public interface IReviewCalculationService
    {
        /// <summary>
        /// Calculates the average rating from a collection of reviews.
        /// </summary>
        /// <param name="reviews">The collection of reviews.</param>
        /// <returns>The average rating, or 0 if there are no reviews.</returns>
        double CalculateAverageRating(ObservableCollection<Review> reviews);

        /// <summary>
        /// Gets the total count of reviews in a collection.
        /// </summary>
        /// <param name="reviews">The collection of reviews.</param>
        /// <returns>The count of reviews, or 0 if the collection is null.</returns>
        int GetReviewCount(ObservableCollection<Review> reviews);

        /// <summary>
        /// Checks if a collection of reviews is empty or null.
        /// </summary>
        /// <param name="reviews">The collection of reviews.</param>
        /// <returns>True if the collection is null or empty, otherwise false.</returns>
        bool AreReviewsEmpty(ObservableCollection<Review> reviews);
    }
}

