using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketMinds.Test.Services.ReviewService
{
    internal class ReviewRepositoryMock : IReviewRepository
    {
        public ObservableCollection<Review> Reviews { get; private set; }

        public ReviewRepositoryMock()
        {
            Reviews = new ObservableCollection<Review>();
        }

        public void CreateReview(Review review)
        {
            Reviews.Add(review);
        }

        public void EditReview(Review review, double rating, string description)
        {
            Review reviewToEdit = Reviews.FirstOrDefault(r =>
                r.Description == review.Description &&
                r.SellerId == review.SellerId &&
                r.BuyerId == review.BuyerId);

            if (reviewToEdit != null)
            {
                reviewToEdit.Rating = rating;
                reviewToEdit.Description = description;
            }
        }

        public void DeleteReview(Review review)
        {
            Review reviewToRemove = Reviews.FirstOrDefault(r =>
                r.Description == review.Description &&
                r.SellerId == review.SellerId &&
                r.BuyerId == review.BuyerId);

            if (reviewToRemove != null)
            {
                Reviews.Remove(reviewToRemove);
            }
        }

        public ObservableCollection<Review> GetAllReviewsByBuyer(User buyer)
        {
            var buyerReviews = Reviews.Where(r => r.BuyerId == buyer.Id);
            return new ObservableCollection<Review>(buyerReviews);
        }

        public ObservableCollection<Review> GetAllReviewsBySeller(User seller)
        {
            var sellerReviews = Reviews.Where(r => r.SellerId == seller.Id);
            return new ObservableCollection<Review>(sellerReviews);
        }
    }
}
