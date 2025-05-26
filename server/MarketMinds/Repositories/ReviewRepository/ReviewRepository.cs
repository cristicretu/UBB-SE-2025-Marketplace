using System.Collections.ObjectModel;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;
using Microsoft.EntityFrameworkCore;
using Server.DataAccessLayer;

namespace MarketMinds.Repositories.ReviewRepository
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ApplicationDbContext context;

        public ReviewRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public ObservableCollection<Review> GetAllReviewsByBuyer(User buyer)
        {
            try
            {
                var reviews = context.Reviews
                    .Include(r => r.ReviewImages)
                    .Where(r => r.BuyerId == buyer.Id)
                    .ToList();

                return new ObservableCollection<Review>(reviews);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting reviews for buyer {buyer.Id}: {ex.Message}");
                throw;
            }
        }

        public ObservableCollection<Review> GetAllReviewsBySeller(User seller)
        {
            try
            {
                var reviews = context.Reviews
                    .Include(r => r.ReviewImages)
                    .Where(r => r.SellerId == seller.Id)
                    .ToList();

                return new ObservableCollection<Review>(reviews);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting reviews for seller {seller.Id}: {ex.Message}");
                throw;
            }
        }

        public void CreateReview(Review review)
        {
            if (review == null)
            {
                throw new ArgumentNullException(nameof(review));
            }

            try
            {
                // Add the review first to get the ID
                context.Reviews.Add(review);
                context.SaveChanges();

                // Now handle the images if any exist
                if (review.Images != null && review.Images.Any())
                {
                    foreach (var image in review.Images)
                    {
                        var reviewImage = new ReviewImage
                        {
                            Url = image.Url,
                            ReviewId = review.Id
                        };

                        context.ReviewImages.Add(reviewImage);
                    }

                    context.SaveChanges();
                }
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"EF Core CreateReview Error: {ex.InnerException?.Message ?? ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General CreateReview Error: {ex.Message}");
                throw;
            }
        }

        public void EditReview(Review review, double rating, string description)
        {
            if (review == null)
            {
                throw new ArgumentNullException(nameof(review));
            }

            try
            {
                var reviewToEdit = context.Reviews
                    .Include(r => r.ReviewImages)
                    .FirstOrDefault(r => r.Id == review.Id && r.SellerId == review.SellerId && r.BuyerId == review.BuyerId);

                if (reviewToEdit == null)
                {
                    throw new KeyNotFoundException($"Review with ID {review.Id} not found.");
                }

                // Update review properties
                reviewToEdit.Rating = rating;
                reviewToEdit.Description = description;

                // Update images - smart approach to avoid duplicates
                // Get current image URLs from database
                var existingImageUrls = reviewToEdit.ReviewImages.Select(ri => ri.Url).ToList();

                // Get new image URLs from the request
                var newImageUrls = review.Images?.Select(img => img.Url).ToList() ?? new List<string>();

                // Find images to remove (exist in DB but not in new list)
                var urlsToRemove = existingImageUrls.Except(newImageUrls).ToList();

                // Find images to add (exist in new list but not in DB)
                var urlsToAdd = newImageUrls.Except(existingImageUrls).ToList();

                // Remove images that are no longer needed
                if (urlsToRemove.Any())
                {
                    var imagesToRemove = context.ReviewImages
                        .Where(ri => ri.ReviewId == review.Id && urlsToRemove.Contains(ri.Url))
                        .ToList();
                    context.ReviewImages.RemoveRange(imagesToRemove);
                }

                // Add new images that don't exist yet
                if (urlsToAdd.Any())
                {
                    foreach (var urlToAdd in urlsToAdd)
                    {
                        var reviewImage = new ReviewImage
                        {
                            Url = urlToAdd,
                            ReviewId = review.Id
                        };

                        context.ReviewImages.Add(reviewImage);
                    }
                }

                context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine($"Concurrency error updating review {review.Id}: {ex.Message}");
                throw;
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"EF Core EditReview Error: {ex.InnerException?.Message ?? ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General EditReview Error: {ex.Message}");
                throw;
            }
        }

        public void DeleteReview(Review review)
        {
            if (review == null)
            {
                throw new ArgumentNullException(nameof(review));
            }

            try
            {
                var reviewToDelete = context.Reviews
                    .Include(r => r.ReviewImages)
                    .FirstOrDefault(r => r.Id == review.Id && r.SellerId == review.SellerId && r.BuyerId == review.BuyerId);

                if (reviewToDelete == null)
                {
                    throw new KeyNotFoundException($"Review with ID {review.Id} not found.");
                }

                context.Reviews.Remove(reviewToDelete);
                context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"EF Core DeleteReview Error: {ex.InnerException?.Message ?? ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General DeleteReview Error: {ex.Message}");
                throw;
            }
        }
    }
}
