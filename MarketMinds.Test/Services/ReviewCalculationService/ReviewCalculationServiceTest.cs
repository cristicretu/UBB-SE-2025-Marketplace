using MarketMinds.Shared.Models;
using NUnit.Framework;
using System.Collections.Generic;
using MarketMinds.Shared.Services.ReviewCalculationService;
using System.Collections.ObjectModel;

namespace MarketMinds.Test.Services
{
    [TestFixture]
    public class ReviewCalculationServiceTest
    {
        // Constants for review properties
        private const int REVIEW_ID_1 = 1;
        private const int REVIEW_ID_2 = 2;
        private const int REVIEW_ID_3 = 3;

        // Constants for seller IDs
        private const int SELLER_ID_1 = 101;
        private const int SELLER_ID_2 = 102;
        private const int SELLER_ID_3 = 103;

        // Constants for buyer ID
        private const int BUYER_ID = 1000;

        // Constants for review texts
        private const string GOOD_PRODUCT_TEXT = "Good product";
        private const string EXCELLENT_TEXT = "Excellent";
        private const string AVERAGE_TEXT = "Average";
        private const string POOR_TEXT = "Poor";
        private const string BAD_TEXT = "Bad";
        private const string TERRIBLE_TEXT = "Terrible";
        private const string AWFUL_TEXT = "Awful";
        private const string VERY_BAD_TEXT = "Very Bad";
        private const string HORRIBLE_TEXT = "Horrible";
        private const string FIRST_REVIEW_TEXT = "First review";
        private const string SECOND_REVIEW_TEXT = "Second review";
        private const string THIRD_REVIEW_TEXT = "Third review";

        // Constants for ratings
        private const float FOUR_STAR_RATING = 4.0f;
        private const float FIVE_STAR_RATING = 5.0f;
        private const float THREE_STAR_RATING = 3.0f;
        private const float ZERO_STAR_RATING = 0.0f;
        private const float NEGATIVE_ONE_STAR_RATING = -1.0f;
        private const float NEGATIVE_TWO_STAR_RATING = -2.0f;
        private const float NEGATIVE_THREE_STAR_RATING = -3.0f;

        // Constants for expected results
        private const float EXPECTED_AVERAGE_FOUR_STARS = 4.0f;
        private const float EXPECTED_AVERAGE_ZERO = 0.0f;
        private const float EXPECTED_AVERAGE_NEGATIVE_TWO = -2.0f;
        private const int EXPECTED_COUNT_ZERO = 0;
        private const int EXPECTED_COUNT_THREE = 3;
        private const int EXPECTED_COUNT_TWO = 2;

        private IReviewCalculationService _service;


        [SetUp]
        public void Setup()
        {
            _service = new ReviewCalculationService();

        }

        private Review CreateReview(int id, string text, List<Image> images, float rating, int authorId)
        {
            return new Review(id, text, images, rating, authorId, BUYER_ID);
        }

        [Test]
        public void CalculateAverageRating_WithValidReviews_ReturnsCorrectAverage()
        {
            // Arrange
            var reviews = new List<Review>
            {
                CreateReview(REVIEW_ID_1, GOOD_PRODUCT_TEXT, new List<Image>(), FOUR_STAR_RATING, SELLER_ID_1),
                CreateReview(REVIEW_ID_2, EXCELLENT_TEXT, new List<Image>(), FIVE_STAR_RATING, SELLER_ID_2),
                CreateReview(REVIEW_ID_3, AVERAGE_TEXT, new List<Image>(), THREE_STAR_RATING, SELLER_ID_3)
            };

            var observableReviews = new ObservableCollection<Review>(reviews);


            // Act
            double result = _service.CalculateAverageRating(observableReviews);

            // Assert
            Assert.That(result, Is.EqualTo(EXPECTED_AVERAGE_FOUR_STARS));
        }

        [Test]
        public void CalculateAverageRating_WithEmptyList_ReturnsZero()
        {
            // Arrange
            var reviews = new List<Review>();

            var observableReviews = new ObservableCollection<Review>(reviews);


            // Act
            double result = _service.CalculateAverageRating(observableReviews);

            // Assert
            Assert.That(result, Is.EqualTo(EXPECTED_AVERAGE_ZERO));
        }

        [Test]
        public void CalculateAverageRating_WithNullList_ReturnsZero()
        {
            // Arrange
            List<Review> reviews = null;

            var observableReviews = new ObservableCollection<Review>(reviews);


            // Act
            double result = _service.CalculateAverageRating(observableReviews);

            // Assert
            Assert.That(result, Is.EqualTo(EXPECTED_AVERAGE_ZERO));
        }

