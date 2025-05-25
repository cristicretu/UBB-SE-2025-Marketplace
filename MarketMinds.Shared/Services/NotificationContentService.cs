using MarketMinds.Shared.Models;
using MarketMinds.Shared.ProxyRepository;

using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Helper;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Shared.Services
{
    public class NotificationContentService : INotificationContentService
    {
        private readonly INotificationRepository notificationRepository;

        public NotificationContentService()
        {
            string baseUrl = AppConfig.GetBaseApiUrl();
            this.notificationRepository = new NotificationProxyRepository(baseUrl);
        }

        /// <summary>
        /// Gets a formatted text describing the count of unread notifications
        /// </summary>
        /// <param name="userId">The user ID to get unread count for</param>
        /// <returns>Formatted text with unread count</returns>
        public string GetUnreadNotificationsCountText(int userId)
        {
            try
            {
                // Get the actual count of unread notifications
                var unreadCount = GetUnreadNotificationsForUser(userId).Result.Count;
                if (unreadCount == 0)
                    return "No unread notifications";
                return $"{unreadCount} unread notification{(unreadCount != 1 ? "s" : "")}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetUnreadNotificationsCountText: Exception: {ex.Message}");
                return "0 unread notifications";
            }
        }

        public int GetUnreadCount(int userId)
        {
            try
            {
                // Get the actual count of unread notifications
                return GetUnreadNotificationsForUser(userId).Result.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetUnreadCount: Exception: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Gets all notifications for a user
        /// </summary>
        /// <param name="recipientId">The recipient user ID</param>
        /// <returns>List of all notifications for the user</returns>
        public async Task<List<Notification>> GetNotificationsForUser(int recipientId)
        {
            try
            {
                return await notificationRepository.GetNotificationsForUser(recipientId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetNotificationsForUser: Exception: {ex.Message}");
                return new List<Notification>();
            }
        }

        /// <summary>
        /// Gets only unread notifications for a user
        /// </summary>
        /// <param name="recipientId">The recipient user ID</param>
        /// <returns>List of unread notifications</returns>
        public async Task<List<Notification>> GetUnreadNotificationsForUser(int recipientId)
        {
            try
            {
                var allNotifications = await notificationRepository.GetNotificationsForUser(recipientId);

                if (allNotifications == null)
                {
                    return new List<Notification>();
                }

                // Filter for only unread notifications and sort by timestamp (newest first)
                return allNotifications
                    .Where(n => !n.IsRead)
                    .OrderByDescending(n => n.Timestamp)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetUnreadNotificationsForUser: Exception: {ex.Message}");
                return new List<Notification>();
            }
        }

        /// <summary>
        /// Marks all notifications as read for a specific user
        /// </summary>
        /// <param name="userId">The user ID whose notifications to mark as read</param>
        public async Task MarkAllAsRead(int userId)
        {
            try
            {
                await notificationRepository.MarkAsRead(userId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in NotificationContentService.MarkAllAsRead: {ex.Message}");
                throw; // Rethrow to let the controller handle it
            }
        }

        /// <summary>
        /// Marks a specific notification as read
        /// </summary>
        /// <param name="notificationId">The ID of the notification to mark as read</param>
        public async Task MarkNotificationAsRead(int notificationId)
        {
            try
            {
                // This needs to be implemented in the repository
                await notificationRepository.MarkNotificationAsRead(notificationId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in NotificationContentService.MarkNotificationAsRead: {ex.Message}");
                throw; // Rethrow to let the controller handle it
            }
        }

        /// <summary>
        /// Clears (deletes) all notifications for a user
        /// </summary>
        /// <param name="userId">The user ID whose notifications to clear</param>
        public async Task ClearAllNotifications(int userId)
        {
            try
            {
                // This needs to be implemented in the repository
                await notificationRepository.ClearAllNotifications(userId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in NotificationContentService.ClearAllNotifications: {ex.Message}");
                throw; // Rethrow to let the controller handle it
            }
        }

        /// <summary>
        /// Adds a new notification
        /// </summary>
        /// <param name="notification">The notification to add</param>
        public async Task AddNotification(Notification notification)
        {
            try
            {
                await notificationRepository.AddNotification(notification);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in NotificationContentService.AddNotification: {ex.Message}");
                throw; // Rethrow to let the controller handle it
            }
        }
    }
}