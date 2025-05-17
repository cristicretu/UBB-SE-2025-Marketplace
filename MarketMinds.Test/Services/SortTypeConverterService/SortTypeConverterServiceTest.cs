using MarketMinds.Shared.Services;
using NUnit.Framework;

namespace MarketMinds.Test.Services
{
    [TestFixture]
    public class SortTypeConverterServiceTest
    {
        // Constants for sort tags
        private const string PRICE_ASCENDING_TAG = "PriceAsc";
        private const string PRICE_DESCENDING_TAG = "PriceDesc";
        private const string INVALID_SORTING_TAG = "InvalidSortTag";
        private const string EMPTY_TAG = "";

        // Constants for field titles
        private const string PRICE_FIELD_TITLE = "Price";

        private SortTypeConverterService _converter;

        [SetUp]
        public void Setup()
        {
            _converter = new SortTypeConverterService();
        }

        [Test]
        public void Convert_PriceAsc_ReturnsNonNullResult()
        {
            // Arrange
            string sortTag = PRICE_ASCENDING_TAG;

            // Act
            var result = _converter.Convert(sortTag);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Convert_PriceAsc_ReturnsCorrectExternalTitle()
        {
            // Arrange
            string sortTag = PRICE_ASCENDING_TAG;

            // Act
            var result = _converter.Convert(sortTag);

            // Assert
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo(PRICE_FIELD_TITLE));
        }

        [Test]
        public void Convert_PriceAsc_ReturnsCorrectInternalTitle()
        {
            // Arrange
            string sortTag = PRICE_ASCENDING_TAG;

            // Act
            var result = _converter.Convert(sortTag);

            // Assert
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo(PRICE_FIELD_TITLE));
        }

        [Test]
        public void Convert_PriceAsc_ReturnsAscendingOrder()
        {
            // Arrange
            string sortTag = PRICE_ASCENDING_TAG;

            // Act
            var result = _converter.Convert(sortTag);

            // Assert
            Assert.That(result.IsAscending, Is.True);
        }

        [Test]
        public void Convert_PriceDesc_ReturnsNonNullResult()
        {
            // Arrange
            string sortTag = PRICE_DESCENDING_TAG;

            // Act
            var result = _converter.Convert(sortTag);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Convert_PriceDesc_ReturnsCorrectExternalTitle()
        {
            // Arrange
            string sortTag = PRICE_DESCENDING_TAG;

            // Act
            var result = _converter.Convert(sortTag);

            // Assert
            Assert.That(result.ExternalAttributeFieldTitle, Is.EqualTo(PRICE_FIELD_TITLE));
        }

        [Test]
        public void Convert_PriceDesc_ReturnsCorrectInternalTitle()
        {
            // Arrange
            string sortTag = PRICE_DESCENDING_TAG;

            // Act
            var result = _converter.Convert(sortTag);

            // Assert
            Assert.That(result.InternalAttributeFieldTitle, Is.EqualTo(PRICE_FIELD_TITLE));
        }

        [Test]
        public void Convert_PriceDesc_ReturnsDescendingOrder()
        {
            // Arrange
            string sortTag = PRICE_DESCENDING_TAG;

            // Act
            var result = _converter.Convert(sortTag);

            // Assert
            Assert.That(result.IsAscending, Is.False);
        }

        [Test]
        public void Convert_InvalidSortTag_ReturnsNull()
        {
            // Arrange
            string sortTag = INVALID_SORTING_TAG;

            // Act
            var result = _converter.Convert(sortTag);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Convert_EmptyString_ReturnsNull()
        {
            // Arrange
            string sortTag = EMPTY_TAG;

            // Act
            var result = _converter.Convert(sortTag);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Convert_NullString_ReturnsNull()
        {
            // Arrange
            string sortTag = null;

            // Act
            var result = _converter.Convert(sortTag);

            // Assert
            Assert.That(result, Is.Null);
        }
    }
}