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

            testSeller = new User { Id = 1, Username = "TestSeller", Email = "seller@test.com" };
            testBuyer = new User { Id = 2, Username = "TestBuyer", Email = "buyer@test.com" };

            userServiceMock.SetupUsers(new List<User> { testSeller, testBuyer });
            reviewsService = new TestableReviewsService(repositoryMock, userServiceMock, null);
        }

        [Test]
        public void GetReviewsBySeller_WithValidSeller_ReturnsCorrectReviews()
        {
            var reviews = new List<Review>
            {
                new Review { Id = 1, SellerId = testSeller.Id, BuyerId = testBuyer.Id, Description = "Great seller", Rating = 5 },
                new Review { Id = 2, SellerId = testSeller.Id, BuyerId = 3, Description = "Good experience", Rating = 4 }
            };
            repositoryMock.SetupSellerReviews(reviews);

            var result = reviewsService.GetReviewsBySeller(testSeller);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Description, Is.EqualTo("Great seller"));
            Assert.That(result[0].Rating, Is.EqualTo(5));
            Assert.That(result[0].SellerUsername, Is.EqualTo(testSeller.Username));
        }

        [Test]
        public void GetReviewsBySeller_WithNullSeller_ReturnsEmptyCollection()
        {
            var result = reviewsService.GetReviewsBySeller(null);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetReviewsByBuyer_WithValidBuyer_ReturnsCorrectReviews()
        {
            var reviews = new List<Review>
            {
                new Review { Id = 1, SellerId = testSeller.Id, BuyerId = testBuyer.Id, Description = "Great buyer", Rating = 5 },
                new Review { Id = 2, SellerId = 3, BuyerId = testBuyer.Id, Description = "Good payment", Rating = 4 }
            };
            repositoryMock.SetupBuyerReviews(reviews);

            var result = reviewsService.GetReviewsByBuyer(testBuyer);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Description, Is.EqualTo("Great buyer"));
            Assert.That(result[0].Rating, Is.EqualTo(5));
            Assert.That(result[0].BuyerUsername, Is.EqualTo(testBuyer.Username));
        }

        [Test]
        public void GetReviewsByBuyer_WithNullBuyer_ReturnsEmptyCollection()
        {
            var result = reviewsService.GetReviewsByBuyer(null);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void AddReview_WithValidData_CreatesReview()
        {
            string description = "Great product and fast shipping";
            double rating = 4.5;
            var images = new List<Image> { new Image { Id = 1, Url = "http://example.com/image.jpg" } };

            reviewsService.AddReview(description, images, rating, testSeller, testBuyer);

            var sellerReviews = repositoryMock.GetCurrentSellerReviews();
            Assert.That(sellerReviews.Count, Is.EqualTo(1));
            Assert.That(sellerReviews[0].Description, Is.EqualTo(description));
            Assert.That(sellerReviews[0].Rating, Is.EqualTo(rating));
            Assert.That(sellerReviews[0].SellerId, Is.EqualTo(testSeller.Id));
            Assert.That(sellerReviews[0].BuyerId, Is.EqualTo(testBuyer.Id));
        }

        [Test]
        public void AddReview_WithNullSeller_ThrowsArgumentNullException()
        {
            Assert.That(() =>
                reviewsService.AddReview("Description", new List<Image>(), 4, null, testBuyer),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void AddReview_WithNullBuyer_ThrowsArgumentNullException()
        {
            Assert.That(() =>
                reviewsService.AddReview("Description", new List<Image>(), 4, testSeller, null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void AddReview_WithEmptyDescription_ThrowsArgumentException()
        {
            Assert.That(() =>
                reviewsService.AddReview("", new List<Image>(), 4, testSeller, testBuyer),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void AddReview_WithNullDescription_ThrowsArgumentException()
        {
            Assert.That(() =>
                reviewsService.AddReview(null, new List<Image>(), 4, testSeller, testBuyer),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void AddReview_WithRatingOutOfRange_ClampsRating()
        {
            double tooHighRating = 6.0;

            reviewsService.AddReview("Good product", new List<Image>(), tooHighRating, testSeller, testBuyer);

            var reviews = repositoryMock.GetCurrentSellerReviews();
            Assert.That(reviews[0].Rating, Is.EqualTo(5.0));
        }

        [Test]
        public void EditReview_WithValidData_UpdatesReview()
        {
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

            reviewsService.EditReview(
                initialReview.Description,
                new List<Image>(),
                initialReview.Rating,
                initialReview.SellerId,
                initialReview.BuyerId,
                newDescription,
                newRating);

            var updatedReviews = repositoryMock.GetCurrentBuyerReviews();
            Assert.That(updatedReviews.Count, Is.EqualTo(1));
            Assert.That(updatedReviews[0].Description, Is.EqualTo(newDescription));
            Assert.That(updatedReviews[0].Rating, Is.EqualTo(newRating));
        }

        [Test]
        public void EditReview_WithNonExistingReview_ThrowsArgumentException()
        {
            repositoryMock.SetupBuyerReviews(new List<Review>());

            Assert.That(() =>
                reviewsService.EditReview("Non-existing", new List<Image>(), 3.0, 999, 999, "New Description", 4.0),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void EditReview_WithEmptyNewDescription_ThrowsArgumentException()
        {
            var initialReview = new Review
            {
                Id = 1,
                SellerId = testSeller.Id,
                BuyerId = testBuyer.Id,
                Description = "Initial description",
                Rating = 3.0
            };
            repositoryMock.SetupBuyerReviews(new List<Review> { initialReview });

            Assert.That(() =>
                reviewsService.EditReview(
                    initialReview.Description,
                    new List<Image>(),
                    initialReview.Rating,
                    initialReview.SellerId,
                    initialReview.BuyerId,
                    "",  // Empty new description
                    4.0),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void EditReview_WithInvalidSellerId_ThrowsArgumentException()
        {
            Assert.That(() =>
                reviewsService.EditReview("Description", new List<Image>(), 3.0, 0, 1, "New Description", 4.0),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void EditReview_WithInvalidBuyerId_ThrowsArgumentException()
        {
            Assert.That(() =>
                reviewsService.EditReview("Description", new List<Image>(), 3.0, 1, 0, "New Description", 4.0),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void DeleteReview_WithValidData_RemovesReview()
        {
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

            reviewsService.DeleteReview(
                reviewToDelete.Description,
                new List<Image>(),
                reviewToDelete.Rating,
                reviewToDelete.SellerId,
                reviewToDelete.BuyerId);

            Assert.That(repositoryMock.GetCurrentBuyerReviews(), Is.Empty);
            Assert.That(repositoryMock.GetCurrentSellerReviews(), Is.Empty);
        }

        [Test]
        public void DeleteReview_WithNonExistingReview_ThrowsArgumentException()
        {
            repositoryMock.SetupBuyerReviews(new List<Review>());

            Assert.That(() =>
                reviewsService.DeleteReview("Non-existing", new List<Image>(), 3.0, 999, 999),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void DeleteReview_WithInvalidSellerId_ThrowsArgumentException()
        {
            Assert.That(() =>
                reviewsService.DeleteReview("Description", new List<Image>(), 3.0, 0, 1),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void DeleteReview_WithInvalidBuyerId_ThrowsArgumentException()
        {
            Assert.That(() =>
                reviewsService.DeleteReview("Description", new List<Image>(), 3.0, 1, 0),
                Throws.TypeOf<ArgumentException>());
        }

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
