using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.IRepository
{
    public interface INotificationRepository
    {
        Task AddNotification(Notification notification);
        Task<List<Notification>> GetNotificationsForUser(int recipientId);
        Task MarkAsRead(int notificationId);
    }
}