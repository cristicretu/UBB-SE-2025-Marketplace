using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.IRepository
{
    public interface INotificationRepository
    {
        void AddNotification(Notification notification);
        Task<List<Notification>> GetNotificationsForUser(int recipientId);
        void MarkAsRead(int notificationId);
    }
}