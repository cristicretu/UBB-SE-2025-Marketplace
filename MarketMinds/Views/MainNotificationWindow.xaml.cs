using System.Diagnostics.CodeAnalysis;
using MarketMinds.Shared.Models;
using MarketMinds.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace MarketMinds.Views
{
    [ExcludeFromCodeCoverage]

    public sealed partial class MainNotificationWindow : Page
    {
        /// <summary>
        /// The page for the main notification window
        /// </summary>
        public NotificationViewModel ViewModel { get; set; }
        private int CurrentUserId { get; set; }

        public MainNotificationWindow()
        {
            this.InitializeComponent();

            CurrentUserId = UserSession.CurrentUserId ?? 0;
            ViewModel = new NotificationViewModel(CurrentUserId);
            RootGrid.DataContext = ViewModel;

            Loaded += MainNotificationWindow_Loaded;
        }

        /// <summary>
        /// Handles the page loaded event. Loads the notifications for the current user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MainNotificationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadUnreadNotificationsAsync(CurrentUserId);
        }

        /// <summary>
        /// Called when page is navigated to
        /// </summary>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.LoadUnreadNotificationsAsync(CurrentUserId);
        }

        /// <summary>
        /// Handles the selection changed event for the notification list. If a notification is selected,
        /// it navigates to the secondary notification page with the notification details and marks the notification as read
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void NotificationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (notificationList.SelectedItem is Notification selectedNotification)
            {
                await ViewModel.MarkAsReadAsync(selectedNotification.NotificationID);

                // Get the frame we're in (could be the notifications frame in the popup)
                Frame currentFrame = null;

                // Find the parent frame
                DependencyObject parent = this;
                while (parent != null && !(parent is Frame))
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                if (parent is Frame frame)
                {
                    currentFrame = frame;
                }
                else if (this.Frame != null)
                {
                    currentFrame = this.Frame;
                }

                // Navigate to secondary notification page
                if (currentFrame != null)
                {
                    currentFrame.Navigate(typeof(SecondaryNotificationWindow), selectedNotification);
                }

                notificationList.SelectedItem = null;
            }
        }
    }
}