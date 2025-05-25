using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ReviewService;
using MarketMinds.Shared.Services.UserService;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace MarketMinds.Test.Services.ReviewService
{
    [TestFixture]
    public class ReviewsServiceTest
    {
        private ReviewProxyRepositoryMock repositoryMock;
        private UserServiceMock userServiceMock;
        private ReviewsService reviewsService;
        private User testSeller;
        private User testBuyer;

        [SetUp]
        public void Setup()
        {
            repositoryMock = new ReviewProxyRepositoryMock();
            userServiceMock = new UserServiceMock();

            // Create test users
            testSeller = new User { Id = 1, Username = "TestSeller", Email = "seller@test.com" };
            testBuyer = new User { Id = 2, Username = "TestBuyer", Email = "buyer@test.com" };

            userServiceMock.SetupUsers(new List<User> { testSeller, testBuyer });

            // Setup the service with mocks
            reviewsService = new TestableReviewsService(repositoryMock, userServiceMock, null);
        }

        [Test]
        public void GetReviewsBySeller_WithValidSeller_ReturnsCorrectReviews()
        {
            // Arrange
            var reviews = new List<Review>
            {
                new Review { Id = 1, SellerId = testSeller.Id, BuyerId = testBuyer.Id, Description = "Great seller", Rating = 5 },
                new Review { Id = 2, SellerId = testSeller.Id, BuyerId = 3, Description = "Good experience", Rating = 4 }
            };
            repositoryMock.SetupSellerReviews(reviews);

            // Act
            var result = reviewsService.GetReviewsBySeller(testSeller);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Great seller", result[0].Description);
            Assert.AreEqual(5, result[0].Rating);
            Assert.AreEqual(testSeller.Username, result[0].SellerUsername);
        }

        [Test]
        public void GetReviewsBySeller_WithNullSeller_ReturnsEmptyCollection()
        {
            // Act
            var result = reviewsService.GetReviewsBySeller(null);

            // Assert
            Assert.IsEmpty(result);
        }

        [Test]
        public void GetReviewsByBuyer_WithValidBuyer_ReturnsCorrectReviews()
        {
            // Arrange
            var reviews = new List<Review>
            {
                new Review { Id = 1, SellerId = testSeller.Id, BuyerId = testBuyer.Id, Description = "Great buyer", Rating = 5 },
                new Review { Id = 2, SellerId = 3, BuyerId = testBuyer.Id, Description = "Good payment", Rating = 4 }
            };
            repositoryMock.SetupBuyerReviews(reviews);

            // Act
            var result = reviewsService.GetReviewsByBuyer(testBuyer);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Great buyer", result[0].Description);
            Assert.AreEqual(5, result[0].Rating);
            Assert.AreEqual(testBuyer.Username, result[0].BuyerUsername);
        }

        [Test]
        public void GetReviewsByBuyer_WithNullBuyer_ReturnsEmptyCollection()
        {
            // Act
            var result = reviewsService.GetReviewsByBuyer(null);

            // Assert
            Assert.IsEmpty(result);
        }

        [Test]
        public void AddReview_WithValidData_CreatesReview()
        {
            // Arrange
            string description = "Great product and fast shipping";
            double rating = 4.5;
            var images = new List<Image> { new Image { Id = 1, Url = "http://example.com/image.jpg" } };

            // Act
            reviewsService.AddReview(description, images, rating, testSeller, testBuyer);

            // Assert
            var sellerReviews = repositoryMock.GetCurrentSellerReviews();
            Assert.AreEqual(1, sellerReviews.Count);
            Assert.AreEqual(description, sellerReviews[0].Description);
            Assert.AreEqual(rating, sellerReviews[0].Rating);
            Assert.AreEqual(testSeller.Id, sellerReviews[0].SellerId);
            Assert.AreEqual(testBuyer.Id, sellerReviews[0].BuyerId);
        }

        [Test]
        public void AddReview_WithNullSeller_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                reviewsService.AddReview("Description", new List<Image>(), 4, null, testBuyer));
        }

        [Test]
        public void AddReview_WithNullBuyer_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                reviewsService.AddReview("Description", new List<Image>(), 4, testSeller, null));
        }

        [Test]
        public void AddReview_WithEmptyDescription_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                reviewsService.AddReview("", new List<Image>(), 4, testSeller, testBuyer));
        }

        [Test]
        public void AddReview_WithNullDescription_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                reviewsService.AddReview(null, new List<Image>(), 4, testSeller, testBuyer));
        }

        [Test]
        public void AddReview_WithRatingOutOfRange_ClampsRating()
        {
            // Arrange
            double tooHighRating = 6.0;

            // Act
            reviewsService.AddReview("Good product", new List<Image>(), tooHighRating, testSeller, testBuyer);

            // Assert
            var reviews = repositoryMock.GetCurrentSellerReviews();
            Assert.AreEqual(5.0, reviews[0].Rating); // Should be clamped to 5.0
        }

        [Test]
        public void EditReview_WithValidData_UpdatesReview()
        {
            // Arrange
            var initialReview = new Review 
            { 
                Id = 1, 
                SellerId = testSeller.Id, 
                BuyerId = testBuyer.Id,
                Description = "Initial description",
                Rating = 3.0
            };
            repositoryMock.SetupBuyerReviews(new List<Review> { initialReview });
            
            string newDescription = "Updated description";
            double newRating = 4.0;

            // Act
            reviewsService.EditReview(
                initialReview.Description,
                new List<Image>(),
                initialReview.Rating,
                initialReview.SellerId,
                initialReview.BuyerId,
                newDescription,
                newRating);

            // Assert
            var updatedReviews = repositoryMock.GetCurrentBuyerReviews();
            Assert.AreEqual(1, updatedReviews.Count);
            Assert.AreEqual(newDescription, updatedReviews[0].Description);
            Assert.AreEqual(newRating, updatedReviews[0].Rating);
        }

        [Test]
        public void EditReview_WithNonExistingReview_ThrowsArgumentException()
        {
            // Arrange
            repositoryMock.SetupBuyerReviews(new List<Review>());

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                reviewsService.EditReview("Non-existing", new List<Image>(), 3.0, 999, 999, "New Description", 4.0));
        }

        [Test]
        public void EditReview_WithEmptyNewDescription_ThrowsArgumentException()
        {
            // Arrange
            var initialReview = new Review 
            { 
                Id = 1, 
                SellerId = testSeller.Id, 
                BuyerId = testBuyer.Id,
                Description = "Initial description",
                Rating = 3.0
            };
            repositoryMock.SetupBuyerReviews(new List<Review> { initialReview });

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                reviewsService.EditReview(
                    initialReview.Description,
                    new List<Image>(),
                    initialReview.Rating,
                    initialReview.SellerId,
                    initialReview.BuyerId,
                    "",  // Empty description
                    4.0));
        }

        [Test]
        public void EditReview_WithInvalidSellerId_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                reviewsService.EditReview("Description", new List<Image>(), 3.0, 0, 1, "New Description", 4.0));
        }

        [Test]
        public void EditReview_WithInvalidBuyerId_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                reviewsService.EditReview("Description", new List<Image>(), 3.0, 1, 0, "New Description", 4.0));
        }

        [Test]
        public void DeleteReview_WithValidData_RemovesReview()
        {
            // Arrange
            var reviewToDelete = new Review 
            { 
                Id = 1, 
                SellerId = testSeller.Id, 
                BuyerId = testBuyer.Id,
                Description = "Review to delete",
                Rating = 3.0
            };
            repositoryMock.SetupBuyerReviews(new List<Review> { reviewToDelete });
            repositoryMock.SetupSellerReviews(new List<Review> { reviewToDelete });

            // Act
            reviewsService.DeleteReview(
                reviewToDelete.Description,
                new List<Image>(),
                reviewToDelete.Rating,
                reviewToDelete.SellerId,
                reviewToDelete.BuyerId);

            // Assert
            Assert.IsEmpty(repositoryMock.GetCurrentBuyerReviews());
            Assert.IsEmpty(repositoryMock.GetCurrentSellerReviews());
        }

        [Test]
        public void DeleteReview_WithNonExistingReview_ThrowsArgumentException()
        {
            // Arrange
            repositoryMock.SetupBuyerReviews(new List<Review>());

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                reviewsService.DeleteReview("Non-existing", new List<Image>(), 3.0, 999, 999));
        }

        [Test]
        public void DeleteReview_WithInvalidSellerId_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                reviewsService.DeleteReview("Description", new List<Image>(), 3.0, 0, 1));
        }

        [Test]
        public void DeleteReview_WithInvalidBuyerId_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                reviewsService.DeleteReview("Description", new List<Image>(), 3.0, 1, 0));
        }

        // Helper test class
        private class TestableReviewsService : ReviewsService
        {
            public TestableReviewsService(
                ReviewProxyRepositoryMock repositoryMock, 
                IUserService userService,
                User currentUser) 
                : base(null, userService, currentUser)
            {
                this.GetType().GetField("repository", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance)
                    .SetValue(this, repositoryMock);
            }
        }
    }
}
