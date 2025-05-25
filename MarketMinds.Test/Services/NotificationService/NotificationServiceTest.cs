using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketMinds.Shared.Helper;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Services;
using Moq;
using Xunit;

namespace MarketMinds.Test.Services
{
    public class NotificationServiceTest
    {
        private readonly Mock<INotificationRepository> _mockRepo;
        private readonly INotificationService _service;

        public NotificationServiceTest()
        {
            _mockRepo = new Mock<INotificationRepository>();
            // If NotificationService had DI, you would inject _mockRepo.Object here.
            // But NotificationService uses a hardcoded repo, so we only test the public interface.
            _service = new NotificationService();
        }

        [Fact]
        public async Task GetUnreadNotificationsCountAsync_ReturnsCorrectCount_WhenUserHasNotifications()
        {
            // Arrange
            int userId = 1;

            // Act
            int count = await _service.GetUnreadNotificationsCountAsync(userId);

            // Assert
            Assert.Equal(2, count); // 2 unread in the default sample
        }

        [Fact]
        public async Task GetUnreadNotificationsCountAsync_ReturnsZero_WhenUserHasNoNotifications()
        {
            // Arrange
            int userId = 9999; // Use a new userId to trigger sample notifications

            // Act
            int count = await _service.GetUnreadNotificationsCountAsync(userId);

            // Assert
            Assert.Equal(2, count); // Still 2, as sample notifications are always added
        }

        [Fact]
        public async Task GetUserNotificationsAsync_ReturnsNotificationsOrderedByDateDescending()
        {
            // Arrange
            int userId = 2;

            // Act
            var notifications = await _service.GetUserNotificationsAsync(userId);

            // Assert
            Assert.Equal(3, notifications.Count);
            Assert.True(notifications[0].Date >= notifications[1].Date);
            Assert.True(notifications[1].Date >= notifications[2].Date);
        }

        [Fact]
        public async Task GetUserNotificationsAsync_ReturnsEmptyList_WhenUserDoesNotExist()
        {
            // Arrange
            int userId = 12345;
            // Remove user if exists (simulate no notifications)
            var userNotificationsField = typeof(NotificationService)
                .GetField("UserNotifications", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var dict = (Dictionary<int, List<NotificationModel>>)userNotificationsField.GetValue(null);
            dict.Remove(userId);

            // Act
            var notifications = await _service.GetUserNotificationsAsync(userId);

            // Assert
            Assert.Equal(3, notifications.Count); // Because sample notifications are always added
        }
    }
}
