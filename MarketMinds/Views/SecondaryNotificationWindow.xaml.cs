using System.Diagnostics.CodeAnalysis;
using SharedClassLibrary.Domain;
using Microsoft.UI.Xaml;
using MarketPlace924.Services;
using SharedClassLibrary.Service;

namespace MarketPlace924
{
    [ExcludeFromCodeCoverage]
    public sealed partial class SecondaryNotificationWindow : Window
    {
        /// <summary>
        /// The window for the secondary notification view
        /// </summary>
        public Notification SelectedNotification { get; }

        public SecondaryNotificationWindow(Notification notification)
        {
            this.InitializeComponent();
            this.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(400, 200, 1080, 600));
            this.SelectedNotification = notification;
            contractFileButton.IsEnabled = false;
            this.Populate();
        }

        /// <summary>
        /// Populates the secondary notification window with the selected notification's details
        /// </summary>
        private void Populate()
        {
            selectedNotificationTitle.Text = this.SelectedNotification.Title;
            selectedNotificationContent.Text = this.SelectedNotification.Content;
        }

        /// <summary>
        /// Handles the click event for the back button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainNotificationWindow();
            mainWindow.Activate();
            this.Close();
        }

        private void GoToContractFile(object sender, RoutedEventArgs e)
        {
        }
    }
}