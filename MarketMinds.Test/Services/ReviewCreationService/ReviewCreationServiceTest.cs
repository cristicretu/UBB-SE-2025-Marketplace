using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ReviewCreationService;
using MarketMinds.Shared.Services.ReviewService;
using MarketMinds.Shared.Services.ImagineUploadService;
using NUnit.Framework;
using System;
using System.Collections.Generic;


namespace MarketMinds.Test.Services.ReviewCreationService
{
    [TestFixture]
    public class ReviewCreationServiceTest
    {
        private ReviewsService _reviewsService;
        private IImageUploadService _imageService;
        private IReviewCreationService _reviewCreationService;

        [SetUp]
        public void Setup()
        {
            _reviewsService = new ReviewsService(null, null, null); // Directly initialize ReviewsService
            _imageService = new ImageUploadService(); // Directly initialize ImageUploadService
            _reviewCreationService = new MarketMinds.Shared.Services.ReviewCreationService.ReviewCreationService(_reviewsService);
        }

        [Test]
        public void CreateReview_ShouldAddReviewAndReturnReview()
        {
            // Arrange
            var description = "Great product!";
            var images = new List<Image> { new Image("image1.jpg"), new Image("image2.jpg") };
            var rating = 4.5;
            var seller = new User(1, "Seller", "seller@mail.com", "token");
            var buyer = new User(2, "Buyer", "buyer@mail.com", "token");

            // Act
            var result = _reviewCreationService.CreateReview(description, images, rating, seller, buyer);

            // Assert
            Assert.That(result.Description, Is.EqualTo(description));
            Assert.That(result.Images, Is.EqualTo(images));
            Assert.That(result.Rating, Is.EqualTo(rating));
            Assert.That(result.SellerId, Is.EqualTo(seller.Id));
            Assert.That(result.BuyerId, Is.EqualTo(buyer.Id));
        }

        [Test]
        public void CreateReview_ShouldThrowException_WhenSellerOrBuyerIsNull()
        {
            // Arrange
            var description = "Great product!";
            var images = new List<Image>();
            var rating = 4.5;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _reviewCreationService.CreateReview(description, images, rating, null, new User(1, "Buyer", "buyer@mail.com", "token")));
            Assert.Throws<ArgumentException>(() => _reviewCreationService.CreateReview(description, images, rating, new User(1, "Seller", "seller@mail.com", "token"), null));
        }

        [Test]
        public void UpdateReview_ShouldUpdateReview()
        {
            // Arrange
            var currentReview = new Review(1, "Old description", new List<Image>(), 3.5, 1, 2);
            var newDescription = "Updated description";
            var newRating = 4.5;

            // Act
            _reviewCreationService.UpdateReview(currentReview, newDescription, newRating);

            // Assert
            // No exception means the test passed
        }

        [Test]
        public void UpdateReview_ShouldThrowException_WhenCurrentReviewIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _reviewCreationService.UpdateReview(null, "New description", 4.5));
        }

        [Test]
        public void EditReview_ShouldEditReview()
        {
            // Arrange
            var review = new Review(1, "Old description", new List<Image>(), 3.5, 1, 2);
            var newDescription = "Updated description";
            var newRating = 4.5;

            // Act
            _reviewCreationService.EditReview(review, newDescription, newRating);

            // Assert
            // No exception means the test passed
        }

        [Test]
        public void EditReview_ShouldThrowException_WhenReviewIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _reviewCreationService.EditReview(null, "New description", 4.5));
        }

        [Test]
        public void DeleteReview_ShouldDeleteReview()
        {
            // Arrange
            var review = new Review(1, "Description", new List<Image>(), 3.5, 1, 2);

            // Act
            _reviewCreationService.DeleteReview(review);

            // Assert
            // No exception means the test passed
        }

        [Test]
        public void DeleteReview_ShouldThrowException_WhenReviewIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _reviewCreationService.DeleteReview(null));
        }

        [Test]
        public void ParseImagesString_ShouldReturnParsedImages()
        {
            // Arrange
            var imagesString = "image1.jpg,image2.jpg";

            // Act
            var result = _reviewCreationService.ParseImagesString(imagesString);

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void FormatImagesString_ShouldReturnFormattedString()
        {
            // Arrange
            var images = new List<Image> { new Image("image1.jpg"), new Image("image2.jpg") };

            // Act
            var result = _reviewCreationService.FormatImagesString(images);

            // Assert
            Assert.That(result, Is.EqualTo("image1.jpg,image2.jpg"));
        }
    }
}
