using MarketMinds.Shared.Models;
using MarketMinds.Shared.ProxyRepository;
using MarketMinds.Shared.Services.UserService;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace MarketMinds.Test.Services.ReviewService
{
    public class ReviewProxyRepositoryMock : ReviewProxyRepository
    {
        private List<Review> sellerReviews;
        private List<Review> buyerReviews;
        private int nextId = 1;

        public ReviewProxyRepositoryMock() : base(null)
        {
            sellerReviews = new List<Review>();
            buyerReviews = new List<Review>();
        }

        public void SetupSellerReviews(List<Review> reviews)
        {
            sellerReviews = reviews;
        }

        public void SetupBuyerReviews(List<Review> reviews)
        {
            buyerReviews = reviews;
        }

        public override string GetReviewsBySellerRaw(int sellerId)
        {
            var filtered = sellerReviews.FindAll(r => r.SellerId == sellerId);
            return JsonSerializer.Serialize(filtered);
        }

        public override string GetReviewsByBuyerRaw(int buyerId)
        {
            var filtered = buyerReviews.FindAll(r => r.BuyerId == buyerId);
            return JsonSerializer.Serialize(filtered);
        }

        public override string CreateReviewRaw(Review reviewToCreate)
        {
            reviewToCreate.Id = nextId++;
            sellerReviews.Add(reviewToCreate);
            buyerReviews.Add(reviewToCreate);
            return JsonSerializer.Serialize(reviewToCreate);
        }

        public override string EditReviewRaw(Review reviewToEdit)
        {
            // Update in seller reviews
            var sellerReview = sellerReviews.Find(r => r.Id == reviewToEdit.Id);
            if (sellerReview != null)
            {
                sellerReviews.Remove(sellerReview);
                sellerReviews.Add(reviewToEdit);
            }

            // Update in buyer reviews
            var buyerReview = buyerReviews.Find(r => r.Id == reviewToEdit.Id);
            if (buyerReview != null)
            {
                buyerReviews.Remove(buyerReview);
                buyerReviews.Add(reviewToEdit);
            }

            return JsonSerializer.Serialize(reviewToEdit);
        }

        public override string DeleteReviewRaw(object deleteRequest)
        {
            var reviewId = (int)deleteRequest.GetType().GetProperty("ReviewId").GetValue(deleteRequest);
            var sellerId = (int)deleteRequest.GetType().GetProperty("SellerId").GetValue(deleteRequest);
            var buyerId = (int)deleteRequest.GetType().GetProperty("BuyerId").GetValue(deleteRequest);

            // Remove from seller reviews
            var sellerReview = sellerReviews.Find(r => r.Id == reviewId && r.SellerId == sellerId && r.BuyerId == buyerId);
            if (sellerReview != null)
            {
                sellerReviews.Remove(sellerReview);
            }

            // Remove from buyer reviews
            var buyerReview = buyerReviews.Find(r => r.Id == reviewId && r.SellerId == sellerId && r.BuyerId == buyerId);
            if (buyerReview != null)
            {
                buyerReviews.Remove(buyerReview);
            }

            return "{}";
        }

        // Helper methods to get current state (for assertions)
        public List<Review> GetCurrentSellerReviews()
        {
            return sellerReviews;
        }

        public List<Review> GetCurrentBuyerReviews()
        {
            return buyerReviews;
        }
    }

    public class UserServiceMock : IUserService
    {
        private Dictionary<int, User> users = new Dictionary<int, User>();

        public void SetupUsers(List<User> usersList)
        {
            users.Clear();
            foreach (var user in usersList)
            {
                users[user.Id] = user;
            }
        }

        public Task<User> GetUserByIdAsync(int userId)
        {
            if (users.TryGetValue(userId, out var user))
            {
                return Task.FromResult(user);
            }
            return Task.FromResult<User>(null);
        }

        // Add more implementations as needed for IUserService
        public Task<string> GetJwtTokenAsync(string email, string password)
        {
            throw new NotImplementedException();
        }

        public Task<User> GetCurrentUserAsync()
        {
            throw new NotImplementedException();
        }

        public Task<User> RegisterUserAsync(string username, string email, string password, string userType)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CheckEmailExistsAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CheckUsernameExistsAsync(string username)
        {
            throw new NotImplementedException();
        }

        public Task<User> GetUserByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ChangePasswordAsync(string email, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RequestPasswordResetAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            throw new NotImplementedException();
        }
    }
}
