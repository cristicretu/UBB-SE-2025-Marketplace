using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ReviewService;
using MarketMinds.Shared.Services.UserService;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Test.Services.ReviewService
{
    [TestFixture]
    internal class ReviewServiceTest
    {
        // Constants for User IDs
        private const string SELLER_1_ID = "1";
        private const string SELLER_2_ID = "2";
        private const string BUYER_ID = "3";
        private const string BUYER_1_ID = "2";
        private const string BUYER_2_ID = "3";

        // Constants for User names
        private const string SELLER_1_NAME = "Marcel";
        private const string SELLER_2_NAME = "Dorel";
        private const string BUYER_NAME = "Sorin";
        private const string SELLER_LUCA_NAME = "Luca";
        private const string BUYER_CRISTI_NAME = "Cristi";

        // Constants for email addresses
        private const string SELLER_1_EMAIL = "marcel@mail.com";
        private const string SELLER_2_EMAIL = "dorel@mail.com";
        private const string BUYER_EMAIL = "sorin@mail.com";
        private const string SELLER_LUCA_EMAIL = "luca@mail.com";
        private const string BUYER_CRISTI_EMAIL = "cristi@mail.com";

        // Constants for Review IDs
        private const int REVIEW_1_ID = 1;
        private const int REVIEW_2_ID = 2;
        private const int REVIEW_3_ID = 3;

        // Constants for Review descriptions
        private const string REVIEW_1_DESCRIPTION = "Review 1";
        private const string REVIEW_2_DESCRIPTION = "Review 2";
        private const string REVIEW_3_DESCRIPTION = "Review 3";
        private const string LOVE_TESTING_DESCRIPTION = "I love testing.";
        private const string LOVE_TESTING_NOT_DESCRIPTION = "I love testing not.";

        // Constants for Review ratings
        private const float RATING_4_5 = 4.5f;
        private const float RATING_5_0 = 5.0f;
        private const float RATING_3_5 = 3.5f;
        private const float RATING_4_8 = 4.8f;
        private const float RATING_3_0 = 3.0f;
        private const float RATING_4_0 = 4.0f;

        // Constants for expected counts
        private const int EXPECTED_SINGLE_ITEM = 1;
        private const int EXPECTED_TWO_ITEMS = 2;
        private const int EXPECTED_ZERO_ITEMS = 0;

        private ReviewRepositoryMock _mockRepository;
        private IReviewsService _reviewsService;

        [Test]
        public void GetAllReviewsBySeller_ShouldReturnOnlySellerReviews()
        {

            var mockConfiguration = new Mock<IConfiguration>();
            var mockUserService = new Mock<IUserService>();
            var currentUser = new User("1", "TestUser", "test@mail.com");

            // Arrange
            _mockRepository = new ReviewRepositoryMock();
            _reviewsService = new ReviewsService(mockConfiguration.Object, mockUserService.Object, currentUser);

            var seller1 = new User(SELLER_1_ID, SELLER_1_NAME, SELLER_1_EMAIL);
            var seller2 = new User(SELLER_2_ID, SELLER_2_NAME, SELLER_2_EMAIL);
            var buyer = new User(BUYER_ID, BUYER_NAME, BUYER_EMAIL);

            _mockRepository.CreateReview(new Review(REVIEW_1_ID, REVIEW_1_DESCRIPTION, new List<Image>(), RATING_4_5, seller1.Id, buyer.Id));
            _mockRepository.CreateReview(new Review(REVIEW_2_ID, REVIEW_2_DESCRIPTION, new List<Image>(), RATING_5_0, seller1.Id, buyer.Id));
            _mockRepository.CreateReview(new Review(REVIEW_3_ID, REVIEW_3_DESCRIPTION, new List<Image>(), RATING_3_5, seller2.Id, buyer.Id));

            // Act
            var seller1Reviews = _reviewsService.GetReviewsBySeller(seller1);

            // Assert
            Assert.That(seller1Reviews.Count, Is.EqualTo(EXPECTED_TWO_ITEMS));
            Assert.That(seller1Reviews.All(r => r.SellerId == seller1.Id), Is.True);
        }

        [Test]
        public void GetReviewsByBuyer_ShouldReturnOnlyBuyerReviews()
        {
            var mockConfiguration = new Mock<IConfiguration>();
            var mockUserService = new Mock<IUserService>();
            var currentUser = new User("1", "TestUser", "test@mail.com");

            // Arrange
            _mockRepository = new ReviewRepositoryMock();
            _reviewsService = new ReviewsService(mockConfiguration.Object, mockUserService.Object, currentUser);

            var seller = new User(SELLER_1_ID, SELLER_1_NAME, SELLER_1_EMAIL);
            var buyer1 = new User(BUYER_1_ID, SELLER_2_NAME, SELLER_2_EMAIL); // Reusing Seller2 constants for buyer1
            var buyer2 = new User(BUYER_2_ID, BUYER_NAME, BUYER_EMAIL);

            _mockRepository.CreateReview(new Review(REVIEW_1_ID, REVIEW_1_DESCRIPTION, new List<Image>(), RATING_4_5, seller.Id, buyer1.Id));
            _mockRepository.CreateReview(new Review(REVIEW_2_ID, REVIEW_2_DESCRIPTION, new List<Image>(), RATING_5_0, seller.Id, buyer1.Id));
            _mockRepository.CreateReview(new Review(REVIEW_3_ID, REVIEW_3_DESCRIPTION, new List<Image>(), RATING_3_5, seller.Id, buyer2.Id));

            // Act
            var buyerReviews = _reviewsService.GetReviewsByBuyer(buyer1);

            // Assert
            Assert.That(buyerReviews.Count, Is.EqualTo(EXPECTED_TWO_ITEMS));
            Assert.That(buyerReviews.All(r => r.BuyerId == buyer1.Id), Is.True);
        }

        [Test]
        public void AddReview_ShouldAddReviewToRepository()
        {
            var mockConfiguration = new Mock<IConfiguration>();
            var mockUserService = new Mock<IUserService>();
            var currentUser = new User("1", "TestUser", "test@mail.com");

            // Arrange
            _mockRepository = new ReviewRepositoryMock();
            _reviewsService = new ReviewsService(mockConfiguration.Object, mockUserService.Object, currentUser);

            var seller = new User(SELLER_1_ID, SELLER_LUCA_NAME, SELLER_LUCA_EMAIL);
            var buyer = new User(BUYER_1_ID, BUYER_CRISTI_NAME, BUYER_CRISTI_EMAIL);
            string description = LOVE_TESTING_DESCRIPTION;
            List<Image> images = new List<Image>();
            float rating = RATING_4_8;

            // Act
            _reviewsService.AddReview(description, images, rating, seller, buyer);

            // Assert
            Assert.That(_mockRepository.Reviews.Count, Is.EqualTo(EXPECTED_SINGLE_ITEM));
            Assert.That(_mockRepository.Reviews[0].Description, Is.EqualTo(description));
            Assert.That(_mockRepository.Reviews[0].Rating, Is.EqualTo(rating));
            Assert.That(_mockRepository.Reviews[0].SellerId, Is.EqualTo(seller.Id));
            Assert.That(_mockRepository.Reviews[0].BuyerId, Is.EqualTo(buyer.Id));
        }

        [Test]
        public void EditReview_ShouldUpdateReviewInRepository()
        {
            var mockConfiguration = new Mock<IConfiguration>();
            var mockUserService = new Mock<IUserService>();
            var currentUser = new User("1", "TestUser", "test@mail.com");

            // Arrange
            _mockRepository = new ReviewRepositoryMock();
            _reviewsService = new ReviewsService(mockConfiguration.Object, mockUserService.Object, currentUser);



            var seller = new User(SELLER_1_ID, SELLER_LUCA_NAME, SELLER_LUCA_EMAIL);
            var buyer = new User(BUYER_1_ID, BUYER_CRISTI_NAME, BUYER_CRISTI_EMAIL);
            string originalDescription = LOVE_TESTING_DESCRIPTION;
            string newDescription = LOVE_TESTING_NOT_DESCRIPTION;
            float originalRating = RATING_3_0;
            float newRating = RATING_4_5;

            var review = new Review(REVIEW_1_ID, originalDescription, new List<Image>(), originalRating, seller.Id, buyer.Id);
            _mockRepository.CreateReview(review);

            // Act
            _reviewsService.EditReview(originalDescription, new List<Image>(), originalRating, seller.Id, buyer.Id, newDescription, newRating);

            // Assert
            Assert.That(_mockRepository.Reviews.Count, Is.EqualTo(EXPECTED_SINGLE_ITEM));
            var editedReview = _mockRepository.Reviews[0];
            Assert.That(editedReview.Description, Is.EqualTo(newDescription));
            Assert.That(editedReview.Rating, Is.EqualTo(newRating));
        }

        [Test]
        public void DeleteReview_ShouldRemoveReviewFromRepository()
        {
            var mockConfiguration = new Mock<IConfiguration>();
            var mockUserService = new Mock<IUserService>();
            var currentUser = new User("1", "TestUser", "test@mail.com");

            // Arrange
            _mockRepository = new ReviewRepositoryMock();
            _reviewsService = new ReviewsService(mockConfiguration.Object, mockUserService.Object, currentUser);

            var seller = new User(SELLER_1_ID, SELLER_LUCA_NAME, SELLER_LUCA_EMAIL);
            var buyer = new User(BUYER_1_ID, BUYER_CRISTI_NAME, BUYER_CRISTI_EMAIL);
            string originalDescription = LOVE_TESTING_DESCRIPTION;
            float rating = RATING_4_0;

            var review = new Review(REVIEW_1_ID, originalDescription, new List<Image>(), rating, seller.Id, buyer.Id);
            _mockRepository.CreateReview(review);
            Assert.That(_mockRepository.Reviews.Count, Is.EqualTo(EXPECTED_SINGLE_ITEM),
                "Precondition: Repository should have one review");

            // Act
            _reviewsService.DeleteReview(originalDescription, new List<Image>(), rating, seller.Id, buyer.Id);

            // Assert
            Assert.That(_mockRepository.Reviews.Count, Is.EqualTo(EXPECTED_ZERO_ITEMS));
        }
    }
}

