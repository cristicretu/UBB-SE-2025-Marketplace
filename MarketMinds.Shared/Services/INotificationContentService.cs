using MarketMinds.Shared.Models;

namespace SharedClassLibrary.Service
{
    public interface INotificationContentService
    {
        string GetUnreadNotificationsCountText(int unreadCount);
        Task<List<Notification>> GetNotificationsForUser(int recipientId);
        void MarkAsRead(int notificationId);
        void AddNotification(Notification notification);
    }
}