        [Test]
        public void CalculateAverageRating_WithAllZeroRatings_ReturnsZero()
        {
            // Arrange
            var reviews = new List<Review>
            {
                CreateReview(REVIEW_ID_1, POOR_TEXT, new List<Image>(), ZERO_STAR_RATING, SELLER_ID_1),
                CreateReview(REVIEW_ID_2, BAD_TEXT, new List<Image>(), ZERO_STAR_RATING, SELLER_ID_2),
                CreateReview(REVIEW_ID_3, TERRIBLE_TEXT, new List<Image>(), ZERO_STAR_RATING, SELLER_ID_3)
            };

            var observableReviews = new ObservableCollection<Review>(reviews);


            // Act
            double result = _service.CalculateAverageRating(observableReviews);

            // Assert
            Assert.That(result, Is.EqualTo(EXPECTED_AVERAGE_ZERO));
        }

        [Test]
        public void CalculateAverageRating_WithNegativeRatings_ReturnsCorrectAverage()
        {
            // Arrange
            var reviews = new List<Review>
            {
                CreateReview(REVIEW_ID_1, AWFUL_TEXT, new List<Image>(), NEGATIVE_ONE_STAR_RATING, SELLER_ID_1),
                CreateReview(REVIEW_ID_2, VERY_BAD_TEXT, new List<Image>(), NEGATIVE_TWO_STAR_RATING, SELLER_ID_2),
                CreateReview(REVIEW_ID_3, HORRIBLE_TEXT, new List<Image>(), NEGATIVE_THREE_STAR_RATING, SELLER_ID_3)
            };

            var observableReviews = new ObservableCollection<Review>(reviews);


            // Act
            double result = _service.CalculateAverageRating(observableReviews);

            // Assert
            Assert.That(result, Is.EqualTo(EXPECTED_AVERAGE_NEGATIVE_TWO));
        }

        [Test]
        public void GetReviewCount_WithValidReviews_ReturnsCorrectCount()
        {
            // Arrange
            var reviews = new List<Review>
            {
                CreateReview(REVIEW_ID_1, FIRST_REVIEW_TEXT, new List<Image>(), FOUR_STAR_RATING, SELLER_ID_1),
                CreateReview(REVIEW_ID_2, SECOND_REVIEW_TEXT, new List<Image>(), FIVE_STAR_RATING, SELLER_ID_2),
                CreateReview(REVIEW_ID_3, THIRD_REVIEW_TEXT, new List<Image>(), THREE_STAR_RATING, SELLER_ID_3)
            };

            var observableReviews = new ObservableCollection<Review>(reviews);


            // Act
            int result = _service.GetReviewCount(observableReviews);


            // Assert
            Assert.That(result, Is.EqualTo(EXPECTED_COUNT_THREE));
        }

        [Test]
        public void GetReviewCount_WithEmptyList_ReturnsZero()
        {
            // Arrange
            var reviews = new List<Review>();

            var observableReviews = new ObservableCollection<Review>(reviews);


            // Act
            int result = _service.GetReviewCount(observableReviews);

            // Assert
            Assert.That(result, Is.EqualTo(EXPECTED_COUNT_ZERO));
        }

        [Test]
        public void GetReviewCount_WithNullList_ReturnsZero()
        {
            // Arrange
            List<Review> reviews = null;

            var observableReviews = new ObservableCollection<Review>(reviews);


            // Act
            int result = _service.GetReviewCount(observableReviews);

            // Assert
            Assert.That(result, Is.EqualTo(EXPECTED_COUNT_ZERO));
        }

        [Test]
        public void AreReviewsEmpty_WithValidReviews_ReturnsFalse()
        {
            // Arrange
            var reviews = new List<Review>
            {
                CreateReview(REVIEW_ID_1, FIRST_REVIEW_TEXT, new List<Image>(), FOUR_STAR_RATING, SELLER_ID_1),
                CreateReview(REVIEW_ID_2, SECOND_REVIEW_TEXT, new List<Image>(), FIVE_STAR_RATING, SELLER_ID_2)
            };


            var observableReviews = new ObservableCollection<Review>(reviews);

            // Act
            bool result = _service.AreReviewsEmpty(observableReviews);


            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void AreReviewsEmpty_WithEmptyList_ReturnsTrue()
        {
            // Arrange
            var reviews = new List<Review>();

            var observableReviews = new ObservableCollection<Review>(reviews);

            // Act
            bool result = _service.AreReviewsEmpty(observableReviews);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void AreReviewsEmpty_WithNullList_ReturnsTrue()
        {
            // Arrange
            List<Review> reviews = null;

            var observableReviews = new ObservableCollection<Review>(reviews);

            // Act
            bool result = _service.AreReviewsEmpty(observableReviews);

            // Assert
            Assert.That(result, Is.True);
        }
    }
}
