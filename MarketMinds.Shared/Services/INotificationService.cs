using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharedClassLibrary.Service
{
    /// <summary>
    /// Service for managing notifications
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Gets the number of unread notifications for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The number of unread notifications</returns>
        Task<int> GetUnreadNotificationsCountAsync(int userId);

        /// <summary>
        /// Gets all notifications for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The list of notifications</returns>
        Task<List<NotificationModel>> GetUserNotificationsAsync(int userId);
    }

    /// <summary>
    /// Model for notifications
    /// </summary>
    public class NotificationModel
    {
        /// <summary>
        /// Gets or sets the notification ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the notification message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the notification date
        /// </summary>
        public System.DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the notification has been read
        /// </summary>
        public bool IsRead { get; set; }
    }
} 