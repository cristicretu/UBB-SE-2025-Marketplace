using MarketMinds.Shared.Helper;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.ProxyRepository;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Shared.Services
{
    /// <summary>
    /// Service for managing notifications -> THIS ISN'T BEING USED !! GO TO NotificationContentService
    /// </summary>
    public class NotificationService : INotificationService
    {
        // This is a placeholder implementation. In a real app, this would use a database
        private static readonly Dictionary<int, List<NotificationModel>> UserNotifications = new();
        private readonly INotificationRepository notificationRepository;

        public NotificationService()
        {
            // change back to the original line when deploying

            // this.notificationRepository = new NotificationProxyRepository(AppConfig.GetBaseApiUrl());
            this.notificationRepository = new NotificationProxyRepository("http://localhost:5001");
        }

        /// <summary>
        /// Gets the number of unread notifications for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The number of unread notifications</returns>
        public Task<int> GetUnreadNotificationsCountAsync(int userId)
        {
            if (!UserNotifications.TryGetValue(userId, out var notifications))
            {
                // Add some sample notifications for demo purposes
                var sampleNotifications = new List<NotificationModel>
                {
                    new NotificationModel
                    {
                        Id = 1,
                        Message = "A product on your wishlist is now available",
                        Date = DateTime.Now.AddDays(-1),
                        IsRead = false
                    },
                    new NotificationModel
                    {
                        Id = 2,
                        Message = "You've moved up in the waitlist for Product X",
                        Date = DateTime.Now.AddHours(-3),
                        IsRead = false
                    },
                    new NotificationModel
                    {
                        Id = 3,
                        Message = "Your order has been shipped",
                        Date = DateTime.Now.AddDays(-2),
                        IsRead = true
                    }
                };

                UserNotifications[userId] = sampleNotifications;
                notifications = sampleNotifications;
            }

            return Task.FromResult(notifications.Count(n => !n.IsRead));
        }

        /// <summary>
        /// Gets all notifications for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The list of notifications</returns>
        public async Task<List<NotificationModel>> GetUserNotificationsAsync(int userId)
        {
            // Make sure we've initialized notifications
            await GetUnreadNotificationsCountAsync(userId);

            if (UserNotifications.TryGetValue(userId, out var notifications))
            {
                return notifications.OrderByDescending(n => n.Date).ToList();
            }

            return new List<NotificationModel>();
        }
    }
} 