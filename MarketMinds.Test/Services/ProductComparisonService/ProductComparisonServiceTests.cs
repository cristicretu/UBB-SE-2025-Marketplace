using NUnit.Framework;
using MarketMinds.Shared.Services.ProductComparisonService;
using MarketMinds.Shared.Models;

namespace MarketMinds.Test.Services.ProductComparisonServiceTests
{
    [TestFixture]
    public class ProductComparisonServiceTests
    {
        private ProductComparisonService _service;

        [SetUp]
        public void Setup()
        {
            _service = new ProductComparisonService();
        }

        /// <summary>
        /// A simple concrete implementation of the abstract Product class for testing purposes.
        /// </summary>
        private class TestProduct : Product
        {
            public TestProduct(int id, string title, string description)
                : base(id, title, description)
            {
            }
        }

        [Test]
        public void AddProduct_WhenLeftProductIsNull_ReturnsNewAsLeftAndIncomplete()
        {
            // Arrange
            TestProduct left = null!;
            var right = new TestProduct(2, "Right", "RightDesc");
            var newProd = new TestProduct(1, "New", "NewDesc");

            // Act
            var result = _service.AddProduct(left, right, newProd);

            // Assert
            Assert.That(result.LeftProduct, Is.SameAs(newProd));
            Assert.That(result.RightProduct, Is.SameAs(right));
            Assert.That(result.IsComplete, Is.False);
        }

        [Test]
        public void AddProduct_WhenNewProductIsDifferent_ReturnsOriginalLeftAndNewAsRightAndComplete()
        {
            // Arrange
            var left = new TestProduct(1, "Left", "LeftDesc");
            var right = new TestProduct(2, "Right", "RightDesc");
            var newProd = new TestProduct(3, "New", "NewDesc");

            // Act
            var result = _service.AddProduct(left, right, newProd);

            // Assert
            Assert.That(result.LeftProduct, Is.SameAs(left));
            Assert.That(result.RightProduct, Is.SameAs(newProd));
            Assert.That(result.IsComplete, Is.True);
        }

        [Test]
        public void AddProduct_WhenNewProductIsSameAsLeft_ReturnsOriginalsAndIncomplete()
        {
            // Arrange
            var left = new TestProduct(1, "Left", "LeftDesc");
            var right = new TestProduct(2, "Right", "RightDesc");
            var newProd = left; // same reference

            // Act
            var result = _service.AddProduct(left, right, newProd);

            // Assert
            Assert.That(result.LeftProduct, Is.SameAs(left));
            Assert.That(result.RightProduct, Is.SameAs(right));
            Assert.That(result.IsComplete, Is.False);
        }

        [Test]
        public void AddProduct_WhenNewProductIsNullAndLeftIsNotNull_ReturnsLeftAndNullRightAndComplete()
        {
            // Arrange
            var left = new TestProduct(1, "Left", "LeftDesc");
            var right = new TestProduct(2, "Right", "RightDesc");
            TestProduct newProd = null!;

            // Act
            var result = _service.AddProduct(left, right, newProd);

            // Assert
            Assert.That(result.LeftProduct, Is.SameAs(left));
            Assert.That(result.RightProduct, Is.Null);
            Assert.That(result.IsComplete, Is.True);
        }
    }
}
