using System.Diagnostics.CodeAnalysis;
using MarketMinds.Shared.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace MarketMinds.Views
{
    [ExcludeFromCodeCoverage]
    public sealed partial class SecondaryNotificationWindow : Page
    {
        /// <summary>
        /// The page for the secondary notification view
        /// </summary>
        public Notification SelectedNotification { get; private set; }

        public SecondaryNotificationWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Called when this page is navigated to
        /// </summary>
        /// <param name="e">Event args containing the parameter (notification)</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Notification notification)
            {
                this.SelectedNotification = notification;
                this.Populate();
            }
        }

        /// <summary>
        /// Populates the secondary notification page with the selected notification's details
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
            // Get the current frame
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

            // Navigate back to the main notification page
            if (currentFrame != null && currentFrame.CanGoBack)
            {
                currentFrame.GoBack();
            }
        }
    }
}