using System.Collections.Generic;
using System.Data;
using SharedClassLibrary.Domain;

namespace SharedClassLibrary.IRepository
{
    public interface INotificationRepository
    {
        void AddNotification(Notification notification);
        Task<List<Notification>> GetNotificationsForUser(int recipientId);
        void MarkAsRead(int notificationId);
    }
}