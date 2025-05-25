using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Services;
using Moq;
using Xunit;

namespace MarketMinds.Test.Services
{
    public class NotificationContentServiceTest
    {
        private readonly Mock<INotificationRepository> _mockRepo;
        private readonly INotificationContentService _service;

        public NotificationContentServiceTest()
        {
            _mockRepo = new Mock<INotificationRepository>();
            _service = new NotificationContentService(_mockRepo.Object);
        }

        [Fact]
        public void GetUnreadNotificationsCountText_ReturnsCorrectText()
        {
            // Arrange
            int unreadCount = 5;

            // Act
            var result = _service.GetUnreadNotificationsCountText(unreadCount);

            // Assert
            Assert.Equal("You've got #5 unread notifications.", result);
        }

        [Fact]
        public async Task GetNotificationsForUser_ReturnsNotificationsFromRepository()
        {
            // Arrange
            int userId = 1;
            var notifications = new List<Notification> { CreateNotification(isRead: false), CreateNotification(isRead: true) };
            _mockRepo.Setup(r => r.GetNotificationsForUser(userId)).ReturnsAsync(notifications);

            // Act
            var result = await _service.GetNotificationsForUser(userId);

            // Assert
            Assert.Equal(notifications, result);
        }

        [Fact]
        public async Task GetUnreadNotificationsForUser_ReturnsOnlyUnreadOrderedByTimestamp()
        {
            // Arrange
            int userId = 2;
            var now = DateTime.UtcNow;
            var notifications = new List<Notification>
            {
                CreateNotification(isRead: false, timestamp: now.AddMinutes(-10)),
                CreateNotification(isRead: true, timestamp: now.AddMinutes(-5)),
                CreateNotification(isRead: false, timestamp: now.AddMinutes(-1))
            };
            _mockRepo.Setup(r => r.GetNotificationsForUser(userId)).ReturnsAsync(notifications);

            // Act
            var result = await _service.GetUnreadNotificationsForUser(userId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.True(result[0].Timestamp < result[1].Timestamp); // Ordered ascending
            Assert.All(result, n => Assert.False(n.IsRead));
        }

        [Fact]
        public async Task GetUnreadNotificationsForUser_ReturnsEmptyList_WhenRepositoryReturnsNull()
        {
            // Arrange
            int userId = 3;
            _mockRepo.Setup(r => r.GetNotificationsForUser(userId)).ReturnsAsync((List<Notification>)null);

            // Act
            var result = await _service.GetUnreadNotificationsForUser(userId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task MarkAllAsRead_CallsRepository()
        {
            // Arrange
            int userId = 4;
            _mockRepo.Setup(r => r.MarkAsRead(userId)).Returns(Task.CompletedTask);

            // Act
            await _service.MarkAllAsRead(userId);

            // Assert
            _mockRepo.Verify(r => r.MarkAsRead(userId), Times.Once);
        }

        [Fact]
        public async Task AddNotification_CallsRepository()
        {
            // Arrange
            var notification = CreateNotification(isRead: false);
            _mockRepo.Setup(r => r.AddNotification(notification)).Returns(Task.CompletedTask);

            // Act
            await _service.AddNotification(notification);

            // Assert
            _mockRepo.Verify(r => r.AddNotification(notification), Times.Once);
        }

        // Helper to create a Notification instance (replace with your actual Notification implementation)
        private Notification CreateNotification(bool isRead, DateTime? timestamp = null)
        {
            // If Notification is abstract, use a test subclass or a mock.
            return new TestNotification
            {
                NotificationID = 1,
                RecipientID = 1,
                IsRead = isRead,
                Timestamp = timestamp ?? DateTime.UtcNow
            };
        }

        // Minimal test implementation for abstract Notification
        private class TestNotification : Notification
        {
            public override string Title => "Test";
            public override string Subtitle => "Test";
            public override string Content => "Test";
        }
    }
}
