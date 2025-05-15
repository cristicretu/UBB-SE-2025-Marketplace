using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharedClassLibrary.Service
{
    /// <summary>
    /// Service for managing notifications
    /// </summary>
    public class NotificationService : INotificationService
    {
        // This is a placeholder implementation. In a real app, this would use a database
        private static readonly Dictionary<int, List<NotificationModel>> UserNotifications = new();

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