using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services
{
    public interface INotificationContentService
    {
        string GetUnreadNotificationsCountText(int unreadCount);
        Task<List<Notification>> GetNotificationsForUser(int recipientId);

        Task<List<Notification>> GetUnreadNotificationsForUser(int recipientId);
        Task MarkAllAsRead(int notificationId);
        Task AddNotification(Notification notification);
    }
}