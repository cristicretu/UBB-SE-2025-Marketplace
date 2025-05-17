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
                review.Rating = rating;
                review.Description = description;
                context.Entry(review).State = EntityState.Modified;
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
                context.Reviews.Remove(review);
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
