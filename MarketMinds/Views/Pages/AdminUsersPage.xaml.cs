using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using MarketMinds.Shared.Models;
using MarketMinds.ViewModels.Admin;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ViewModelLayer.ViewModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace MarketMinds.Views.Pages
{
    public sealed partial class AdminUsersPage : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdminUsersPage"/> class.
        /// </summary>
        public AdminUsersPage()
        {
            this.InitializeComponent();
            this.ViewModel = MarketMinds.App.AdminViewModel;
            _ = this.ViewModel.LoadDataAsync();
        }

        /// <summary>
        /// Gets or sets the view model for this view.
        /// </summary>
        public IAdminViewModel ViewModel
        {
            get => (IAdminViewModel)this.DataContext;
            set => this.DataContext = value;
        }

        private async void TrackOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var contentDialog = new ContentDialog
            {
                Title = "Enter Tracked Order ID",
                PrimaryButtonText = "Confirm",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            TextBox inputTextBox = new TextBox { PlaceholderText = "Enter Tracked Order ID" };
            contentDialog.Content = inputTextBox;

            var result = await contentDialog.ShowAsync();
            bool parseSuccessful = int.TryParse(inputTextBox.Text, out int trackedOrderID);

            if (result == ContentDialogResult.Primary && parseSuccessful)
            {
                try
                {
                    // Validate that the tracked order exists
                    var trackedOrder = await App.TrackedOrderViewModel.GetTrackedOrderByIDAsync(trackedOrderID);
                    if (trackedOrder == null)
                    {
                        await ShowErrorDialog($"No tracked order found with ID {trackedOrderID}. Please verify the order ID and try again.");
                        return;
                    }

                    var trackedOrderWindow = new TrackedOrderWindow();
                    var trackedOrderControlPage = new TrackedOrderControlPage();
                    trackedOrderControlPage.SetTrackedOrderID(trackedOrderID);
                    trackedOrderWindow.Content = trackedOrderControlPage;
                    trackedOrderWindow.Activate();
                }
                catch (Exception ex)
                {
                    await ShowErrorDialog($"Error loading tracked order: {ex.Message}");
                }
            }
            else if (result == ContentDialogResult.Primary && !parseSuccessful)
            {
                await ShowErrorDialog("Please enter a valid order ID number!");
            }
        }

        private async Task ShowErrorDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
