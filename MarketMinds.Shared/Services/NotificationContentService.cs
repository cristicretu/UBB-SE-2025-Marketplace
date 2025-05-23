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

        public string GetUnreadNotificationsCountText(int unreadCount)
        {
            return $"You've got #{unreadCount} unread notifications.";
        }

        public async Task<List<Notification>> GetNotificationsForUser(int recipientId)
        {
            return await notificationRepository.GetNotificationsForUser(recipientId);
        }

        public async Task<List<Notification>> GetUnreadNotificationsForUser(int recipientId)
        {
            try
            {
                var allNotificationsTask = notificationRepository.GetNotificationsForUser(recipientId);
                var allNotifications = await allNotificationsTask;

                if (allNotifications == null)
                {
                    return new List<Notification>();
                }

                List<Notification> allUnreadNotifications = new List<Notification>();
                foreach (var notification in allNotifications)
                {
                    if (!notification.IsRead)
                    {
                        allUnreadNotifications.Add(notification);
                    }
                }
                allUnreadNotifications = allUnreadNotifications.OrderBy(n => n.Timestamp).ToList();

                return allUnreadNotifications;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetUnreadNotificationsForUser: Exception: {ex.Message}");
                return new List<Notification>();
            }
        }

        public async Task MarkAllAsRead(int userID)
        {
            try
            {
                await notificationRepository.MarkAsRead(userID);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in NotificationContentService.MarkAsRead: {ex.Message}");
            }
        }

        public async Task AddNotification(Notification notification)
        {
            try
            {
                await notificationRepository.AddNotification(notification);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in NotificationContentService.AddNotification: {ex.Message}");
            }
        }
    }
}