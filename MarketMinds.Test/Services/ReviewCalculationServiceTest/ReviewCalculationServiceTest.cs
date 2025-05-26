using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ReviewCalculationService;
using NUnit.Framework;
using System.Collections.ObjectModel;

namespace MarketMinds.Test.Services.ReviewCalculationServiceTest
{
    [TestFixture]
    public class ReviewCalculationServiceTest
    {
        private ReviewCalculationService reviewCalculationService;

        [SetUp]
        public void Setup()
        {
            reviewCalculationService = new ReviewCalculationService();
        }

        [Test]
        public void CalculateAverageRating_WithValidReviews_ReturnsCorrectAverage()
        {
            // Arrange
            var reviews = new ObservableCollection<Review>
            {
                new Review { Rating = 3.0 },
                new Review { Rating = 4.0 },
                new Review { Rating = 5.0 }
            };

            // Act
            var result = reviewCalculationService.CalculateAverageRating(reviews);

            // Assert
            Assert.That(result, Is.EqualTo(4.0));
        }

        [Test]
        public void CalculateAverageRating_WithEmptyCollection_ReturnsZero()
        {
            // Arrange
            var reviews = new ObservableCollection<Review>();

            // Act
            var result = reviewCalculationService.CalculateAverageRating(reviews);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void CalculateAverageRating_WithNullCollection_ReturnsZero()
        {
            // Act
            var result = reviewCalculationService.CalculateAverageRating(null);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void CalculateAverageRating_WithSingleReview_ReturnsThatRating()
        {
            // Arrange
            var reviews = new ObservableCollection<Review>
            {
                new Review { Rating = 4.5 }
            };

            // Act
            var result = reviewCalculationService.CalculateAverageRating(reviews);

            // Assert
            Assert.That(result, Is.EqualTo(4.5));
        }

        [Test]
        public void GetReviewCount_WithValidCollection_ReturnsCorrectCount()
        {
            // Arrange
            var reviews = new ObservableCollection<Review>
            {
                new Review(),
                new Review(),
                new Review()
            };

            // Act
            var result = reviewCalculationService.GetReviewCount(reviews);

            // Assert
            Assert.That(result, Is.EqualTo(3));
        }

        [Test]
        public void GetReviewCount_WithEmptyCollection_ReturnsZero()
        {
            // Arrange
            var reviews = new ObservableCollection<Review>();

            // Act
            var result = reviewCalculationService.GetReviewCount(reviews);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void GetReviewCount_WithNullCollection_ReturnsZero()
        {
            // Act
            var result = reviewCalculationService.GetReviewCount(null);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void AreReviewsEmpty_WithNullCollection_ReturnsTrue()
        {
            // Act
            var result = reviewCalculationService.AreReviewsEmpty(null);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void AreReviewsEmpty_WithEmptyCollection_ReturnsTrue()
        {
            // Arrange
            var reviews = new ObservableCollection<Review>();

            // Act
            var result = reviewCalculationService.AreReviewsEmpty(reviews);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void AreReviewsEmpty_WithNonEmptyCollection_ReturnsFalse()
        {
            // Arrange
            var reviews = new ObservableCollection<Review>
            {
                new Review()
            };

            // Act
            var result = reviewCalculationService.AreReviewsEmpty(reviews);

            // Assert
            Assert.That(result, Is.False);
        }
    }
}
