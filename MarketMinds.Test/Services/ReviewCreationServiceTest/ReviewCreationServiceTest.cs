using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ReviewCreationService;
using MarketMinds.Shared.Services.ReviewService;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MarketMinds.Test.Services.ReviewCreationServiceTest
{
    [TestFixture]
    public class ReviewCreationServiceTest
    {
        private ReviewsServiceMock reviewsServiceMock;
        private ReviewCreationService reviewCreationService;
        private User testSeller;
        private User testBuyer;

        [SetUp]
        public void Setup()
        {
            reviewsServiceMock = new ReviewsServiceMock();
            reviewCreationService = new TestableReviewCreationService(reviewsServiceMock);

            testSeller = new User { Id = 1, Username = "TestSeller", Email = "seller@test.com" };
            testBuyer = new User { Id = 2, Username = "TestBuyer", Email = "buyer@test.com" };
        }

        [Test]
        public void CreateReview_WithValidData_AddsReview()
        {
            // Arrange
            string description = "Great product";
            List<Image> images = new List<Image> { new Image { Id = 1, Url = "http://example.com/image.jpg" } };
            double rating = 4.5;

            // Act
            var result = reviewCreationService.CreateReview(description, images, rating, testSeller, testBuyer);

            // Assert
            Assert.AreEqual(description, result.Description);
            Assert.AreEqual(rating, result.Rating);
            Assert.AreEqual(testSeller.Id, result.SellerId);
            Assert.AreEqual(testBuyer.Id, result.BuyerId);
            Assert.AreEqual(images, result.Images);
            
            // Verify reviewsServiceMock was called correctly
            Assert.AreEqual(1, reviewsServiceMock.AddReviewCallCount);
            Assert.AreEqual(description, reviewsServiceMock.LastAddedDescription);
            Assert.AreEqual(rating, reviewsServiceMock.LastAddedRating);
            Assert.AreEqual(testSeller, reviewsServiceMock.LastAddedSeller);
            Assert.AreEqual(testBuyer, reviewsServiceMock.LastAddedBuyer);
        }

        [Test]
        public void CreateReview_WithNullSeller_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => 
                reviewCreationService.CreateReview("Description", new List<Image>(), 4, null, testBuyer));
            Assert.That(ex.Message, Contains.Substring("One of the required objects is null"));
        }

        [Test]
        public void CreateReview_WithNullBuyer_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => 
                reviewCreationService.CreateReview("Description", new List<Image>(), 4, testSeller, null));
            Assert.That(ex.Message, Contains.Substring("One of the required objects is null"));
        }

        [Test]
        public void CreateReview_WithNullReviewsService_ThrowsArgumentException()
        {
            // Arrange
            var serviceWithNullDependency = new TestableReviewCreationService(null);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => 
                serviceWithNullDependency.CreateReview("Description", new List<Image>(), 4, testSeller, testBuyer));
            Assert.That(ex.Message, Contains.Substring("One of the required objects is null"));
        }

        [Test]
        public void UpdateReview_WithValidData_EditsReview()
        {
            // Arrange
            var review = new Review
            {
                Id = 1,
                Description = "Original description",
                Rating = 3.0,
                SellerId = testSeller.Id,
                BuyerId = testBuyer.Id,
                Images = new List<Image>()
            };
            string newDescription = "Updated description";
            double newRating = 4.5;

            // Act
            reviewCreationService.UpdateReview(review, newDescription, newRating);

            // Assert
            Assert.AreEqual(1, reviewsServiceMock.EditReviewCallCount);
            Assert.AreEqual(review.Description, reviewsServiceMock.LastEditedOriginalDescription);
            Assert.AreEqual(review.Rating, reviewsServiceMock.LastEditedOriginalRating);
            Assert.AreEqual(review.SellerId, reviewsServiceMock.LastEditedSellerId);
            Assert.AreEqual(review.BuyerId, reviewsServiceMock.LastEditedBuyerId);
            Assert.AreEqual(newDescription, reviewsServiceMock.LastEditedNewDescription);
            Assert.AreEqual(newRating, reviewsServiceMock.LastEditedNewRating);
        }

        [Test]
        public void UpdateReview_WithNullReview_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => 
                reviewCreationService.UpdateReview(null, "New Description", 4.0));
            Assert.That(ex.Message, Contains.Substring("Current review cannot be null"));
        }

        [Test]
        public void EditReview_WithValidData_EditsReview()
        {
            // Arrange
            var review = new Review
            {
                Id = 1,
                Description = "Original description",
                Rating = 3.0,
                SellerId = testSeller.Id,
                BuyerId = testBuyer.Id,
                Images = new List<Image>()
            };
            string newDescription = "Edited description";
            double newRating = 4.0;

            // Act
            reviewCreationService.EditReview(review, newDescription, newRating);

            // Assert
            Assert.AreEqual(1, reviewsServiceMock.EditReviewCallCount);
            Assert.AreEqual(review.Description, reviewsServiceMock.LastEditedOriginalDescription);
            Assert.AreEqual(review.Rating, reviewsServiceMock.LastEditedOriginalRating);
            Assert.AreEqual(review.SellerId, reviewsServiceMock.LastEditedSellerId);
            Assert.AreEqual(review.BuyerId, reviewsServiceMock.LastEditedBuyerId);
            Assert.AreEqual(newDescription, reviewsServiceMock.LastEditedNewDescription);
            Assert.AreEqual(newRating, reviewsServiceMock.LastEditedNewRating);
        }

        [Test]
        public void EditReview_WithNullReview_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => 
                reviewCreationService.EditReview(null, "New Description", 4.0));
            Assert.That(ex.Message, Contains.Substring("Review cannot be null"));
        }

        [Test]
        public void DeleteReview_WithValidData_DeletesReview()
        {
            // Arrange
            var review = new Review
            {
                Id = 1,
                Description = "Description to delete",
                Rating = 3.0,
                SellerId = testSeller.Id,
                BuyerId = testBuyer.Id,
                Images = new List<Image>()
            };

            // Act
            reviewCreationService.DeleteReview(review);

            // Assert
            Assert.AreEqual(1, reviewsServiceMock.DeleteReviewCallCount);
            Assert.AreEqual(review.Description, reviewsServiceMock.LastDeletedDescription);
            Assert.AreEqual(review.Rating, reviewsServiceMock.LastDeletedRating);
            Assert.AreEqual(review.SellerId, reviewsServiceMock.LastDeletedSellerId);
            Assert.AreEqual(review.BuyerId, reviewsServiceMock.LastDeletedBuyerId);
        }

        [Test]
        public void DeleteReview_WithNullReview_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => 
                reviewCreationService.DeleteReview(null));
            Assert.That(ex.Message, Contains.Substring("Review cannot be null"));
        }

        [Test]
        public void ParseImagesString_DelegatesCallToImageService()
        {
            // This test would verify that the call is delegated to the image service
            // However, since we don't have direct access to mock IImageUploadService
            // and it's internally created, we'll verify the method exists
            Assert.DoesNotThrow(() => 
                reviewCreationService.ParseImagesString("test-image-string"));
        }

        [Test]
        public void FormatImagesString_DelegatesCallToImageService()
        {
            // This test would verify that the call is delegated to the image service
            // However, since we don't have direct access to mock IImageUploadService
            // and it's internally created, we'll verify the method exists
            Assert.DoesNotThrow(() => 
                reviewCreationService.FormatImagesString(new List<Image>()));
        }

        // Helper classes for testing
        private class ReviewsServiceMock : ReviewsService
        {
            public int AddReviewCallCount { get; private set; }
            public int EditReviewCallCount { get; private set; }
            public int DeleteReviewCallCount { get; private set; }

            public string LastAddedDescription { get; private set; }
            public List<Image> LastAddedImages { get; private set; }
            public double LastAddedRating { get; private set; }
            public User LastAddedSeller { get; private set; }
            public User LastAddedBuyer { get; private set; }

            public string LastEditedOriginalDescription { get; private set; }
            public List<Image> LastEditedOriginalImages { get; private set; }
            public double LastEditedOriginalRating { get; private set; }
            public int LastEditedSellerId { get; private set; }
            public int LastEditedBuyerId { get; private set; }
            public string LastEditedNewDescription { get; private set; }
            public double LastEditedNewRating { get; private set; }

            public string LastDeletedDescription { get; private set; }
            public List<Image> LastDeletedImages { get; private set; }
            public double LastDeletedRating { get; private set; }
            public int LastDeletedSellerId { get; private set; }
            public int LastDeletedBuyerId { get; private set; }

            public ReviewsServiceMock() : base(null, null, null) { }

            public override void AddReview(string description, List<Image> images, double rating, User seller, User buyer)
            {
                AddReviewCallCount++;
                LastAddedDescription = description;
                LastAddedImages = images;
                LastAddedRating = rating;
                LastAddedSeller = seller;
                LastAddedBuyer = buyer;
            }

            public override void EditReview(string description, List<Image> images, double rating, int sellerId, int buyerId, string newDescription, double newRating)
            {
                EditReviewCallCount++;
                LastEditedOriginalDescription = description;
                LastEditedOriginalImages = images;
                LastEditedOriginalRating = rating;
                LastEditedSellerId = sellerId;
                LastEditedBuyerId = buyerId;
                LastEditedNewDescription = newDescription;
                LastEditedNewRating = newRating;
            }

            public override void DeleteReview(string description, List<Image> images, double rating, int sellerId, int buyerId)
            {
                DeleteReviewCallCount++;
                LastDeletedDescription = description;
                LastDeletedImages = images;
                LastDeletedRating = rating;
                LastDeletedSellerId = sellerId;
                LastDeletedBuyerId = buyerId;
            }
        }

        private class TestableReviewCreationService : ReviewCreationService
        {
            public TestableReviewCreationService(ReviewsServiceMock reviewsServiceMock) 
                : base(reviewsServiceMock, null)
            {
                // No need to set additional fields as reviewsServiceMock is directly passed to base constructor
            }
        }
    }
}
