using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using Moq;
using Xunit;

namespace MarketMinds.Test.Services
{
    public class OrderHistoryServiceTest
    {
        private readonly Mock<IOrderHistoryRepository> _mockRepo;
        private readonly IOrderHistoryService _service;

        public OrderHistoryServiceTest()
        {
            _mockRepo = new Mock<IOrderHistoryRepository>();
            _service = new OrderHistoryService(_mockRepo.Object);
        }

        [Fact]
        public async Task GetProductsFromOrderHistoryAsync_ReturnsProductsFromRepository()
        {
            // Arrange
            int orderHistoryId = 1;
            var products = new List<Product> { new TestProduct { Id = 1 }, new TestProduct { Id = 2 } };
            _mockRepo.Setup(r => r.GetProductsFromOrderHistoryAsync(orderHistoryId)).ReturnsAsync(products);

            // Act
            var result = await _service.GetProductsFromOrderHistoryAsync(orderHistoryId);

            // Assert
            Assert.Equal(products, result);
        }

        [Fact]
        public async Task GetProductsFromOrderHistoryAsync_ThrowsArgumentException_WhenIdIsInvalid()
        {
            // Arrange
            int invalidId = 0;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetProductsFromOrderHistoryAsync(invalidId));
        }

        [Fact]
        public async Task CreateOrderHistoryAsync_ReturnsNewOrderHistoryId()
        {
            // Arrange
            int userId = 5;
            int expectedOrderHistoryId = 10;
            _mockRepo.Setup(r => r.CreateOrderHistoryAsync(userId)).ReturnsAsync(expectedOrderHistoryId);

            // Act
            var result = await _service.CreateOrderHistoryAsync(userId);

            // Assert
            Assert.Equal(expectedOrderHistoryId, result);
        }

        [Fact]
        public async Task CreateOrderHistoryAsync_ThrowsArgumentException_WhenUserIdIsInvalid()
        {
            // Arrange
            int invalidUserId = -1;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateOrderHistoryAsync(invalidUserId));
        }

        // Minimal test implementation for abstract Product
        private class TestProduct : Product
        {
            // Implement any required abstract members or properties if needed
        }
    }
}
