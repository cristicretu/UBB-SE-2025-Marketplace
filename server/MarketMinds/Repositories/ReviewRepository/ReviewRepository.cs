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
                context.Reviews.Add(review);
                context.SaveChanges();
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

                // Update images
                if (review.ReviewImages != null)
                {
                    // Remove existing images
                    context.ReviewImages.RemoveRange(reviewToEdit.ReviewImages);
                    
                    // Add new images
                    reviewToEdit.ReviewImages = review.ReviewImages;
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
