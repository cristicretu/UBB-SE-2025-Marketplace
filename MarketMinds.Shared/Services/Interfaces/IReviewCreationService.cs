using System.Collections.Generic;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services.ReviewCreationService
{
    /// <summary>
    /// Interface for ReviewCreationService to manage review creation and related operations.
    /// </summary>
    public interface IReviewCreationService
    {
        /// <summary>
        /// Creates a new review.
        /// </summary>
        /// <param name="description">The description of the review.</param>
        /// <param name="images">The images associated with the review.</param>
        /// <param name="rating">The rating of the review.</param>
        /// <param name="seller">The seller being reviewed.</param>
        /// <param name="buyer">The buyer writing the review.</param>
        /// <returns>The created Review object.</returns>
        Review CreateReview(string description, List<Image> images, double rating, User seller, User buyer);

        /// <summary>
        /// Updates an existing review with new details.
        /// </summary>
        /// <param name="currentReview">The current review to update.</param>
        /// <param name="newDescription">The new description for the review.</param>
        /// <param name="newRating">The new rating for the review.</param>
        void UpdateReview(Review currentReview, string newDescription, double newRating);

        /// <summary>
        /// Edits an existing review.
        /// </summary>
        /// <param name="review">The review to edit.</param>
        /// <param name="description">The new description for the review.</param>
        /// <param name="rating">The new rating for the review.</param>
        void EditReview(Review review, string description, double rating);

        /// <summary>
        /// Deletes a review.
        /// </summary>
        /// <param name="review">The review to delete.</param>
        void DeleteReview(Review review);

        /// <summary>
        /// Parses a string of image URLs into a list of Image objects.
        /// </summary>
        /// <param name="imagesString">The string of image URLs.</param>
        /// <returns>A list of Image objects.</returns>
        List<Image> ParseImagesString(string imagesString);

        /// <summary>
        /// Formats a list of images into a single string.
        /// </summary>
        /// <param name="images">The list of images.</param>
        /// <returns>A formatted string of image URLs.</returns>
        string FormatImagesString(List<Image> images);
    }
}

