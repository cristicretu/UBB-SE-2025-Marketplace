using MarketMinds.Shared.Helper;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.ProxyRepository;
using MarketMinds.Shared.Services.ProductTagService;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace MarketMinds.Test.Services.ProductTagService
{
    [TestFixture]
    public class ProductTagServiceTest
    {
        // Constants to replace magic strings and numbers
        private const int TAG_ID_1 = 1;
        private const int TAG_ID_2 = 2;
        private const int TAG_ID_3 = 3;
        private const string ELECTRONICS_TAG_TITLE = "Electronics";
        private const string CLOTHING_TAG_TITLE = "Clothing";
        private const string BOOKS_TAG_TITLE = "Books";
        private const string GAMING_TAG_TITLE = "Gaming";
        private const string FURNITURE_TAG_TITLE = "Furniture";
        private const int EXPECTED_EMPTY_COUNT = 0;
        private const int EXPECTED_SINGLE_TAG_COUNT = 1;
        private const int EXPECTED_THREE_TAGS_COUNT = 3;

        private Mock<IConfiguration> _mockConfiguration;
        private Mock<ProductTagProxyRepository> _mockRepository;
        private IProductTagService _service;
        private JsonSerializerOptions _testJsonOptions;

        [SetUp]
        public void Setup()
        {
            _mockConfiguration = new Mock<IConfiguration>();

            var mockConigurationSection = new Mock<IConfigurationSection>();
            mockConigurationSection.Setup(x => x.Value)
                                   .Returns("http://localhost:5000");
            _mockConfiguration.Setup(x => x.GetSection("ApiSettings:BaseUrl"))
                              .Returns(mockConigurationSection.Object);

            _mockRepository = new Mock<ProductTagProxyRepository>(_mockConfiguration.Object);
            _service = new MarketMinds.Shared.Services.ProductTagService.ProductTagService(AppConfig.Configuration);

            _testJsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

        }

        #region GetAllProductTags Tests

        [Test]
        public void GetAllProductTags_ReturnsNonNullResult()
        {
            // Arrange
            var tagsToReturn = new List<ProductTag>
            {
                new ProductTag(TAG_ID_1, ELECTRONICS_TAG_TITLE),
                new ProductTag(TAG_ID_2, CLOTHING_TAG_TITLE),
                new ProductTag(TAG_ID_3, BOOKS_TAG_TITLE)
            };
            _mockRepository.Setup(repo => repo.GetAllProductTagsRaw())
                           .Returns(SerializeTags(tagsToReturn));

            // Act
            var result = _service.GetAllProductTags();

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void GetAllProductTags_ReturnsCorrectNumberOfTags()
        {
            // Arrange
            var tagsToReturn = new List<ProductTag>
            {
                new ProductTag(TAG_ID_1, ELECTRONICS_TAG_TITLE),
                new ProductTag(TAG_ID_2, CLOTHING_TAG_TITLE),
                new ProductTag(TAG_ID_3, BOOKS_TAG_TITLE)
            };
            _mockRepository.Setup(repo => repo.GetAllProductTagsRaw())
                           .Returns(SerializeTags(tagsToReturn));

            // Act
            var result = _service.GetAllProductTags();

            // Assert
            Assert.That(result.Count, Is.EqualTo(EXPECTED_THREE_TAGS_COUNT));
        }

        [Test]
        public void GetAllProductTags_ReturnsCorrectFirstTagTitle()
        {
            // Arrange
            var tagsToReturn = new List<ProductTag>
            {
                new ProductTag(TAG_ID_1, ELECTRONICS_TAG_TITLE),
                new ProductTag(TAG_ID_2, CLOTHING_TAG_TITLE),
                new ProductTag(TAG_ID_3, BOOKS_TAG_TITLE)
            };
            _mockRepository.Setup(repo => repo.GetAllProductTagsRaw())
                           .Returns(SerializeTags(tagsToReturn));

            // Act
            var result = _service.GetAllProductTags();

            // Assert
            Assert.That(result[0].DisplayTitle, Is.EqualTo(ELECTRONICS_TAG_TITLE));
        }

        [Test]
        public void GetAllProductTags_ReturnsCorrectSecondTagTitle()
        {
            // Arrange
            var tagsToReturn = new List<ProductTag>
            {
                new ProductTag(TAG_ID_1, ELECTRONICS_TAG_TITLE),
                new ProductTag(TAG_ID_2, CLOTHING_TAG_TITLE),
                new ProductTag(TAG_ID_3, BOOKS_TAG_TITLE)
            };
            _mockRepository.Setup(repo => repo.GetAllProductTagsRaw())
                           .Returns(SerializeTags(tagsToReturn));

            // Act
            var result = _service.GetAllProductTags();

            // Assert
            Assert.That(result[1].DisplayTitle, Is.EqualTo(CLOTHING_TAG_TITLE));
        }

        [Test]
        public void GetAllProductTags_ReturnsCorrectThirdTagTitle()
        {
            // Arrange
            var tagsToReturn = new List<ProductTag>
            {
                new ProductTag(TAG_ID_1, ELECTRONICS_TAG_TITLE),
                new ProductTag(TAG_ID_2, CLOTHING_TAG_TITLE),
                new ProductTag(TAG_ID_3, BOOKS_TAG_TITLE)
            };
            _mockRepository.Setup(repo => repo.GetAllProductTagsRaw())
                           .Returns(SerializeTags(tagsToReturn));
            // Act
            var result = _service.GetAllProductTags();

            // Assert
            Assert.That(result[2].DisplayTitle, Is.EqualTo(BOOKS_TAG_TITLE));
        }

        [Test]
        public void GetAllProductTags_WithEmptyRepository_ReturnsNonNullResult()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.GetAllProductTagsRaw())
                           .Returns("[]");

            // Act
            var result = _service.GetAllProductTags();

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void GetAllProductTags_WithEmptyRepository_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.GetAllProductTagsRaw())
                           .Returns("[]");
            // Act
            var result = _service.GetAllProductTags();

            // Assert
            Assert.That(result.Count, Is.EqualTo(EXPECTED_EMPTY_COUNT));
        }
        [Test]
        public void GetAllProductTags_RepositoryThrowsException_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllProductTagsRaw()).Throws(new System.Net.Http.HttpRequestException("Network error"));

            // Act
            var result = _service.GetAllProductTags();

            // Assert
            Assert.That(result.Count, Is.EqualTo(EXPECTED_EMPTY_COUNT));
        }

        #endregion

        #region CreateProductTag Tests

        [Test]
        public void CreateProductTag_ReturnsNonNullResult()
        {
            // Arrange
            var createdTag = new ProductTag(TAG_ID_1, GAMING_TAG_TITLE);
            _mockRepository.Setup(repo => repo.CreateProductTagRaw(GAMING_TAG_TITLE))
                           .Returns(SerializeTag(createdTag));
            // Act
            var result = _service.CreateProductTag(GAMING_TAG_TITLE);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void CreateProductTag_ReturnsTagWithCorrectTitle()
        {
            // Arrange
            var createdTag = new ProductTag(TAG_ID_1, GAMING_TAG_TITLE);
            _mockRepository.Setup(repo => repo.CreateProductTagRaw(GAMING_TAG_TITLE))
                           .Returns(SerializeTag(createdTag));

            // Act
            var result = _service.CreateProductTag(GAMING_TAG_TITLE);

            // Assert
            Assert.That(result.DisplayTitle, Is.EqualTo(GAMING_TAG_TITLE));
        }

        [Test]
        public void CreateProductTag_ReturnsTagWithCorrectId()
        {
            // Arrange
            var createdTag = new ProductTag(TAG_ID_1, GAMING_TAG_TITLE);
            _mockRepository.Setup(repo => repo.CreateProductTagRaw(GAMING_TAG_TITLE))
                           .Returns(SerializeTag(createdTag));
            // Act
            var result = _service.CreateProductTag(GAMING_TAG_TITLE);

            // Assert
            Assert.That(result.Id, Is.EqualTo(TAG_ID_1));
        }

        [Test]
        public void CreateProductTag_AddsTagToRepository()
        {
            // Arrange
            var createdTag = new ProductTag(TAG_ID_1, GAMING_TAG_TITLE);
            _mockRepository.Setup(repo => repo.CreateProductTagRaw(GAMING_TAG_TITLE))
                           .Returns(SerializeTag(createdTag));
            // Act
            _service.CreateProductTag(GAMING_TAG_TITLE);

            // Assert
            _mockRepository.Verify(repo => repo.CreateProductTagRaw(GAMING_TAG_TITLE), Times.Once);
        }

        [Test]
        public void CreateProductTag_AddsTagWithCorrectTitleToRepository()
        {
            // Arrange
            var createdTag = new ProductTag(TAG_ID_1, GAMING_TAG_TITLE);
            _mockRepository.Setup(repo => repo.CreateProductTagRaw(GAMING_TAG_TITLE))
                           .Returns(SerializeTag(createdTag));

            // Act
            _service.CreateProductTag(GAMING_TAG_TITLE);

            // Assert
            _mockRepository.Setup(repo => repo.GetAllProductTagsRaw())
                            .Returns(SerializeTags(new List<ProductTag> { createdTag }));
            var result = _service.GetAllProductTags();

            Assert.That(result.First().DisplayTitle, Is.EqualTo(GAMING_TAG_TITLE));
        }

        #endregion

        #region CreateProductTag Multiple Tags Tests

        [Test]
        public void CreateProductTag_MultipleTags_FirstTagHasCorrectId()
        {
            // Arrange
            var createdTag = new ProductTag(TAG_ID_1, ELECTRONICS_TAG_TITLE);
            _mockRepository.Setup(repo => repo.CreateProductTagRaw(ELECTRONICS_TAG_TITLE))
                           .Returns(SerializeTag(createdTag));
            // Act
            var tag1 = _service.CreateProductTag(ELECTRONICS_TAG_TITLE);

            // Assert
            Assert.That(tag1.Id, Is.EqualTo(TAG_ID_1));
        }

        [Test]
        public void CreateProductTag_MultipleTags_SecondTagHasCorrectId()
        {
            // Arrange
            var createdTag1 = new ProductTag(TAG_ID_2, ELECTRONICS_TAG_TITLE);
            var createdTag2 = new ProductTag(TAG_ID_2, CLOTHING_TAG_TITLE);

            _mockRepository.Setup(repo => repo.CreateProductTagRaw(ELECTRONICS_TAG_TITLE))
                           .Returns(SerializeTag(createdTag1));
            _mockRepository.Setup(repo => repo.CreateProductTagRaw(CLOTHING_TAG_TITLE))
                           .Returns(SerializeTag(createdTag2));

            // Act
            _service.CreateProductTag(ELECTRONICS_TAG_TITLE);
            var tag2 = _service.CreateProductTag(CLOTHING_TAG_TITLE);

            // Assert
            Assert.That(tag2.Id, Is.EqualTo(TAG_ID_2));
        }

        [Test]
        public void CreateProductTag_MultipleTags_ThirdTagHasCorrectId()
        {
            // Arrange
            var createdTag1 = new ProductTag(TAG_ID_3, ELECTRONICS_TAG_TITLE);
            var createdTag2 = new ProductTag(TAG_ID_3, CLOTHING_TAG_TITLE);
            var createdTag3 = new ProductTag(TAG_ID_3, BOOKS_TAG_TITLE);
            _mockRepository.Setup(repo => repo.CreateProductTagRaw(ELECTRONICS_TAG_TITLE))
                           .Returns(SerializeTag(createdTag1));
            _mockRepository.Setup(repo => repo.CreateProductTagRaw(CLOTHING_TAG_TITLE))
                            .Returns(SerializeTag(createdTag2));
            _mockRepository.Setup(repo => repo.CreateProductTagRaw(BOOKS_TAG_TITLE))
                            .Returns(SerializeTag(createdTag3));

            // Act
            _service.CreateProductTag(ELECTRONICS_TAG_TITLE);
            _service.CreateProductTag(CLOTHING_TAG_TITLE);
            var tag3 = _service.CreateProductTag(BOOKS_TAG_TITLE);

            // Assert
            Assert.That(tag3.Id, Is.EqualTo(TAG_ID_3));
        }

        [Test]
        public void CreateProductTag_MultipleTags_AddsAllTagsToRepository()
        {
            // Arrange
            var createdTag1 = new ProductTag(TAG_ID_1, ELECTRONICS_TAG_TITLE);
            var createdTag2 = new ProductTag(TAG_ID_2, CLOTHING_TAG_TITLE);
            var createdTag3 = new ProductTag(TAG_ID_3, BOOKS_TAG_TITLE);

            _mockRepository.Setup(repo => repo.CreateProductTagRaw(ELECTRONICS_TAG_TITLE))
                           .Returns(SerializeTag(createdTag1));
            _mockRepository.Setup(repo => repo.CreateProductTagRaw(CLOTHING_TAG_TITLE))
                            .Returns(SerializeTag(createdTag2));
            _mockRepository.Setup(repo => repo.CreateProductTagRaw(BOOKS_TAG_TITLE))
                            .Returns(SerializeTag(createdTag3));
            _mockRepository.Setup(repo => repo.GetAllProductTagsRaw())
                            .Returns(SerializeTags(new List<ProductTag> { createdTag1, createdTag2, createdTag3 }));

            // Act
            _service.CreateProductTag(ELECTRONICS_TAG_TITLE);
            _service.CreateProductTag(CLOTHING_TAG_TITLE);
            _service.CreateProductTag(BOOKS_TAG_TITLE);

            // Assert
            var result = _service.GetAllProductTags();
            Assert.That(result.Count, Is.EqualTo(EXPECTED_THREE_TAGS_COUNT));
        }

        [Test]
        public void CreateProductTag_MultipleTags_EnsuresUniqueIds()
        {
            // Arrange
            var createdTag1 = new ProductTag(TAG_ID_1, ELECTRONICS_TAG_TITLE);
            var createdTag2 = new ProductTag(TAG_ID_2, CLOTHING_TAG_TITLE);
            _mockRepository.Setup(repo => repo.CreateProductTagRaw(ELECTRONICS_TAG_TITLE))
                           .Returns(SerializeTag(createdTag1));
            _mockRepository.Setup(repo => repo.CreateProductTagRaw(CLOTHING_TAG_TITLE))
                            .Returns(SerializeTag(createdTag2));

            // Act
            var tag1 = _service.CreateProductTag(ELECTRONICS_TAG_TITLE);
            var tag2 = _service.CreateProductTag(CLOTHING_TAG_TITLE);

            // Assert
            Assert.That(tag1.Id, Is.Not.EqualTo(tag2.Id));
        }

        #endregion

        #region DeleteProductTag Tests

        [Test]
        public void DeleteProductTag_ExistingTag_RemovesTagFromRepository()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.DeleteProductTag(ELECTRONICS_TAG_TITLE));

            // Act
            _service.DeleteProductTag(ELECTRONICS_TAG_TITLE);

            // Assert
            _mockRepository.Verify(repo => repo.DeleteProductTag(ELECTRONICS_TAG_TITLE), Times.Once);
        }

        [Test]
        public void DeleteProductTag_ExistingTag_RemovesCorrectTag()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.DeleteProductTag(ELECTRONICS_TAG_TITLE));

            // Act
            _service.DeleteProductTag(ELECTRONICS_TAG_TITLE);

            // Assert
            var result = _service.GetAllProductTags();
            _mockRepository.Verify(repo => repo.DeleteProductTag(ELECTRONICS_TAG_TITLE), Times.Once);
            Assert.That(result.Any(t => t.DisplayTitle == ELECTRONICS_TAG_TITLE), Is.False);
        }

        [Test]
        public void DeleteProductTag_NonExistentTag_DoesNotChangeRepositoryCount()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.DeleteProductTag(FURNITURE_TAG_TITLE));
            _mockRepository.Setup(repo => repo.GetAllProductTagsRaw())
                           .Returns(SerializeTags(new List<ProductTag> { new ProductTag(TAG_ID_1, ELECTRONICS_TAG_TITLE) }));

            // Act
            _service.DeleteProductTag(FURNITURE_TAG_TITLE);

            // Assert
            var result = _service.GetAllProductTags();
            _mockRepository.Verify(repo => repo.DeleteProductTag(FURNITURE_TAG_TITLE), Times.Once);
            Assert.That(result.Count, Is.EqualTo(EXPECTED_SINGLE_TAG_COUNT));
        }

        [Test]
        public void DeleteProductTag_NonExistentTag_KeepsExistingTagUnchanged()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.DeleteProductTag(FURNITURE_TAG_TITLE));
            _mockRepository.Setup(repo => repo.GetAllProductTagsRaw())
                           .Returns(SerializeTags(new List<ProductTag> { new ProductTag(TAG_ID_1, ELECTRONICS_TAG_TITLE) }));

            // Act
            _service.DeleteProductTag(FURNITURE_TAG_TITLE);

            // Assert
            var result = _service.GetAllProductTags();
            _mockRepository.Verify(repo => repo.DeleteProductTag(FURNITURE_TAG_TITLE), Times.Once);
            Assert.That(result.First().DisplayTitle, Is.EqualTo(ELECTRONICS_TAG_TITLE));
        }

        [Test]
        public void DeleteProductTag_WithMultipleTagsWithSameTitle_RemovesAllMatchingTags()
        {
            // Arrange
            var duplicateTags = new List<ProductTag>
            {
                new ProductTag(TAG_ID_1, ELECTRONICS_TAG_TITLE),
                new ProductTag(TAG_ID_2, BOOKS_TAG_TITLE),
                new ProductTag(TAG_ID_3, ELECTRONICS_TAG_TITLE)
            };
            _mockRepository.Setup(repo => repo.GetAllProductTagsRaw())
                           .Returns(SerializeTags(duplicateTags));
            _mockRepository.Setup(repo => repo.DeleteProductTag(ELECTRONICS_TAG_TITLE));

            var remainingTags = new List<ProductTag>
            {
                new ProductTag(TAG_ID_2, BOOKS_TAG_TITLE)
            };
            _mockRepository.Setup(repo => repo.GetAllProductTagsRaw())
                           .Returns(SerializeTags(remainingTags));
            // Act
            _service.DeleteProductTag(ELECTRONICS_TAG_TITLE);

            // Assert
            var result = _service.GetAllProductTags();
            _mockRepository.Verify(repo => repo.DeleteProductTag(ELECTRONICS_TAG_TITLE), Times.Once);
            Assert.That(result.Count, Is.EqualTo(EXPECTED_SINGLE_TAG_COUNT));
        }

        [Test]
        public void DeleteProductTag_WithMultipleTagsWithSameTitle_KeepsNonMatchingTags()
        {
            // Arrange
            var duplicateTags = new List<ProductTag>
            {
                new ProductTag(TAG_ID_1, ELECTRONICS_TAG_TITLE),
                new ProductTag(TAG_ID_2, BOOKS_TAG_TITLE),
                new ProductTag(TAG_ID_3, ELECTRONICS_TAG_TITLE)
            };
            _mockRepository.Setup(repo => repo.GetAllProductTagsRaw())
                           .Returns(SerializeTags(duplicateTags));
            _mockRepository.Setup(repo => repo.DeleteProductTag(ELECTRONICS_TAG_TITLE));

            var remainingTags = new List<ProductTag>
            {
                new ProductTag(TAG_ID_2, BOOKS_TAG_TITLE)
            };
            _mockRepository.Setup(repo => repo.GetAllProductTagsRaw())
                           .Returns(SerializeTags(remainingTags));

            // Act
            _service.DeleteProductTag(ELECTRONICS_TAG_TITLE);

            // Assert
            var result = _service.GetAllProductTags();
            _mockRepository.Verify(repo => repo.DeleteProductTag(ELECTRONICS_TAG_TITLE), Times.Once);
            Assert.That(result.First().DisplayTitle, Is.EqualTo(BOOKS_TAG_TITLE));
        }

        [Test]
        public void DeleteProductTag_WithMultipleTagsWithSameTitle_RemovesAllInstancesOfMatchingTitle()
        {
            // Arrange
            var duplicateTags = new List<ProductTag>
            {
                new ProductTag(TAG_ID_1, ELECTRONICS_TAG_TITLE),
                new ProductTag(TAG_ID_2, BOOKS_TAG_TITLE),
                new ProductTag(TAG_ID_3, ELECTRONICS_TAG_TITLE)
            };
            _mockRepository.Setup(repo => repo.GetAllProductTagsRaw())
                           .Returns(SerializeTag(duplicateTags[1]));
            _mockRepository.Setup(repo => repo.DeleteProductTag(ELECTRONICS_TAG_TITLE));

            // Act
            _service.DeleteProductTag(ELECTRONICS_TAG_TITLE);

            // Assert
            var result = _service.GetAllProductTags();
            _mockRepository.Verify(repo => repo.DeleteProductTag(ELECTRONICS_TAG_TITLE), Times.Once);
            Assert.That(result.Any(t => t.DisplayTitle == ELECTRONICS_TAG_TITLE), Is.False);
        }

        #endregion

        #region Helper Methods

        private string SerializeTags(List<ProductTag> tags)
        {
            if (tags == null)
            {
                return "[]";
            }
            return JsonSerializer.Serialize(tags, _testJsonOptions);
        }

        private string SerializeTag(ProductTag tag)
        {
            if (tag == null)
            {
                return null;
            }
            return JsonSerializer.Serialize(tag, _testJsonOptions);
        }

        #endregion
    }
}