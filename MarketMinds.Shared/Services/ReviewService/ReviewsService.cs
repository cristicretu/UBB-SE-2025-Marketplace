using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Text.Json;
using MarketMinds.Shared.Models;
using Microsoft.Extensions.Configuration;
using MarketMinds.Shared.ProxyRepository;
using MarketMinds.Shared.Services.Interfaces;
using MarketMinds.Shared.Services.UserService;

namespace MarketMinds.Shared.Services.ReviewService
{
    public class ReviewsService : IReviewsService
    {
        private readonly ReviewProxyRepository repository;
        private readonly IUserService userService;
        private readonly User currentUser;
        private readonly JsonSerializerOptions jsonOptions;
        private Dictionary<int, string> userCache = new Dictionary<int, string>();

        public ReviewsService(IConfiguration configuration, IUserService userService, User currentUser = null)
        {
            repository = new ReviewProxyRepository(configuration);
            this.userService = userService;
            this.currentUser = currentUser;

            // If userService is null, create a new instance with the configuration
            if (this.userService == null)
            {
                this.userService = new MarketMinds.Shared.Services.UserService.UserService(configuration);
            }

            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        private async Task<string> GetUsernameForIdAsync(int userId)
        {
            if (currentUser != null && currentUser.Id == userId)
            {
                return currentUser.Username;
            }

            if (userCache.ContainsKey(userId))
            {
                return userCache[userId];
            }

            try
            {
                // Ensure userService is not null before calling it
                if (userService == null)
                {
                    string fallbackUsernameLocal = $"User #{userId}"; // Renamed to avoid conflict
                    userCache[userId] = fallbackUsernameLocal;
                    return fallbackUsernameLocal;
                }

                var user = await userService.GetUserByIdAsync(userId);
                if (user != null)
                {
                    userCache[userId] = user.Username;
                    return user.Username;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting username for ID {userId}: {ex.Message}");
            }

            string fallbackUsername = $"User #{userId}";
            userCache[userId] = fallbackUsername;
            return fallbackUsername;
        }

        public ObservableCollection<Review> GetReviewsBySeller(User seller)
        {
            if (seller == null)
            {
                return new ObservableCollection<Review>();
            }

            try
            {
                var jsonResponse = repository.GetReviewsBySellerRaw(seller.Id);
                var sharedReviews = JsonSerializer.Deserialize<List<Review>>(jsonResponse, jsonOptions);
                var domainReviews = new List<Review>();

                if (sharedReviews != null)
                {
                    foreach (var sharedReview in sharedReviews)
                    {
                        var domainReview = ConvertToDomainReview(sharedReview);
                        domainReview.SellerUsername = seller.Username;
                        // Fetch buyer username asynchronously
                        Task.Run(async () =>
                        {
                            domainReview.BuyerUsername = await GetUsernameForIdAsync(domainReview.BuyerId);
                        }).Wait();
                        domainReviews.Add(domainReview);
                    }
                }

                return new ObservableCollection<Review>(domainReviews);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting reviews by seller: {ex.Message}");
                return new ObservableCollection<Review>();
            }
        }

        public ObservableCollection<Review> GetReviewsByBuyer(User buyer)
        {
            if (buyer == null)
            {
                return new ObservableCollection<Review>();
            }

            try
            {
                var jsonResponse = repository.GetReviewsByBuyerRaw(buyer.Id);
                var sharedReviews = JsonSerializer.Deserialize<List<Review>>(jsonResponse, jsonOptions);
                var domainReviews = new List<Review>();

                if (sharedReviews != null)
                {
                    foreach (var sharedReview in sharedReviews)
                    {
                        var domainReview = ConvertToDomainReview(sharedReview);
                        domainReview.BuyerUsername = buyer.Username;
                        // Fetch seller username asynchronously
                        Task.Run(async () =>
                        {
                            domainReview.SellerUsername = await GetUsernameForIdAsync(domainReview.SellerId);
                        }).Wait();
                        domainReviews.Add(domainReview);
                    }
                }

                return new ObservableCollection<Review>(domainReviews);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting reviews by buyer: {ex.Message}");
                return new ObservableCollection<Review>();
            }
        }

        public void AddReview(string description, List<Image> images, double rating, User seller, User buyer)
        {
            if (seller == null)
            {
                throw new ArgumentNullException(nameof(seller), "Seller cannot be null.");
            }

            if (buyer == null)
            {
                throw new ArgumentNullException(nameof(buyer), "Buyer cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("Review description cannot be null or empty.", nameof(description));
            }

            try
            {
                // Ensure rating is within the expected range (0-5)
                double validRating = Math.Max(0, Math.Min(5, rating));

                // Create a Review object
                var reviewToCreate = new MarketMinds.Shared.Models.Review
                {
                    Description = description,
                    Images = ConvertToSharedImages(images ?? new List<Image>()),
                    Rating = validRating,
                    SellerId = seller.Id,
                    BuyerId = buyer.Id
                };

                repository.CreateReviewRaw(reviewToCreate);
                userCache.Clear();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding review: {ex.Message}");
                throw;
            }
        }

        public void EditReview(string description, List<Image> images, double rating, int sellerId, int buyerId, string newDescription, double newRating)
        {
            if (string.IsNullOrWhiteSpace(newDescription))
            {
                throw new ArgumentException("New review description cannot be null or empty.", nameof(newDescription));
            }

            if (sellerId <= 0)
            {
                throw new ArgumentException("Invalid seller ID.", nameof(sellerId));
            }

            if (buyerId <= 0)
            {
                throw new ArgumentException("Invalid buyer ID.", nameof(buyerId));
            }

            try
            {
                // Ensure the new rating is within the expected range (0-5)
                double validRating = Math.Max(0, Math.Min(5, newRating));

                // Get the review ID from the repository
                var reviews = repository.GetReviewsByBuyerRaw(buyerId);
                var sharedReviews = JsonSerializer.Deserialize<List<MarketMinds.Shared.Models.Review>>(reviews, jsonOptions);
                var reviewToEdit = sharedReviews?.FirstOrDefault(r => r.SellerId == sellerId && r.BuyerId == buyerId);

                if (reviewToEdit == null)
                {
                    throw new ArgumentException("Review not found.");
                }

                // Create a shared Review object with updated values
                var updatedReview = new MarketMinds.Shared.Models.Review
                {
                    Id = reviewToEdit.Id,
                    Description = newDescription,
                    Images = images ?? new List<Image>(),
                    Rating = validRating,
                    SellerId = sellerId,
                    BuyerId = buyerId
                };

                repository.EditReviewRaw(updatedReview);
                userCache.Clear();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error editing review: {ex.Message}");
                throw;
            }
        }

        public void DeleteReview(string description, List<Image> images, double rating, int sellerId, int buyerId)
        {
            if (sellerId <= 0)
            {
                throw new ArgumentException("Invalid seller ID.", nameof(sellerId));
            }

            if (buyerId <= 0)
            {
                throw new ArgumentException("Invalid buyer ID.", nameof(buyerId));
            }

            try
            {
                // Get the review ID from the repository
                var reviews = repository.GetReviewsByBuyerRaw(buyerId);
                var sharedReviews = JsonSerializer.Deserialize<List<MarketMinds.Shared.Models.Review>>(reviews, jsonOptions);
                var reviewToDelete = sharedReviews?.FirstOrDefault(r => r.SellerId == sellerId && r.BuyerId == buyerId);

                if (reviewToDelete == null)
                {
                    throw new ArgumentException("Review not found.");
                }

                var deleteRequest = new
                {
                    ReviewId = reviewToDelete.Id,
                    SellerId = sellerId,
                    BuyerId = buyerId
                };

                repository.DeleteReviewRaw(deleteRequest);
                userCache.Clear();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting review: {ex.Message}");
                throw;
            }
        }

        // Helper methods to convert between domain and shared models
        private MarketMinds.Shared.Models.User ConvertToSharedUser(User domainUser)
        {
            if (domainUser == null)
            {
                return null;
            }

            var user = new MarketMinds.Shared.Models.User
            {
                Id = domainUser.Id,
                Username = domainUser.Username,
                Email = domainUser.Email,
                PasswordHash = domainUser.PasswordHash,
                UserType = domainUser.UserType,
                Balance = domainUser.Balance,
            };

            if (domainUser.GetType().GetProperty("Rating") != null)
            {
                user.Rating = domainUser.Rating;
            }

            return user;
        }

        private Review ConvertToDomainReview(MarketMinds.Shared.Models.Review sharedReview)
        {
            if (sharedReview == null)
            {
                return null;
            }

            return new Review
            {
                Id = sharedReview.Id,
                Description = sharedReview.Description,
                Images = ConvertToDomainImages(sharedReview.Images),
                Rating = sharedReview.Rating,
                SellerId = sharedReview.SellerId,
                BuyerId = sharedReview.BuyerId,
                SellerUsername = string.Empty,  // Will be populated later
                BuyerUsername = string.Empty // Will be populated later
            };
        }

        private List<Image> ConvertToDomainImages(List<MarketMinds.Shared.Models.Image> sharedImages)
        {
            return sharedImages?.Select(img => new Image
            {
                Id = img.Id,
                Url = img.Url
            }).ToList() ?? new List<Image>();
        }

        private List<MarketMinds.Shared.Models.Image> ConvertToSharedImages(List<Image> domainImages)
        {
            return domainImages?.Select(img => new MarketMinds.Shared.Models.Image
            {
                Id = img.Id,
                Url = img.Url
            }).ToList() ?? new List<MarketMinds.Shared.Models.Image>();
        }
    }
}
