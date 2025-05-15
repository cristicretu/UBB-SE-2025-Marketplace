using System.Diagnostics.CodeAnalysis;
using SharedClassLibrary.Domain;
using MarketPlace924.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MarketPlace924
{
    [ExcludeFromCodeCoverage]

    public sealed partial class MainNotificationWindow : Window
    {
        /// <summary>
        /// The page for the main notification window
        /// </summary>
        private const int CurrentUserId = 1;

        public NotificationViewModel ViewModel = new NotificationViewModel(CurrentUserId);

        public MainNotificationWindow()
        {
            this.InitializeComponent();
            this.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(400, 200, 1080, 600));

            RootPanel.DataContext = ViewModel;

            Activated += MainNotificationWindow_Activated;
        }

        /// <summary>
        /// Handles the window activation event. If the window is activated, it loads the notifications for the current user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void MainNotificationWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState != WindowActivationState.Deactivated)
            {
                await ViewModel.LoadNotificationsAsync(CurrentUserId);
            }
        }

        /// <summary>
        /// Handles the selection changed event for the notification list. If a notification is selected,
        /// it opens a secondary window with the notification details and marks the notification as read
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void NotificationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (notificationList.SelectedItem is Notification selectedNotification)
            {
                var secondaryWindow = new SecondaryNotificationWindow(selectedNotification);
                secondaryWindow.Activate();

                await ViewModel.MarkAsReadAsync(selectedNotification.NotificationID);

                notificationList.SelectedItem = null;
            }

            this.Close();
        }

        /// <summary>
        /// Handles the click event for the back button. It closes the current window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Need to create main Window and activate it;
            this.Close();
        }
    }
}