using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MarketMinds.Shared.Helper;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using Microsoft.UI.Dispatching;

namespace MarketMinds.ViewModels
{
    /// <summary>
    /// Manages notifications by loading, updating, and marking them as read.
    /// </summary>
    public class NotificationViewModel : INotifyPropertyChanged, INotificationViewModel
    {
        private ObservableCollection<Notification> notifications;
        private INotificationContentService notificationContentService;
        private int unreadCount;
        private bool isLoading;
        private int currentUserId;

        /// <summary>
        /// Occurs when a popup message should be displayed.
        /// </summary>
        public event Action<string> ShowPopup;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationViewModel"/> class.
        /// </summary>
        /// <param name="currentUserId">The identifier of the current user for notification retrieval.</param>
        public NotificationViewModel(int userId, bool autoLoad = true)
        {
            Notifications = new ObservableCollection<Notification>();
            this.currentUserId = userId;
            this.notificationContentService = new NotificationContentService();
            MarkAsReadCommand = new NotificationRelayCommand<int>(async (id) => await MarkAsReadAsync(id));
            ClearAllCommand = new RelayCommand(ClearAllNotifications);

            if (autoLoad)
            {
                _ = LoadUnreadNotificationsAsync(currentUserId);
            }
        }

        /// <summary>
        /// Gets or sets the collection of notifications.
        /// </summary>
        public ObservableCollection<Notification> Notifications
        {
            get => notifications;
            set
            {
                notifications = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the count of unread notifications.
        /// </summary>
        public int UnreadCount
        {
            get => unreadCount;
            set
            {
                unreadCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UnReadNotificationsCountText));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether notifications are currently being loaded.
        /// </summary>
        public bool IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the command for marking a notification as read.
        /// </summary>
        public ICommand MarkAsReadCommand { get; }

        /// <summary>
        /// Gets the command for clearing all notifications.
        /// </summary>
        public ICommand ClearAllCommand { get; }

        /// <summary>
        /// Clears all notifications by marking them as read and removing them from the collection
        /// </summary>
        /// <returns></returns>
        public async Task ClearAllNotifications()
        {
            if (Notifications.Count == 0)
            {
                return;
            }
            try
            {
                var notificationsToClear = Notifications.ToList();

                await this.notificationContentService.MarkAllAsRead(currentUserId);

                DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
                {
                    Notifications.Clear();
                    UnreadCount = 0;
                    OnPropertyChanged(nameof(UnReadNotificationsCountText));
                });
                ShowPopup?.Invoke("All notifications cleared.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing notifications: {ex.Message}");
                ShowPopup?.Invoke($"Failed to clear notifications: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously loads notifications for the specified recipient.
        /// </summary>
        /// <param name="recipientId">The recipient user identifier.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task LoadUnreadNotificationsAsync(int recipientId)
        {
            try
            {
                IsLoading = true;

                var loadedNotifications = await this.notificationContentService.GetUnreadNotificationsForUser(recipientId);

                Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
                {
                    Notifications.Clear();
                    foreach (var notification in loadedNotifications)
                    {
                        Notifications.Add(notification);
                    }
                    UnreadCount = Notifications.Count(n => !n.IsRead);
                    OnPropertyChanged(nameof(UnReadNotificationsCountText));
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading notifications: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Asynchronously marks the specified notification as read.
        /// </summary>
        /// <param name="notificationId">The identifier of the notification to mark as read.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task MarkAsReadAsync(int notificationId)
        {
            try
            {
                await this.notificationContentService.MarkAllAsRead(notificationId);

                // Update the local notification state
                var notification = Notifications.FirstOrDefault(n => n.NotificationID == notificationId);
                if (notification != null)
                {
                    notification.IsRead = true;
                    UnreadCount = Notifications.Count(n => !n.IsRead);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error marking notification as read: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously adds a new notification and, if the recipient matches the current user, inserts it at the beginning of the collection.
        /// </summary>
        /// <param name="notification">The notification to add.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="notification"/> is <c>null</c>.</exception>
        public async Task AddNotificationAsync(Notification notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            try
            {
                await this.notificationContentService.AddNotification(notification);

                if (notification.RecipientID == currentUserId)
                {
                    Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
                    {
                        Notifications.Insert(0, notification);
                        UnreadCount++;
                        OnPropertyChanged(nameof(UnReadNotificationsCountText));
                        ShowPopup?.Invoke("Notification sent!");
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed; optional.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Gets a formatted text representing the count of unread notifications.
        /// </summary>
        public string UnReadNotificationsCountText
        {
            get => "You've got #" + unreadCount + " unread notifications.";
        }
    }
}
