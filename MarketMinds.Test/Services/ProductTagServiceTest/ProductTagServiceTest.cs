using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ProductTagService;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketMinds.Test.Services.ProductTagServiceTest
{
    [TestFixture]
    public class ProductTagServiceTest
    {
        private ProductTagProxyRepositoryMock repositoryMock;
        private ProductTagService productTagService;

        [SetUp]
        public void Setup()
        {
            repositoryMock = new ProductTagProxyRepositoryMock();
            productTagService = new TestableProductTagService(repositoryMock);
        }

        [Test]
        public void GetAllProductTags_ReturnsAllTags()
        {
            // Arrange
            var tags = new List<ProductTag>
            {
                new ProductTag { Id = 1, Title = "Electronics" },
                new ProductTag { Id = 2, Title = "Clothing" },
                new ProductTag { Id = 3, Title = "Books" }
            };
            repositoryMock.SetupProductTags(tags);

            // Act
            var result = productTagService.GetAllProductTags();

            // Assert
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result[0].Title, Is.EqualTo("Electronics"));
            Assert.That(result[1].Title, Is.EqualTo("Clothing"));
            Assert.That(result[2].Title, Is.EqualTo("Books"));
        }

        [Test]
        public void GetAllProductTags_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            repositoryMock.SetupProductTags(new List<ProductTag>());

            // Act
            var result = productTagService.GetAllProductTags();

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetAllProductTags_ExceptionThrown_ReturnsEmptyList()
        {
            // Arrange
            var mockService = new ExceptionThrowingProductTagService(repositoryMock);

            // Act
            var result = mockService.GetAllProductTags();

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void CreateProductTag_ValidTitle_ReturnsCreatedTag()
        {
            // Arrange
            const string tagTitle = "New Tag";

            // Act
            var result = productTagService.CreateProductTag(tagTitle);

            // Assert
            Assert.That(result.Title, Is.EqualTo(tagTitle));
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(repositoryMock.GetCurrentTags(), Has.Count.EqualTo(1));
        }

        [Test]
        public void CreateProductTag_NullTitle_ThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => productTagService.CreateProductTag(null));
            Assert.That(ex.Message, Does.Contain("Product tag display title cannot be null or empty"));
        }

        [Test]
        public void CreateProductTag_EmptyTitle_ThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => productTagService.CreateProductTag(""));
            Assert.That(ex.Message, Does.Contain("Product tag display title cannot be null or empty"));
        }

        [Test]
        public void CreateProductTag_WhitespaceTitle_ThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => productTagService.CreateProductTag("   "));
            Assert.That(ex.Message, Does.Contain("Product tag display title cannot be null or empty"));
        }

        [Test]
        public void CreateProductTag_TitleTooLong_ThrowsArgumentException()
        {
            var longTitle = new string('a', 101);
            var ex = Assert.Throws<ArgumentException>(() => productTagService.CreateProductTag(longTitle));
            Assert.That(ex.Message, Does.Contain("Product tag display title cannot exceed 100 characters"));
        }

        [Test]
        public void CreateProductTag_ExceptionThrown_PropagatesException()
        {
            var mockService = new ExceptionThrowingProductTagService(repositoryMock);
            Assert.Throws<InvalidOperationException>(() => mockService.CreateProductTag("Test Tag"));
        }

        [Test]
        public void DeleteProductTag_ExistingTitle_RemovesTag()
        {
            const string tagTitle = "Tag to Delete";
            repositoryMock.SetupProductTags(new List<ProductTag>
            {
                new ProductTag { Id = 1, Title = tagTitle }
            });

            // Act
            productTagService.DeleteProductTag(tagTitle);

            // Assert
            Assert.That(repositoryMock.GetCurrentTags(), Is.Empty);
        }

        [Test]
        public void DeleteProductTag_NonExistingTitle_ThrowsException()
        {
            repositoryMock.SetupProductTags(new List<ProductTag>
            {
                new ProductTag { Id = 1, Title = "Existing Tag" }
            });

            Assert.Throws<InvalidOperationException>(() => productTagService.DeleteProductTag("Non-existing Tag"));
        }

        [Test]
        public void DeleteProductTag_NullTitle_ThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => productTagService.DeleteProductTag(null));
            Assert.That(ex.Message, Does.Contain("Product tag display title cannot be null or empty"));
        }

        [Test]
        public void DeleteProductTag_EmptyTitle_ThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => productTagService.DeleteProductTag(""));
            Assert.That(ex.Message, Does.Contain("Product tag display title cannot be null or empty"));
        }

        [Test]
        public void DeleteProductTag_WhitespaceTitle_ThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => productTagService.DeleteProductTag("   "));
            Assert.That(ex.Message, Does.Contain("Product tag display title cannot be null or empty"));
        }

        [Test]
        public void DeleteProductTag_ExceptionThrown_PropagatesException()
        {
            var mockService = new ExceptionThrowingProductTagService(repositoryMock);
            repositoryMock.SetupProductTags(new List<ProductTag>
            {
                new ProductTag { Id = 1, Title = "Test Tag" }
            });

            Assert.Throws<InvalidOperationException>(() => mockService.DeleteProductTag("Test Tag"));
        }

        // Helper test classes
        private class TestableProductTagService : ProductTagService
        {
            public TestableProductTagService(ProductTagProxyRepositoryMock repositoryMock) : base(null)
            {
                this.GetType().GetField("repository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(this, repositoryMock);
            }
        }

        private class ExceptionThrowingProductTagService : ProductTagService
        {
            public ExceptionThrowingProductTagService(ProductTagProxyRepositoryMock repositoryMock) : base(null)
            {
                this.GetType().GetField("repository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(this, repositoryMock);
            }

            public override List<ProductTag> GetAllProductTags()
            {
                throw new InvalidOperationException("Test exception");
            }

            public override ProductTag CreateProductTag(string displayTitle)
            {
                throw new InvalidOperationException("Test exception");
            }

            public override void DeleteProductTag(string displayTitle)
            {
                throw new InvalidOperationException("Test exception");
            }
        }
    }
}
