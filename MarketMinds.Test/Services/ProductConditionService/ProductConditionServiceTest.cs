using MarketMinds.Shared.Services.ProductConditionService;
using MarketMinds.Shared.IRepository;

using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using System.Net.Http;
using System;

namespace MarketMinds.Test.Services.ProductConditionService
{
    [TestFixture]
    public class ProductConditionServiceTest
    {
        // Constants to replace magic strings and numbers
        private const int FIRST_CONDITION_ID = 1;
        private const int SECOND_CONDITION_ID = 2;
        private const string NEW_CONDITION_TITLE = "New";
        private const string NEW_CONDITION_DESCRIPTION = "Never used";
        private const string USED_CONDITION_TITLE = "Used";
        private const string USED_CONDITION_DESCRIPTION = "Shows use marks";
        private const string CONDITION_TO_DELETE_TITLE = "Condition to Delete";
        private const string CONDITION_TO_DELETE_DESCRIPTION = "Description";
        private const int EXPECTED_SINGLE_ITEM = 1;
        private const int EXPECTED_TWO_ITEMS = 2;
        private const int EXPECTED_ZERO_ITEMS = 0;

        private  Mock<IProductConditionRepository> _mockRepository;
        private IProductConditionService _service;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new Mock<IProductConditionRepository>();
            _service = new MarketMinds.Shared.Services.ProductConditionService.ProductConditionService(_mockRepository.Object);
        }


        #region GetAllProductConditions Tests

        [Test]
        public void GetAllProductConditions_ReturnsCorrectNumberOfConditions()
        {
            // Arrange
            string fakeJsonResponse = $"[{{\"id\":{FIRST_CONDITION_ID},\"name\":\"{NEW_CONDITION_TITLE}\",\"description\":\"{NEW_CONDITION_DESCRIPTION}\"}}, {{\"id\":{SECOND_CONDITION_ID},\"name\":\"{USED_CONDITION_TITLE}\",\"description\":\"{USED_CONDITION_DESCRIPTION}\"}}]";
            _mockRepository.Setup(r => r.GetAllProductConditionsRaw()).Returns(fakeJsonResponse);

            // Act
            var result = _service.GetAllProductConditions();

            // Assert
            Assert.That(result.Count, Is.EqualTo(EXPECTED_TWO_ITEMS));
        }

