using MarketMinds.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketMinds.Shared.IRepository
{
    public interface INotificationRepository
    {
        /// <summary>
        /// Adds a new notification to the repository
        /// </summary>
        /// <param name="notification">The notification to add</param>
        Task AddNotification(Notification notification);

        /// <summary>
        /// Gets all notifications for a specific user
        /// </summary>
        /// <param name="recipientId">The recipient user ID</param>
        /// <returns>List of notifications for the user</returns>
        Task<List<Notification>> GetNotificationsForUser(int recipientId);

        /// <summary>
        /// Marks all notifications for a user as read
        /// </summary>
        /// <param name="userId">The user ID whose notifications to mark as read</param>
        Task MarkAsRead(int userId);

        /// <summary>
        /// Marks a specific notification as read
        /// </summary>
        /// <param name="notificationId">The ID of the notification to mark as read</param>
        Task MarkNotificationAsRead(int notificationId);

        /// <summary>
        /// Clears (deletes) all notifications for a user
        /// </summary>
        /// <param name="userId">The user ID whose notifications to clear</param>
        Task ClearAllNotifications(int userId);
    }
}