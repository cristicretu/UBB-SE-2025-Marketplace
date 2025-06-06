﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MarketMinds.Shared.Models;

namespace MarketMinds.ViewModels
{
    public interface INotificationViewModel
    {
        bool IsLoading { get; set; }
        ICommand MarkAsReadCommand { get; }
        ObservableCollection<Notification> Notifications { get; set; }
        int UnreadCount { get; set; }
        string UnReadNotificationsCountText { get; }

        event PropertyChangedEventHandler PropertyChanged;
        event Action<string> ShowPopup;

        Task AddNotificationAsync(Notification notification);
        Task LoadUnreadNotificationsAsync(int recipientId);
        Task MarkAsReadAsync(int notificationId);
    }
}