using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ImagineUploadService;
using MarketMinds.Shared.Services.ReviewService;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Shared.Services.ReviewCreationService
{
    public class ReviewCreationService : IReviewCreationService
    {
        private readonly ReviewsService reviewsService;
        private readonly IImageUploadService imageService;

        public ReviewCreationService(ReviewsService reviewsService, IConfiguration configuration)
        {
            this.reviewsService = reviewsService;
            this.imageService = new ImageUploadService(configuration);
        }

        public Review CreateReview(string description, List<Image> images, double rating, User seller, User buyer)
        {
            if (seller == null || buyer == null || reviewsService == null)
            {
                throw new ArgumentException("One of the required objects is null!");
            }

            reviewsService.AddReview(description, images, rating, seller, buyer);
            return new Review(-1, description, images, rating, seller.Id, buyer.Id);
        }

        public void UpdateReview(Review currentReview, string newDescription, double newRating)
        {
            if (currentReview == null)
            {
                throw new ArgumentException("Current review cannot be null");
            }

            reviewsService.EditReview(
                currentReview.Id,
                currentReview.Description,
                currentReview.Images,
                currentReview.Rating,
                currentReview.SellerId,
                currentReview.BuyerId,
                newDescription,
                newRating);
        }

        public void EditReview(Review review, string description, double rating)
        {
            if (review == null)
            {
                throw new ArgumentException("Review cannot be null");
            }

            reviewsService.EditReview(
                review.Id,
                review.Description,
                review.Images,
                review.Rating,
                review.SellerId,
                review.BuyerId,
                description,
                rating);
        }

        public void DeleteReview(Review review)
        {
            if (review == null)
            {
                throw new ArgumentException("Review cannot be null");
            }

            reviewsService.DeleteReview(
                review.Id,
                review.Description,
                review.Images,
                review.Rating,
                review.SellerId,
                review.BuyerId);
        }

        public List<Image> ParseImagesString(string imagesString)
        {
            return imageService.ParseImagesString(imagesString);
        }

        public string FormatImagesString(List<Image> images)
        {
            return imageService.FormatImagesString(images);
        }
    }
}