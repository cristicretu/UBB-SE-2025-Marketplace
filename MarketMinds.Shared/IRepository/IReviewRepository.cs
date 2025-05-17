using System.Collections.ObjectModel;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.IRepository
{
    /// <summary>
    /// Interface for ReviewRepository to manage review operations.
    /// </summary>
    public interface IReviewRepository
    {
        /// <summary>
        /// Retrieves all reviews by a specific buyer.
        /// </summary>
        /// <param name="buyer">The buyer whose reviews are to be retrieved.</param>
        /// <returns>An observable collection of reviews by the buyer.</returns>
        ObservableCollection<Review> GetAllReviewsByBuyer(User buyer);

        /// <summary>
        /// Retrieves all reviews for a specific seller.
        /// </summary>
        /// <param name="seller">The seller whose reviews are to be retrieved.</param>
        /// <returns>An observable collection of reviews for the seller.</returns>
        ObservableCollection<Review> GetAllReviewsBySeller(User seller);

        /// <summary>
        /// Creates a new review.
        /// </summary>
        /// <param name="review">The review to be created.</param>
        void CreateReview(Review review);

        /// <summary>
        /// Edits an existing review.
        /// </summary>
        /// <param name="review">The review to be edited.</param>
        /// <param name="rating">The new rating for the review.</param>
        /// <param name="description">The new description for the review.</param>
        void EditReview(Review review, double rating, string description);

        /// <summary>
        /// Deletes a review.
        /// </summary>
        /// <param name="review">The review to be deleted.</param>
        void DeleteReview(Review review);
    }
}


