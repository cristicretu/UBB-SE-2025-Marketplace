using System;
using System.Collections.Generic;
using System.Linq;
using SharedClassLibrary.Domain;
using SharedClassLibrary.ProxyRepository;
using SharedClassLibrary.Shared;
using SharedClassLibrary.IRepository;
using SharedClassLibrary.Helper;

namespace SharedClassLibrary.Service
{
    public class NotificationContentService : INotificationContentService
    {
        private readonly INotificationRepository notificationRepository;

        public NotificationContentService()
        {
            this.notificationRepository = new NotificationProxyRepository(AppConfig.GetBaseApiUrl());
        }

        public string GetUnreadNotificationsCountText(int unreadCount)
        {
            return $"You've got #{unreadCount} unread notifications.";
        }

        public async Task<List<Notification>> GetNotificationsForUser(int recipientId)
        {
            return await notificationRepository.GetNotificationsForUser(recipientId);
        }

        public void MarkAsRead(int notificationId)
        {
            notificationRepository.MarkAsRead(notificationId);
        }

        public void AddNotification(Notification notification)
        {
            notificationRepository.AddNotification(notification);
        }
    }
}