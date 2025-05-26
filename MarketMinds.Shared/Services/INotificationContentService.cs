using MarketMinds.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketMinds.Shared.Services
{
    public interface INotificationContentService
    {
        /// <summary>
        /// Gets a formatted text describing the count of unread notifications
        /// </summary>
        string GetUnreadNotificationsCountText(int userId);

        int GetUnreadCount(int userId);
        
        /// <summary>
        /// Gets all notifications for a user
        /// </summary>
        Task<List<Notification>> GetNotificationsForUser(int recipientId);
        
        /// <summary>
        /// Gets only unread notifications for a user
        /// </summary>
        Task<List<Notification>> GetUnreadNotificationsForUser(int recipientId);
        
        /// <summary>
        /// Marks all notifications as read for a specific user
        /// </summary>
        Task MarkAllAsRead(int userId);
        
        /// <summary>
        /// Marks a specific notification as read
        /// </summary>
        Task MarkNotificationAsRead(int notificationId);
        
        /// <summary>
        /// Clears (deletes) all notifications for a user
        /// </summary>
        Task ClearAllNotifications(int userId);
        
        /// <summary>
        /// Adds a new notification
        /// </summary>
        Task AddNotification(Notification notification);
    }
}