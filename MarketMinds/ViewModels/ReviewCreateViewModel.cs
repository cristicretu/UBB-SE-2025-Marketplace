using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ReviewService;
using MarketMinds.Shared.Services.ImagineUploadService;
using MarketMinds.Shared.Services.ReviewCreationService;

namespace ViewModelLayer.ViewModel
{
    public class ReviewCreateViewModel
    {
        public Review CurrentReview { get; set; }
        public ReviewsService ReviewsService { get; set; }
        public User Seller { get; set; }
        public User Buyer { get; set; }
        public string Description { get; set; }
        public List<Image> Images { get; set; }
        public double Rating { get; set; }
        public string StatusMessage { get; set; }
        private readonly ReviewCreationService reviewCreationService;
        private readonly ImageUploadService imageUploadService;

        public string ImagesString
        {
            get => reviewCreationService.FormatImagesString(Images);
            set => Images = reviewCreationService.ParseImagesString(value);
        }

        public ReviewCreateViewModel(ReviewsService reviewsService, User buyer, User seller)
        {
            ReviewsService = reviewsService;
            Buyer = buyer;
            Seller = seller;
            reviewCreationService = new ReviewCreationService(reviewsService);
            imageUploadService = new ImageUploadService();
            Images = new List<Image>();
            StatusMessage = string.Empty;
        }

        public async Task<bool> AddImage(Stream imageStream, string fileName)
        {
            if (imageStream == null)
            {
                StatusMessage = "No image stream provided.";
                return false;
            }
            if (string.IsNullOrEmpty(fileName))
            {
                StatusMessage = "No file name provided.";
                return false;
            }

            try
            {
                string originalImagesString = ImagesString;
                StatusMessage = "Uploading image...";

                // Call the updated service method
                string updatedImagesString = await imageUploadService.AddImageToCollection(imageStream, fileName, ImagesString);

                if (updatedImagesString != originalImagesString)
                {
                    ImagesString = updatedImagesString;
                    StatusMessage = "Image uploaded successfully";
                    return true;
                }
                else
                {
                    // This case might mean the image was a duplicate and wasn't added, or no link was returned.
                    // The service's AddImageToCollection already handles not adding duplicates.
                    // If updatedImagesString is same as original, it means either upload failed to return a link OR it was a duplicate.
                    // If it was a duplicate and already in ImagesString, this is fine.
                    // If upload failed and returned null/empty, it would also be same as original if original was also null/empty.
                    // Let's assume if it's same, it means no *new* unique image was added or upload failed.
                    StatusMessage = "Image not added (possibly a duplicate or upload issue).";
                    return false;
                }
            }
            catch (Exception imageAdditionException)
            {
                // Handle errors
                StatusMessage = $"Error uploading image: {imageAdditionException.Message}";
                return false;
            }
        }

        public void SubmitReview()
        {
            try
            {
                Debug.WriteLine("Submitting Review");
                StatusMessage = "Submitting review...";
                CurrentReview = reviewCreationService.CreateReview(Description, Images, Rating, Seller, Buyer);
                StatusMessage = "Review submitted successfully";
            }
            catch (Exception reviewSubmissionException)
            {
                StatusMessage = $"Error submitting review: {reviewSubmissionException.Message}";
            }
        }

        public void UpdateReview()
        {
            try
            {
                StatusMessage = "Updating review...";
                reviewCreationService.UpdateReview(CurrentReview, Description, Rating);
                StatusMessage = "Review updated successfully";
            }
            catch (Exception reviewUpdateException)
            {
                StatusMessage = $"Error updating review: {reviewUpdateException.Message}";
            }
        }
    }
}