        [Test]
        public void GetAllProductConditions_EmptyResponse_ReturnsEmptyList()
        {
            // Arrange
            string emptyJsonResponse = "[]";
            _mockRepository.Setup(r => r.GetAllProductConditionsRaw()).Returns(emptyJsonResponse);

            // Act
            var result = _service.GetAllProductConditions();

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetAllProductConditions_FirstConditionHasCorrectTitle()
        {
            // Arrange
            string fakeJsonResponse = $"[{{\"id\":{FIRST_CONDITION_ID},\"name\":\"{NEW_CONDITION_TITLE}\",\"description\":\"{NEW_CONDITION_DESCRIPTION}\"}}, {{\"id\":{SECOND_CONDITION_ID},\"name\":\"{USED_CONDITION_TITLE}\",\"description\":\"{USED_CONDITION_DESCRIPTION}\"}}]";
            _mockRepository.Setup(r => r.GetAllProductConditionsRaw()).Returns(fakeJsonResponse);

            // Act
            var result = _service.GetAllProductConditions();

            // Assert
            Assert.That(result[0].DisplayTitle, Contains.Substring(NEW_CONDITION_TITLE));
        }

        [Test]
        public void GetAllProductConditions_SecondConditionHasCorrectTitle()
        {
            // Arrange
            string fakeJsonResponse = $"[{{\"id\":{FIRST_CONDITION_ID},\"name\":\"{NEW_CONDITION_TITLE}\",\"description\":\"{NEW_CONDITION_DESCRIPTION}\"}}, {{\"id\":{SECOND_CONDITION_ID},\"name\":\"{USED_CONDITION_TITLE}\",\"description\":\"{USED_CONDITION_DESCRIPTION}\"}}]";
            _mockRepository.Setup(r => r.GetAllProductConditionsRaw()).Returns(fakeJsonResponse);

            // Act
            var result = _service.GetAllProductConditions();

            // Assert
            Assert.That(result[1].DisplayTitle, Is.EqualTo(USED_CONDITION_TITLE));
        }

        [Test]
        public void GetAllProductConditions_FirstConditionHasCorrectDescription()
        {
            // Arrange
            string fakeJsonResponse = $"[{{\"id\":{FIRST_CONDITION_ID},\"name\":\"{NEW_CONDITION_TITLE}\",\"description\":\"{NEW_CONDITION_DESCRIPTION}\"}}, {{\"id\":{SECOND_CONDITION_ID},\"name\":\"{USED_CONDITION_TITLE}\",\"description\":\"{USED_CONDITION_DESCRIPTION}\"}}]";
            _mockRepository.Setup(r => r.GetAllProductConditionsRaw()).Returns(fakeJsonResponse);

            // Act
            var result = _service.GetAllProductConditions();

            // Assert
            Assert.That(result[0].Description, Is.EqualTo(NEW_CONDITION_DESCRIPTION));
        }

        [Test]
        public void GetAllProductConditions_SecondConditionHasCorrectDescription()
        {
            // Arrange
            string fakeJsonResponse = $"[{{\"id\":{FIRST_CONDITION_ID},\"name\":\"{NEW_CONDITION_TITLE}\",\"description\":\"{NEW_CONDITION_DESCRIPTION}\"}}, {{\"id\":{SECOND_CONDITION_ID},\"name\":\"{USED_CONDITION_TITLE}\",\"description\":\"{USED_CONDITION_DESCRIPTION}\"}}]";
            _mockRepository.Setup(r => r.GetAllProductConditionsRaw()).Returns(fakeJsonResponse);

            // Act
            var result = _service.GetAllProductConditions();

            // Assert
            Assert.That(result[1].Description, Is.EqualTo(USED_CONDITION_DESCRIPTION));
        }

        [Test]
        public void GetAllProductConditions_RepositoryThrowsException_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllProductConditionsRaw()).Throws(new HttpRequestException("Network error"));

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _service.GetAllProductConditions());
            Assert.That(exception.Message, Contains.Substring("Error retrieving product conditions"));
        }

        #endregion

        #region CreateProductCondition Tests

        [Test]
        public void CreateProductCondition_ReturnsConditionWithCorrectTitle()
        {
            // Arrange
            string fakeJsonResponse = $"{{\"id\":{FIRST_CONDITION_ID},\"name\":\"{NEW_CONDITION_TITLE}\",\"description\":\"{NEW_CONDITION_DESCRIPTION}\"}}";
            _mockRepository.Setup(r => r.CreateProductConditionRaw(NEW_CONDITION_TITLE, NEW_CONDITION_DESCRIPTION))
                           .Returns(fakeJsonResponse);

            // Act
            var result = _service.CreateProductCondition(NEW_CONDITION_TITLE, NEW_CONDITION_DESCRIPTION);

            // Assert
            Assert.That(result.DisplayTitle, Is.EqualTo(NEW_CONDITION_TITLE));
        }

        [Test]
        public void CreateProductCondition_EmptyTitle_ThrowsArgumentException()
        {
            // Arrange
            string title = "   "; // Whitespace
            string description = "Test description";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _service.CreateProductCondition(title, description));
            Assert.That(exception.Message, Contains.Substring("title cannot be null or empty"));
        }

        [Test]
        public void CreateProductCondition_TitleTooLong_ThrowsArgumentException()
        {
            // Arrange
            string title = new string('A', 101); // 101 characters
            string description = "Test description";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _service.CreateProductCondition(title, description));
            Assert.That(exception.Message, Contains.Substring("title cannot exceed 100 characters"));
        }

        [Test]
        public void CreateProductCondition_DescriptionTooLong_ThrowsArgumentException()
        {
            // Arrange
            string title = "Test Title";
            string description = new string('A', 501); // 501 characters

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _service.CreateProductCondition(title, description));
            Assert.That(exception.Message, Contains.Substring("description cannot exceed 500 characters"));
        }

        [Test]
        public void CreateProductCondition_RepositoryThrowsException_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockRepository.Setup(r => r.CreateProductConditionRaw(It.IsAny<string>(), It.IsAny<string>()))
                           .Throws(new HttpRequestException("Network error"));

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                _service.CreateProductCondition("Test Title", "Test Description"));
            Assert.That(exception.Message, Contains.Substring("Error creating product condition"));
        }

        [Test]
        public void CreateProductCondition_ReturnsConditionWithCorrectDescription()
        {
            // Arrange
            string fakeJsonResponse = $"{{\"id\":{FIRST_CONDITION_ID},\"name\":\"{NEW_CONDITION_TITLE}\",\"description\":\"{NEW_CONDITION_DESCRIPTION}\"}}";
            _mockRepository.Setup(r => r.CreateProductConditionRaw(NEW_CONDITION_TITLE, NEW_CONDITION_DESCRIPTION))
                           .Returns(fakeJsonResponse);
            // Act
            var result = _service.CreateProductCondition(NEW_CONDITION_TITLE, NEW_CONDITION_DESCRIPTION);

            // Assert
            Assert.That(result.Description, Contains.Substring(NEW_CONDITION_DESCRIPTION));
        }

        [Test]
        public void CreateProductCondition_ReturnsConditionWithCorrectId()
        {
            // Arrange
            string fakeJsonResponse = $"{{\"id\":{FIRST_CONDITION_ID},\"name\":\"{NEW_CONDITION_TITLE}\",\"description\":\"{NEW_CONDITION_DESCRIPTION}\"}}";
            _mockRepository.Setup(r => r.CreateProductConditionRaw(NEW_CONDITION_TITLE, NEW_CONDITION_DESCRIPTION))
                          .Returns(fakeJsonResponse);

            // Act
            var result = _service.CreateProductCondition(NEW_CONDITION_TITLE, NEW_CONDITION_DESCRIPTION);

            // Assert
            Assert.That(result.Id, Is.EqualTo(FIRST_CONDITION_ID));
        }

        [Test]
        public void CreateProductCondition_AddsConditionToRepository()
        {
            // Arrange
            string fakeJsonResponse = $"{{\"id\":{FIRST_CONDITION_ID},\"name\":\"{NEW_CONDITION_TITLE}\",\"description\":\"{NEW_CONDITION_DESCRIPTION}\"}}";
            _mockRepository.Setup(r => r.CreateProductConditionRaw(NEW_CONDITION_TITLE, NEW_CONDITION_DESCRIPTION))
                           .Returns(fakeJsonResponse);

            // Act
            _service.CreateProductCondition(NEW_CONDITION_TITLE, NEW_CONDITION_DESCRIPTION);

            // Assert
            _mockRepository.Verify(r => r.CreateProductConditionRaw(NEW_CONDITION_TITLE, NEW_CONDITION_DESCRIPTION), Times.Once);
        }

        #endregion

        #region DeleteProductCondition Tests

        [Test]
        public void DeleteProductCondition_RemovesConditionFromRepository()
        {
            // Arrange
            _mockRepository.Setup(r => r.DeleteProductConditionRaw(CONDITION_TO_DELETE_TITLE));

            // Act
            _service.DeleteProductCondition(CONDITION_TO_DELETE_TITLE);

            // Assert
            _mockRepository.Verify(r => r.DeleteProductConditionRaw(CONDITION_TO_DELETE_TITLE), Times.Once);
        }

        [Test]
        public void DeleteProductCondition_EmptyTitle_ThrowsArgumentException()
        {
            // Arrange
            string title = "   "; // Whitespace

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _service.DeleteProductCondition(title));
            Assert.That(exception.Message, Contains.Substring("title cannot be null or empty"));
        }

        [Test]
        public void DeleteProductCondition_KeyNotFoundException_DoesNotThrowException()
        {
            // Arrange
            string title = "Non-existent Title";
            _mockRepository.Setup(r => r.DeleteProductConditionRaw(title))
                           .Throws(new KeyNotFoundException("Condition not found"));

            // Act & Assert - should not throw
            _service.DeleteProductCondition(title);
        }

        #endregion
    }
}