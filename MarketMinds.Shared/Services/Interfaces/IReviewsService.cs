using System.Collections.ObjectModel;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services.ReviewService
{
    /// <summary>
    /// Interface for ReviewsService to manage review operations.
    /// </summary>
    public interface IReviewsService
    {
        /// <summary>
        /// Retrieves all reviews for a specific seller.
        /// </summary>
        /// <param name="seller">The seller whose reviews are to be retrieved.</param>
        /// <returns>An observable collection of reviews for the seller.</returns>
        ObservableCollection<Review> GetReviewsBySeller(User seller);

        /// <summary>
        /// Retrieves all reviews by a specific buyer.
        /// </summary>
        /// <param name="buyer">The buyer whose reviews are to be retrieved.</param>
        /// <returns>An observable collection of reviews by the buyer.</returns>
        ObservableCollection<Review> GetReviewsByBuyer(User buyer);

        /// <summary>
        /// Adds a new review.
        /// </summary>
        /// <param name="description">The description of the review.</param>
        /// <param name="images">The images associated with the review.</param>
        /// <param name="rating">The rating of the review.</param>
        /// <param name="seller">The seller being reviewed.</param>
        /// <param name="buyer">The buyer writing the review.</param>
        void AddReview(string description, List<Image> images, double rating, User seller, User buyer);

        /// <summary>
        /// Edits an existing review.
        /// </summary>
        /// <param name="reviewId">The ID of the review to edit.</param>
        /// <param name="description">The current description of the review.</param>
        /// <param name="images">The current images associated with the review.</param>
        /// <param name="rating">The current rating of the review.</param>
        /// <param name="sellerid">The ID of the seller being reviewed.</param>
        /// <param name="buyerid">The ID of the buyer writing the review.</param>
        /// <param name="newDescription">The new description for the review.</param>
        /// <param name="newRating">The new rating for the review.</param>
        void EditReview(int reviewId, string description, List<Image> images, double rating, int sellerid, int buyerid, string newDescription, double newRating);

        /// <summary>
        /// Deletes a review.
        /// </summary>
        /// <param name="reviewId">The ID of the review to delete.</param>
        /// <param name="description">The description of the review.</param>
        /// <param name="images">The images associated with the review.</param>
        /// <param name="rating">The rating of the review.</param>
        /// <param name="sellerid">The ID of the seller being reviewed.</param>
        /// <param name="buyerid">The ID of the buyer writing the review.</param>
        void DeleteReview(int reviewId, string description, List<Image> images, double rating, int sellerid, int buyerid);
    }
}


